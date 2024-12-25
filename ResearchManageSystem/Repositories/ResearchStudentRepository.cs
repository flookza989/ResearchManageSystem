using Microsoft.EntityFrameworkCore;
using ResearchManageSystem.Data;
using ResearchManageSystem.Data.Entities;
using ResearchManageSystem.Repositories.Interfaces;

namespace ResearchManageSystem.Repositories
{
    public class ResearchStudentRepository : GenericRepository<ResearchStudent>, IResearchStudentRepository
    {
        public ResearchStudentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ResearchStudent> GetResearchStudentAsync(int researchId, int studentId)
        {
            return await _context.ResearchStudents
                .FirstOrDefaultAsync(rs => rs.ResearchId == researchId && rs.StudentId == studentId);
        }

        public async Task<ResearchStudent> GetResearchLeaderAsync(int researchId)
        {
            return await _context.ResearchStudents
                .FirstOrDefaultAsync(rs => rs.ResearchId == researchId && rs.IsLeader);
        }

        public async Task<IEnumerable<ResearchStudent>> GetResearchStudentsByResearchIdAsync(int researchId)
        {
            return await _context.ResearchStudents
                .Include(rs => rs.Student)
                .Where(rs => rs.ResearchId == researchId)
                .ToListAsync();
        }

        public override async Task<ResearchStudent> GetByIdAsync(int id)
        {
            return await _context.ResearchStudents
                .Include(rs => rs.Student)
                .Include(rs => rs.Research)
                .FirstOrDefaultAsync(rs => rs.Id == id);
        }
    }
}