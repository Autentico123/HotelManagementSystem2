using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HotelManagementSystem2.Models.ViewModels
{
    public class GCashPaymentWithVerificationViewModel
    {
        [Required(ErrorMessage = "Booking ID is required")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Your GCash mobile number is required")]
        [RegularExpression(@"^(09|\+639)\d{9}$", ErrorMessage = "Please enter a valid Philippine mobile number (e.g., 09171234567 or +639171234567)")]
        [Display(Name = "Your GCash Mobile Number")]
        public string SenderGCashNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Your account name is required")]
        [StringLength(100, ErrorMessage = "Account name cannot exceed 100 characters")]
        [Display(Name = "Your GCash Account Name")]
        public string SenderAccountName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Payment Amount")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Display(Name = "GCash Reference Number")]
        [StringLength(50)]
        public string? ReferenceNumber { get; set; }

        [Display(Name = "Upload Payment Proof (Screenshot)")]
        public IFormFile? ProofImage { get; set; }

        [Display(Name = "Additional Notes")]
        [StringLength(500)]
        public string? Notes { get; set; }

        // Read-only properties for display
        public string? BookingNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal RemainingBalance { get; set; }
        public string? GuestName { get; set; }
        
        // Admin GCash details
        public string AdminGCashNumber { get; set; } = string.Empty;
        public string AdminGCashAccountName { get; set; } = string.Empty;
        public string? AdminGCashQRCode { get; set; }
        public string? PaymentInstructions { get; set; }
    }

    public class VerifyPaymentViewModel
    {
        [Required]
        public int PaymentId { get; set; }

        [Required]
        public bool IsApproved { get; set; }

        [Display(Name = "Verification Notes / Rejection Reason")]
        [StringLength(500)]
        public string? Notes { get; set; }

        // Display properties
        public GCashPayment? Payment { get; set; }
    }
}
