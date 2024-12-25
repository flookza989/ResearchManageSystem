using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchManageSystem.Data.Entities
{
    public class Research
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string Status { get; set; }

        // Relationships
        public int ResearcherId { get; set; }
        
        [ForeignKey("ResearcherId")]
        public virtual User Researcher { get; set; }

        public int? AdvisorId { get; set; }
        
        [ForeignKey("AdvisorId")]
        public virtual User Advisor { get; set; }

        // Collection navigation properties
        public virtual ICollection<ResearchStudent> ResearchStudents { get; set; }
        public virtual ICollection<Submission> Submissions { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Research()
        {
            ResearchStudents = new HashSet<ResearchStudent>();
            Submissions = new HashSet<Submission>();
        }
    }
}