using Microsoft.EntityFrameworkCore;
using ResearchManageSystem.Data;
using ResearchManageSystem.Data.Entities;
using ResearchManageSystem.Enums;
using ResearchManageSystem.Repositories.Interfaces;

namespace ResearchManageSystem.Repositories
{
    public class ResearchRepository : GenericRepository<Research>, IResearchRepository
    {
        public ResearchRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Research> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Research
                .Include(r => r.Researcher)
                .Include(r => r.Advisor)
                .Include(r => r.ResearchStudents)
                    .ThenInclude(rs => rs.Student)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<(IEnumerable<Research> Items, int TotalCount)> GetPagedResearchesAsync(
            int page,
            int pageSize,
            string searchTerm = null,
            string statusFilter = null,
            int? researcherId = null)
        {
            var query = _context.Research
                .Include(r => r.Researcher)
                .Include(r => r.Advisor)
                .Include(r => r.ResearchStudents)
                    .ThenInclude(rs => rs.Student)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(r => r.Title.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(r => r.Status == statusFilter);
            }

            if (researcherId.HasValue)
            {
                query = query.Where(r => r.ResearcherId == researcherId);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<Research>> GetResearchesByResearcherIdAsync(int researcherId)
        {
            return await _context.Research
                .Include(r => r.Advisor)
                .Include(r => r.ResearchStudents)
                    .ThenInclude(rs => rs.Student)
                .Where(r => r.ResearcherId == researcherId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Research>> GetResearchesByAdvisorIdAsync(int advisorId)
        {
            return await _context.Research
                .Include(r => r.Researcher)
                .Include(r => r.ResearchStudents)
                    .ThenInclude(rs => rs.Student)
                .Where(r => r.AdvisorId == advisorId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Research> GetCurrentResearchByUserIdAsync(int userId)
        {
            return await _context.Research
                .Include(r => r.Advisor)
                .Include(r => r.ResearchStudents)
                    .ThenInclude(rs => rs.Student)
                .FirstOrDefaultAsync(r => r.ResearchStudents.Any(rs => rs.StudentId == userId));
        }
    }
}