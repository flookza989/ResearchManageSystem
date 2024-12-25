using ResearchManageSystem.Data;
using ResearchManageSystem.Repositories.Interfaces;

namespace ResearchManageSystem.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed = false;

        private IUserRepository _users;
        private IResearchRepository _researches;
        private IResearchStudentRepository _researchStudents;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IResearchRepository Researches => _researches ??= new ResearchRepository(_context);
        public IResearchStudentRepository ResearchStudents => _researchStudents ??= new ResearchStudentRepository(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}