using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchManageSystem.Data.Entities
{
    public class ResearchStudent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ResearchId { get; set; }

        [ForeignKey("ResearchId")]
        public virtual Research Research { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; }

        public bool IsLeader { get; set; }
        public DateTime JoinedDate { get; set; }
    }
}