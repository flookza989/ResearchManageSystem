using Microsoft.EntityFrameworkCore;
using ResearchManageSystem.Data.Entities;

namespace ResearchManageSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Research> Research { get; set; }
        public DbSet<ResearchStudent> ResearchStudents { get; set; }
        public DbSet<Submission> Submissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configurations
            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Research configurations
            modelBuilder.Entity<Research>()
                .HasOne(r => r.Advisor)
                .WithMany(u => u.AdvisedResearch)
                .HasForeignKey(r => r.AdvisorId)
                .OnDelete(DeleteBehavior.Restrict);

            // ResearchStudent configurations
            modelBuilder.Entity<ResearchStudent>(entity =>
            {
                entity.HasKey(rs => new { rs.ResearchId, rs.StudentId });

                entity.HasOne(rs => rs.Research)
                    .WithMany(r => r.ResearchStudents)
                    .HasForeignKey(rs => rs.ResearchId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rs => rs.Student)
                    .WithMany(u => u.ResearchParticipations)
                    .HasForeignKey(rs => rs.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(rs => new { rs.ResearchId, rs.IsLeader })
                    .HasFilter("[IsLeader] = 1")
                    .IsUnique();
            });

            // Submission configurations
            modelBuilder.Entity<Submission>()
                .HasOne(s => s.Research)
                .WithMany(r => r.Submissions)
                .HasForeignKey(s => s.ResearchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Submission>()
                .HasOne(s => s.SubmittedBy)
                .WithMany()
                .HasForeignKey(s => s.SubmittedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Submission>()
                .HasOne(s => s.ReviewedBy)
                .WithMany()
                .HasForeignKey(s => s.ReviewedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Department configurations
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Faculty)
                .WithMany(f => f.Departments)
                .HasForeignKey(d => d.FacultyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}