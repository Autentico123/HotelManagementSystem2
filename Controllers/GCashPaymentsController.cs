using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem2.Data;
using HotelManagementSystem2.Models;
using HotelManagementSystem2.Models.ViewModels;

namespace HotelManagementSystem2.Controllers
{
    [Authorize]
    public class GCashPaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public GCashPaymentsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: GCashPayments/Pay/5
        [HttpGet]
        public async Task<IActionResult> Pay(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Check authorization
            if (!User.IsInRole("Admin") && booking.CreatedBy != User.Identity?.Name)
            {
                return Forbid();
            }

            // Get system settings for admin GCash number
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                // Create default settings if none exist
                settings = new SystemSettings();
                _context.SystemSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            var model = new GCashPaymentWithVerificationViewModel
            {
                BookingId = booking.Id,
                BookingNumber = booking.BookingNumber,
                TotalAmount = booking.TotalAmount,
                AmountPaid = booking.AmountPaid,
                RemainingBalance = booking.Balance,
                GuestName = booking.GuestName,
                Amount = booking.Balance, // Default to remaining balance
                SenderGCashNumber = booking.GuestPhone, // Pre-fill with guest phone
                AdminGCashNumber = settings.AdminGCashNumber,
                AdminGCashAccountName = settings.AdminGCashAccountName,
                AdminGCashQRCode = settings.GCashQRCodeUrl,
                PaymentInstructions = settings.PaymentInstructions
            };

            return View(model);
        }

        // POST: GCashPayments/Pay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(GCashPaymentWithVerificationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var booking = await _context.Bookings.FindAsync(model.BookingId);
                if (booking == null)
                {
                    return NotFound();
                }

                // Check authorization
                if (!User.IsInRole("Admin") && booking.CreatedBy != User.Identity?.Name)
                {
                    return Forbid();
                }

                // Validate payment amount
                if (model.Amount > booking.Balance)
                {
                    ModelState.AddModelError("Amount", "Payment amount cannot exceed the remaining balance.");
                    return View(await RepopulateViewModel(model));
                }

                // Handle file upload
                string? proofImagePath = null;
                if (model.ProofImage != null && model.ProofImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "gcash-proofs");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{model.ProofImage.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProofImage.CopyToAsync(fileStream);
                    }

                    proofImagePath = $"/uploads/gcash-proofs/{uniqueFileName}";
                }

                // Get admin GCash number from settings
                var settings = await _context.SystemSettings.FirstOrDefaultAsync();

                // Create GCash payment record
                var payment = new GCashPayment
                {
                    BookingId = model.BookingId,
                    SenderGCashNumber = model.SenderGCashNumber,
                    SenderAccountName = model.SenderAccountName,
                    ReceiverGCashNumber = settings?.AdminGCashNumber ?? "Not Set",
                    Amount = model.Amount,
                    ReferenceNumber = model.ReferenceNumber ?? GenerateReferenceNumber(),
                    ProofImageUrl = proofImagePath,
                    Status = PaymentStatus.Pending,
                    PaymentDate = DateTime.UtcNow,
                    Notes = model.Notes
                };

                _context.GCashPayments.Add(payment);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"GCash payment of ?{model.Amount:N2} submitted successfully! Reference: {payment.ReferenceNumber}. Waiting for admin verification.";
                return RedirectToAction("Details", "Bookings", new { id = booking.Id });
            }

            return View(await RepopulateViewModel(model));
        }

        // GET: GCashPayments/PendingPayments
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PendingPayments()
        {
            var pendingPayments = await _context.GCashPayments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Room)
                .Where(p => p.Status == PaymentStatus.Pending)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return View(pendingPayments);
        }

        // GET: GCashPayments/AllPayments
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllPayments()
        {
            var payments = await _context.GCashPayments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Room)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return View(payments);
        }

        // GET: GCashPayments/Verify/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Verify(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.GCashPayments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Room)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

            var model = new VerifyPaymentViewModel
            {
                PaymentId = payment.Id,
                Payment = payment
            };

            return View(model);
        }

        // POST: GCashPayments/Verify
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Verify(VerifyPaymentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var payment = await _context.GCashPayments
                    .Include(p => p.Booking)
                    .FirstOrDefaultAsync(p => p.Id == model.PaymentId);

                if (payment == null)
                {
                    return NotFound();
                }

                if (model.IsApproved)
                {
                    // Approve payment
                    payment.Status = PaymentStatus.Verified;
                    payment.VerifiedBy = User.Identity?.Name ?? "Admin";
                    payment.VerifiedDate = DateTime.UtcNow;

                    // Update booking
                    payment.Booking.AmountPaid += payment.Amount;
                    payment.Booking.Balance = payment.Booking.TotalAmount - payment.Booking.AmountPaid;
                    payment.Booking.PaymentMethod = "GCash";
                    payment.Booking.GCashNumber = payment.SenderGCashNumber;
                    payment.Booking.GCashAccountName = payment.SenderAccountName;
                    payment.Booking.GCashReferenceNumber = payment.ReferenceNumber;
                    payment.Booking.GCashPaymentDate = payment.PaymentDate;

                    // Update booking status if fully paid
                    if (payment.Booking.Balance <= 0 && payment.Booking.Status == BookingStatus.Pending)
                    {
                        payment.Booking.Status = BookingStatus.Confirmed;
                    }

                    TempData["Success"] = $"Payment of ?{payment.Amount:N2} has been verified and applied to booking {payment.Booking.BookingNumber}";
                }
                else
                {
                    // Reject payment
                    payment.Status = PaymentStatus.Rejected;
                    payment.VerifiedBy = User.Identity?.Name ?? "Admin";
                    payment.VerifiedDate = DateTime.UtcNow;
                    payment.RejectionReason = model.Notes;

                    TempData["Warning"] = $"Payment of ?{payment.Amount:N2} has been rejected.";
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(PendingPayments));
            }

            var paymentData = await _context.GCashPayments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Room)
                .FirstOrDefaultAsync(p => p.Id == model.PaymentId);

            model.Payment = paymentData;
            return View(model);
        }

        // Helper method to generate reference number
        private string GenerateReferenceNumber()
        {
            return $"GCASH{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        }

        // Helper method to repopulate view model
        private async Task<GCashPaymentWithVerificationViewModel> RepopulateViewModel(GCashPaymentWithVerificationViewModel model)
        {
            var booking = await _context.Bookings.FindAsync(model.BookingId);
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();

            if (booking != null)
            {
                model.BookingNumber = booking.BookingNumber;
                model.TotalAmount = booking.TotalAmount;
                model.AmountPaid = booking.AmountPaid;
                model.RemainingBalance = booking.Balance;
                model.GuestName = booking.GuestName;
            }

            if (settings != null)
            {
                model.AdminGCashNumber = settings.AdminGCashNumber;
                model.AdminGCashAccountName = settings.AdminGCashAccountName;
                model.AdminGCashQRCode = settings.GCashQRCodeUrl;
                model.PaymentInstructions = settings.PaymentInstructions;
            }

            return model;
        }
    }
}
