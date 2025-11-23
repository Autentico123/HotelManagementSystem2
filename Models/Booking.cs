using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem2.Models
{
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        CheckedIn,
        CheckedOut,
        Cancelled
    }

    public class Booking
    {
        public int Id { get; set; }
        
        [Display(Name = "Booking Number")]
        public string BookingNumber { get; set; } = string.Empty;
        
        // Guest information
        [Required(ErrorMessage = "Guest name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Guest Name")]
        public string GuestName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string GuestEmail { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string GuestPhone { get; set; } = string.Empty;
        
        [Display(Name = "Address")]
        public string GuestAddress { get; set; } = string.Empty;
        
        [Display(Name = "ID Document")]
        public string IdentityDocument { get; set; } = string.Empty;
        
        // Booking details
        [Required(ErrorMessage = "Please select a room")]
        [Display(Name = "Room")]
        public int RoomId { get; set; }
        
        public Room Room { get; set; } = null!;
        
        [Required(ErrorMessage = "Check-in date is required")]
        [Display(Name = "Check-in Date")]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }
        
        [Required(ErrorMessage = "Check-out date is required")]
        [Display(Name = "Check-out Date")]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }
        
        [Required(ErrorMessage = "Number of guests is required")]
        [Range(1, 10, ErrorMessage = "Number of guests must be between 1 and 10")]
        [Display(Name = "Number of Guests")]
        public int NumberOfGuests { get; set; }
        
        // Payment
        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }
        
        [Display(Name = "Amount Paid")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "Amount paid cannot be negative")]
        public decimal AmountPaid { get; set; }
        
        [Display(Name = "Balance")]
        [DataType(DataType.Currency)]
        public decimal Balance { get; set; }
        
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = string.Empty;
        
        // GCash Payment Details
        [Display(Name = "GCash Number")]
        public string? GCashNumber { get; set; }
        
        [Display(Name = "GCash Account Name")]
        public string? GCashAccountName { get; set; }
        
        [Display(Name = "GCash Reference Number")]
        public string? GCashReferenceNumber { get; set; }
        
        [Display(Name = "GCash Payment Date")]
        public DateTime? GCashPaymentDate { get; set; }
        
        // Status and tracking
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; } = string.Empty;
        
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}
