using Microsoft.EntityFrameworkCore;
using ResearchManageSystem.Data;
using ResearchManageSystem.Data.Entities;
using ResearchManageSystem.Enums;
using ResearchManageSystem.Repositories.Interfaces;
using System.Linq.Expressions;

namespace ResearchManageSystem.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User> GetUserByIdWithDepartmentAndFacAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Department)
                .ThenInclude(d => d.Faculty)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<IEnumerable<Research>> GetUserResearchAsync(int userId)
        {
            // Get research where user is either an advisor or a student
            return await _context.Research
                .Include(r => r.Advisor)
                .Include(r => r.ResearchStudents)
                    .ThenInclude(rs => rs.Student)
                .Where(r => r.AdvisorId == userId || r.ResearchStudents.Any(rs => rs.StudentId == userId))
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByDepartmentAsync(int departmentId)
        {
            return await _context.Users
                .Include(u => u.Department)
                .Where(u => u.DepartmentId == departmentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAllUsersWithDetailsAsync()
        {
            return await _context.Users
                .Include(u => u.Department)
                .ThenInclude(d => d.Faculty)
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        public async Task<bool> IsUsernameUniqueAsync(string username)
        {
            return !await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<int> CountAsync(Expression<Func<User, bool>> predicate)
        {
            return await _context.Users.CountAsync(predicate);
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsWithFacultyAsync()
        {
            return await _context.Departments
                .Include(d => d.Faculty)
                .OrderBy(d => d.Faculty.Name)
                .ThenBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<(IEnumerable<User> Users, int TotalCount)> GetPagedUsersWithDetailsAsync(int page, int pageSize)
        {
            var query = _context.Users
                .Include(u => u.Department)
                .ThenInclude(d => d.Faculty)
                .OrderBy(u => u.Username);

            var totalCount = await query.CountAsync();
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }

        public async Task<User> GetResearchLeaderAsync(int researchId)
        {
            return await _context.ResearchStudents
                .Include(rs => rs.Student)
                .Where(rs => rs.ResearchId == researchId && rs.IsLeader)
                .Select(rs => rs.Student)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<User>> GetResearchMembersAsync(int researchId)
        {
            return await _context.ResearchStudents
                .Include(rs => rs.Student)
                .Where(rs => rs.ResearchId == researchId)
                .OrderByDescending(rs => rs.IsLeader)
                .Select(rs => rs.Student)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetStudentsWithoutResearchAsync()
        {
            return await _context.Users
                .Where(u => u.Role == UserRole.Student)
                .Include(u => u.ResearchParticipations)
                .Where(u => !u.ResearchParticipations.Any())
                .ToListAsync();
        }

        public async Task<User> GetUserByIdWithResearchStudentAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.ResearchParticipations)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}