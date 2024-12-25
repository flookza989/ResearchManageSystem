using System;
using System.Collections.Generic;

namespace ResearchManageSystem.Data.Entities
{
    public class Faculty
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<Department> Departments { get; set; }
    }
}