using System.ComponentModel.DataAnnotations;

namespace FastPMS.Models.ViewModel
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        // Default values - these won't be shown in the registration form
        public string Role { get; set; } = "Client";
        public string Department { get; set; } = "None";
    }
}
