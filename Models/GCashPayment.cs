using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem2.Models
{
    public enum PaymentStatus
    {
        Pending,
        Verified,
        Rejected
    }

    public class GCashPayment
    {
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        [Required]
        [Display(Name = "Sender GCash Number")]
        public string SenderGCashNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Sender Account Name")]
        public string SenderAccountName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Receiver GCash Number")]
        public string ReceiverGCashNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Amount Sent")]
        public decimal Amount { get; set; }

        [Display(Name = "Reference Number")]
        public string? ReferenceNumber { get; set; }

        [Display(Name = "Screenshot/Proof")]
        public string? ProofImageUrl { get; set; }

        [Display(Name = "Payment Status")]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Verified By")]
        public string? VerifiedBy { get; set; }

        [Display(Name = "Verified Date")]
        public DateTime? VerifiedDate { get; set; }

        [Display(Name = "Rejection Reason")]
        public string? RejectionReason { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}
