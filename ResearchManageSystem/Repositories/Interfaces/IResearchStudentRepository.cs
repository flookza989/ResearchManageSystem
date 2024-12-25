using ResearchManageSystem.Data.Entities;

namespace ResearchManageSystem.Repositories.Interfaces
{
    public interface IResearchStudentRepository : IGenericRepository<ResearchStudent>
    {
        Task<ResearchStudent> GetResearchStudentAsync(int researchId, int studentId);
        Task<ResearchStudent> GetResearchLeaderAsync(int researchId);
        Task<IEnumerable<ResearchStudent>> GetResearchStudentsByResearchIdAsync(int researchId);
    }
}