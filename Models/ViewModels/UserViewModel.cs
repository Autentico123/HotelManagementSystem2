namespace HotelManagementSystem2.Models.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public bool EmailConfirmed { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool IsLocked => LockoutEnd.HasValue && LockoutEnd > DateTimeOffset.UtcNow;
    }
}
