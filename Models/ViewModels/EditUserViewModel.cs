using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem2.Models.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Role")]
        public string? SelectedRole { get; set; }

        public List<string> CurrentRoles { get; set; } = new List<string>();
        public List<string> AllRoles { get; set; } = new List<string>();
    }
}
