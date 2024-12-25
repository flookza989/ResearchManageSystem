using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ResearchManageSystem.Models
{
    public class ProfileViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Display(Name = "Department")]
        public string DepartmentName { get; set; }

        [Display(Name = "Faculty")]
        public string FacultyName { get; set; }

        [Display(Name = "Role")]
        public string Role { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile? ProfileImage { get; set; }

        public string? CurrentProfileImage { get; set; }

        [Display(Name = "Current Password")]
        public string? CurrentPassword { get; set; }

        [Display(Name = "New Password")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        public string? NewPassword { get; set; }

        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match")]
        public string? ConfirmNewPassword { get; set; }
    }
}