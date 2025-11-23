using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem2.Models.ViewModels
{
    public class GCashPaymentViewModel
    {
        [Required(ErrorMessage = "Booking ID is required")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "GCash mobile number is required")]
        [RegularExpression(@"^(09|\+639)\d{9}$", ErrorMessage = "Please enter a valid Philippine mobile number (e.g., 09171234567 or +639171234567)")]
        [Display(Name = "GCash Mobile Number")]
        public string GCashNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Payment Amount")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Account name is required")]
        [StringLength(100, ErrorMessage = "Account name cannot exceed 100 characters")]
        [Display(Name = "GCash Account Name")]
        public string AccountName { get; set; } = string.Empty;

        [Display(Name = "Reference Number")]
        [StringLength(50)]
        public string? ReferenceNumber { get; set; }

        // Read-only properties for display
        public string? BookingNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal RemainingBalance { get; set; }
        public string? GuestName { get; set; }
    }
}
