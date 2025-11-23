using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem2.Data;
using HotelManagementSystem2.Models;
using HotelManagementSystem2.Models.ViewModels;

namespace HotelManagementSystem2.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            IQueryable<Booking> bookingsQuery = _context.Bookings.Include(b => b.Room);
            
            // If user is Guest, show only their bookings
            if (User.IsInRole("Guest"))
            {
                var userEmail = User.Identity?.Name;
                bookingsQuery = bookingsQuery.Where(b => b.GuestEmail == userEmail);
            }
            
            var bookings = await bookingsQuery
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            return View(bookings);
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (booking == null)
            {
                return NotFound();
            }

            // Guests can only view their own bookings
            if (User.IsInRole("Guest") && booking.GuestEmail != User.Identity?.Name)
            {
                return Forbid();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public async Task<IActionResult> Create(int? roomId)
        {
            var availableRooms = await _context.Rooms
                .Where(r => r.IsActive && r.Status == RoomStatus.Available)
                .ToListAsync();
            
            ViewData["RoomId"] = new SelectList(availableRooms, "Id", "RoomNumber", roomId);
            
            // Pre-fill guest info if user is logged in
            var booking = new Booking();
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
                if (user != null)
                {
                    booking.GuestName = user.FullName;
                    booking.GuestEmail = user.Email;
                    booking.GuestPhone = user.PhoneNumber ?? "";
                }
            }
            
            if (roomId.HasValue)
            {
                booking.RoomId = roomId.Value;
            }
            
            return View(booking);
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            // Remove validation for auto-generated fields
            ModelState.Remove("BookingNumber");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("CreatedBy");
            ModelState.Remove("Room");
            ModelState.Remove("TotalAmount");
            ModelState.Remove("Balance");
            
            if (ModelState.IsValid)
            {
                // Validate dates
                if (booking.CheckOutDate <= booking.CheckInDate)
                {
                    ModelState.AddModelError("CheckOutDate", "Check-out date must be after check-in date.");
                    LoadRoomsList(booking.RoomId);
                    return View(booking);
                }

                // Check room availability
                var room = await _context.Rooms.FindAsync(booking.RoomId);
                if (room == null || room.Status != RoomStatus.Available)
                {
                    ModelState.AddModelError("RoomId", "Selected room is not available.");
                    LoadRoomsList(booking.RoomId);
                    return View(booking);
                }

                // Check for overlapping bookings
                var hasOverlap = await _context.Bookings
                    .AnyAsync(b => b.RoomId == booking.RoomId &&
                                  b.Status != BookingStatus.Cancelled &&
                                  b.Status != BookingStatus.CheckedOut &&
                                  ((booking.CheckInDate >= b.CheckInDate && booking.CheckInDate < b.CheckOutDate) ||
                                   (booking.CheckOutDate > b.CheckInDate && booking.CheckOutDate <= b.CheckOutDate) ||
                                   (booking.CheckInDate <= b.CheckInDate && booking.CheckOutDate >= b.CheckOutDate)));

                if (hasOverlap)
                {
                    ModelState.AddModelError("", "Room is already booked for the selected dates.");
                    LoadRoomsList(booking.RoomId);
                    return View(booking);
                }

                // Calculate total amount
                var numberOfNights = (booking.CheckOutDate - booking.CheckInDate).Days;
                booking.TotalAmount = room.PricePerNight * numberOfNights;
                booking.Balance = booking.TotalAmount - booking.AmountPaid;

                // Generate booking number
                booking.BookingNumber = $"BK{DateTime.Now:yyyyMMddHHmmss}";
                booking.CreatedAt = DateTime.UtcNow;
                booking.CreatedBy = User.Identity?.Name ?? "System";
                
                // Set status to Pending for guests, Confirmed for admin
                booking.Status = User.IsInRole("Admin") ? BookingStatus.Confirmed : BookingStatus.Pending;

                _context.Add(booking);

                // Update room status
                room.Status = RoomStatus.Reserved;
                _context.Update(room);

                await _context.SaveChangesAsync();
                TempData["Success"] = "Booking created successfully! " + 
                    (User.IsInRole("Guest") ? "Your booking is pending admin approval." : "");
                return RedirectToAction(nameof(Index));
            }
            
            LoadRoomsList(booking.RoomId);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            
            ViewData["RoomId"] = new SelectList(await _context.Rooms.Where(r => r.IsActive).ToListAsync(), "Id", "RoomNumber", booking.RoomId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            // Remove ModelState validation for auto-generated fields
            ModelState.Remove("BookingNumber");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("CreatedBy");
            ModelState.Remove("Room");

            if (ModelState.IsValid)
            {
                try
                {
                    booking.Balance = booking.TotalAmount - booking.AmountPaid;
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "Booking updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            LoadRoomsList(booking.RoomId);
            return View(booking);
        }

        // POST: Bookings/CheckIn/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CheckIn(int id)
        {
            var booking = await _context.Bookings.Include(b => b.Room).FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status == BookingStatus.Confirmed || booking.Status == BookingStatus.Pending)
            {
                booking.Status = BookingStatus.CheckedIn;
                booking.Room.Status = RoomStatus.Occupied;
                
                _context.Update(booking);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Guest checked in successfully!";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Bookings/CheckOut/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CheckOut(int id)
        {
            var booking = await _context.Bookings.Include(b => b.Room).FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status == BookingStatus.CheckedIn)
            {
                booking.Status = BookingStatus.CheckedOut;
                booking.Room.Status = RoomStatus.Available;
                
                _context.Update(booking);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Guest checked out successfully!";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Bookings/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings.Include(b => b.Room).FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = BookingStatus.Cancelled;
            booking.Room.Status = RoomStatus.Available;
            
            await _context.SaveChangesAsync();
            TempData["Success"] = "Booking cancelled successfully!";
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Bookings/GCashPayment/5
        [HttpGet]
        public async Task<IActionResult> GCashPayment(int? id)
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

            // Check authorization - only admin or booking creator can pay
            if (!User.IsInRole("Admin") && booking.CreatedBy != User.Identity?.Name)
            {
                return Forbid();
            }

            var model = new GCashPaymentViewModel
            {
                BookingId = booking.Id,
                BookingNumber = booking.BookingNumber,
                TotalAmount = booking.TotalAmount,
                AmountPaid = booking.AmountPaid,
                RemainingBalance = booking.Balance,
                GuestName = booking.GuestName,
                Amount = booking.Balance, // Default to remaining balance
                GCashNumber = booking.GuestPhone // Pre-fill with guest phone
            };

            return View(model);
        }

        // POST: Bookings/GCashPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GCashPayment(GCashPaymentViewModel model)
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
                    
                    // Repopulate view model
                    model.BookingNumber = booking.BookingNumber;
                    model.TotalAmount = booking.TotalAmount;
                    model.AmountPaid = booking.AmountPaid;
                    model.RemainingBalance = booking.Balance;
                    model.GuestName = booking.GuestName;
                    
                    return View(model);
                }

                // Update booking with GCash payment
                booking.AmountPaid += model.Amount;
                booking.Balance = booking.TotalAmount - booking.AmountPaid;
                booking.PaymentMethod = "GCash";
                booking.GCashNumber = model.GCashNumber;
                booking.GCashAccountName = model.AccountName;
                booking.GCashReferenceNumber = model.ReferenceNumber ?? GenerateGCashReference();
                booking.GCashPaymentDate = DateTime.UtcNow;

                // If fully paid, update status
                if (booking.Balance <= 0)
                {
                    if (booking.Status == BookingStatus.Pending)
                    {
                        booking.Status = BookingStatus.Confirmed;
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"GCash payment of ?{model.Amount:N2} processed successfully! Reference: {booking.GCashReferenceNumber}";
                return RedirectToAction(nameof(Details), new { id = booking.Id });
            }

            // Repopulate view model if validation fails
            var bookingData = await _context.Bookings.FindAsync(model.BookingId);
            if (bookingData != null)
            {
                model.BookingNumber = bookingData.BookingNumber;
                model.TotalAmount = bookingData.TotalAmount;
                model.AmountPaid = bookingData.AmountPaid;
                model.RemainingBalance = bookingData.Balance;
                model.GuestName = bookingData.GuestName;
            }

            return View(model);
        }

        // Helper method to generate GCash reference number
        private string GenerateGCashReference()
        {
            return $"GCASH{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        }

        private void LoadRoomsList(int? selectedRoomId = null)
        {
            ViewBag.RoomId = new SelectList(_context.Rooms.Where(r => r.IsActive), "Id", "RoomNumber", selectedRoomId);
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }
    }
}
