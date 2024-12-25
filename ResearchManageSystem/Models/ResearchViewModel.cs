using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ResearchManageSystem.Models
{
    public class ResearchViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        [Display(Name = "Research Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Advisor is required")]
        [Display(Name = "Advisor")]
        public int? AdvisorId { get; set; }

        // Read-only properties
        public string Status { get; set; }
        public string ResearcherName { get; set; }
        public string AdvisorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Students properties
        public List<ResearchStudentViewModel> Students { get; set; } = new();
        public SelectList AvailableStudents { get; set; }

        // Dropdown lists
        public SelectList Advisors { get; set; }
    }

    public class ResearchStudentViewModel
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public bool IsLeader { get; set; }
        public DateTime JoinedDate { get; set; }
    }

    public class ResearchListViewModel
    {
        public IEnumerable<ResearchViewModel> Researches { get; set; }
        public PaginationInfo Pagination { get; set; }
        public string SearchTerm { get; set; }
        public string StatusFilter { get; set; }
        public SelectList StatusList { get; set; }
    }
}