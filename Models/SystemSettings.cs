using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem2.Models
{
    public class SystemSettings
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Admin GCash Number")]
        [RegularExpression(@"^(09|\+639)\d{9}$", ErrorMessage = "Invalid Philippine mobile number")]
        public string AdminGCashNumber { get; set; } = "09171234567";

        [Required]
        [Display(Name = "Admin GCash Account Name")]
        public string AdminGCashAccountName { get; set; } = "Hotel Administrator";

        [Display(Name = "GCash QR Code Image")]
        public string? GCashQRCodeUrl { get; set; }

        [Display(Name = "Payment Instructions")]
        public string PaymentInstructions { get; set; } = "Please send payment to the GCash number above and upload proof of payment.";

        [Display(Name = "Last Updated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated By")]
        public string UpdatedBy { get; set; } = "System";
    }
}
