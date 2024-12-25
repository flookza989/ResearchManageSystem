using System.ComponentModel.DataAnnotations;

namespace ResearchManageSystem.Models
{
    public class UserManagementViewModel
    {
        public IEnumerable<UserListItemViewModel> Users { get; set; }
        public IFormFile UploadFile { get; set; }
        public PaginationInfo Pagination { get; set; }
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class UserListItemViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Username")]
        public string Username { get; set; }
        
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Display(Name = "Role")]
        public string Role { get; set; }
        
        [Display(Name = "Department")]
        public string DepartmentName { get; set; }
        
        [Display(Name = "Faculty")]
        public string FacultyName { get; set; }
        
        public string ProfileImagePath { get; set; }
        
        // Helper properties
        public string FullName => $"{FirstName} {LastName}";
        public string DepartmentAndFaculty => $"{DepartmentName} - {FacultyName}";
    }

    public class UserImportResult
    {
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

}