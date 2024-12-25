using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchManageSystem.Data.Entities
{
    public class Submission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ResearchId { get; set; }

        [ForeignKey("ResearchId")]
        public virtual Research Research { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string DocumentPath { get; set; }

        public string Status { get; set; } // เช่น "Pending", "Approved", "Rejected"

        public string Comments { get; set; }

        [Required]
        public DateTime SubmittedDate { get; set; }

        public int SubmittedById { get; set; }

        [ForeignKey("SubmittedById")]
        public virtual User SubmittedBy { get; set; }

        public DateTime? ReviewedDate { get; set; }

        public int? ReviewedById { get; set; }

        [ForeignKey("ReviewedById")]
        public virtual User ReviewedBy { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}