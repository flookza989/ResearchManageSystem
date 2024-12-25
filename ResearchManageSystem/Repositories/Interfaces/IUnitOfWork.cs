using ResearchManageSystem.Data.Entities;

namespace ResearchManageSystem.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IResearchRepository Researches { get; }
        IResearchStudentRepository ResearchStudents { get; }
        Task<int> CompleteAsync();
    }
}