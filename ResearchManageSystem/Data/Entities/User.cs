using System;
using System.Collections.Generic;
using ResearchManageSystem.Enums;

namespace ResearchManageSystem.Data.Entities
{
    public class User
    {
        public User()
        {
            AdvisedResearch = new HashSet<Research>();
            ResearchParticipations = new HashSet<ResearchStudent>();
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public int DepartmentId { get; set; }
        public string? ProfileImagePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual Department Department { get; set; }
        public virtual ICollection<Research> AdvisedResearch { get; set; }
        public virtual ICollection<ResearchStudent> ResearchParticipations { get; set; }

        // Helper property
        public string FullName => $"{FirstName} {LastName}";
    }
}