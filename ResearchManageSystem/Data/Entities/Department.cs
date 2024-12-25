using System;
using System.Collections.Generic;

namespace ResearchManageSystem.Data.Entities
{
    public class Department
    {
        public Department()
        {
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public int FacultyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual Faculty Faculty { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}