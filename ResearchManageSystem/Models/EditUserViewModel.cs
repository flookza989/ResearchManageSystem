using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using ResearchManageSystem.Enums;
using System.ComponentModel.DataAnnotations;

namespace ResearchManageSystem.Models
{
    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Display(Name = "New Password")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string? NewPassword { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Role")]
        public UserRole Role { get; set; }

        [Required]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Departments { get; set; }
    }
}
