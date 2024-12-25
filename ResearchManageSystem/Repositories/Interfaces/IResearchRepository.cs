using ResearchManageSystem.Data.Entities;

namespace ResearchManageSystem.Repositories.Interfaces
{
    public interface IResearchRepository
    {
        Task<Research> GetByIdAsync(int id);
        Task<Research> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Research>> GetAllAsync();
        Task<(IEnumerable<Research> Items, int TotalCount)> GetPagedResearchesAsync(
            int page,
            int pageSize,
            string searchTerm = null,
            string statusFilter = null,
            int? researcherId = null
        );
        Task AddAsync(Research research);
        void Update(Research research);
        void Remove(Research research);
        Task<IEnumerable<Research>> GetResearchesByResearcherIdAsync(int researcherId);
        Task<IEnumerable<Research>> GetResearchesByAdvisorIdAsync(int advisorId);
        Task<Research> GetCurrentResearchByUserIdAsync(int userId);
    }
}