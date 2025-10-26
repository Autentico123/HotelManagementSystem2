namespace HotelManagementSystem2.Models
{
    public enum RoomType
    {
        Single,
        Double,
        Suite,
        Deluxe,
        Family
    }

    public enum RoomStatus
    {
        Available,
        Occupied,
        Maintenance,
        Reserved
    }

    public class Room
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public RoomType RoomType { get; set; }
        public RoomStatus Status { get; set; } = RoomStatus.Available;
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Amenities { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
