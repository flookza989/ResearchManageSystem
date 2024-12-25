using ResearchManageSystem.Data.Entities;
using System.Linq.Expressions;

namespace ResearchManageSystem.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetUserByIdWithDepartmentAndFacAsync(int userId);
        Task<User> GetUserByIdWithResearchStudentAsync(int userId);
        Task<User> GetUserByUsernameAsync(string username);
        Task<IEnumerable<Research>> GetUserResearchAsync(int userId);
        Task<IEnumerable<User>> GetUsersByDepartmentAsync(int departmentId);
        Task<IEnumerable<User>> GetAllUsersWithDetailsAsync();
        Task<bool> IsUsernameUniqueAsync(string username);
        Task<int> CountAsync(Expression<Func<User, bool>> predicate);
        Task<IEnumerable<Department>> GetAllDepartmentsWithFacultyAsync();
        Task<(IEnumerable<User> Users, int TotalCount)> GetPagedUsersWithDetailsAsync(int page, int pageSize);
        Task<User> GetResearchLeaderAsync(int researchId);
        Task<IEnumerable<User>> GetResearchMembersAsync(int researchId);
        Task<IEnumerable<User>> GetStudentsWithoutResearchAsync();
    }
}