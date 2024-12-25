using Microsoft.Extensions.Logging;
using ResearchManageSystem.Data.Entities;
using ResearchManageSystem.Repositories.Interfaces;

namespace ResearchManageSystem.Services
{
    public interface IResearchService
    {
        Task<Research> GetResearchByIdAsync(int id);
        Task<Research> GetResearchByIdWithDetailsAsync(int id);
        Task<IEnumerable<Research>> GetAllResearchesAsync();
        Task<(IEnumerable<Research> Researches, int TotalCount)> GetPagedResearchesAsync(
            int page, 
            int pageSize, 
            string searchTerm = null, 
            string statusFilter = null, 
            int? researcherId = null
        );
        Task<Research> CreateResearchAsync(Research research);
        Task UpdateResearchAsync(Research research);
        Task DeleteResearchAsync(int id);
        Task<IEnumerable<Research>> GetResearchesByResearcherIdAsync(int researcherId);
        Task<IEnumerable<Research>> GetResearchesByAdvisorIdAsync(int advisorId);
        Task<bool> HasExistingResearch(int researcherId);
        Task<ResearchStudent> GetResearchStudentAsync(int researchId, int studentId);
        Task<ResearchStudent> GetResearchStudentByIdAsync(int researchStudentId);
        Task AddResearchStudentAsync(ResearchStudent researchStudent);
        Task RemoveResearchStudentAsync(int researchStudentId);
        Task<bool> IsStudentLeaderInAnyResearch(int studentId);
        Task<Research> GetCurrentResearchByUserIdAsync(int userId);
    }

    public class ResearchService : IResearchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ResearchService> _logger;

        public ResearchService(IUnitOfWork unitOfWork, ILogger<ResearchService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ResearchStudent> GetResearchStudentAsync(int researchId, int studentId)
        {
            try
            {
                return await _unitOfWork.ResearchStudents.GetResearchStudentAsync(researchId, studentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting research student for ResearchId: {ResearchId}, StudentId: {StudentId}", 
                    researchId, studentId);
                throw;
            }
        }

        public async Task<ResearchStudent> GetResearchStudentByIdAsync(int researchStudentId)
        {
            try
            {
                return await _unitOfWork.ResearchStudents.GetByIdAsync(researchStudentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting research student with ID {Id}", researchStudentId);
                throw;
            }
        }

        public async Task AddResearchStudentAsync(ResearchStudent researchStudent)
        {
            try
            {
                // Check if student is already in this research
                var existingStudent = await GetResearchStudentAsync(researchStudent.ResearchId, researchStudent.StudentId);
                if (existingStudent != null)
                {
                    throw new InvalidOperationException("Student is already in this research project.");
                }

                // If this student is being set as leader, remove leader status from other students
                if (researchStudent.IsLeader)
                {
                    var currentLeader = await _unitOfWork.ResearchStudents
                        .GetResearchLeaderAsync(researchStudent.ResearchId);
                    
                    if (currentLeader != null)
                    {
                        currentLeader.IsLeader = false;
                        _unitOfWork.ResearchStudents.Update(currentLeader);
                    }
                }

                await _unitOfWork.ResearchStudents.AddAsync(researchStudent);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding research student");
                throw;
            }
        }

        public async Task RemoveResearchStudentAsync(int researchStudentId)
        {
            try
            {
                var researchStudent = await GetResearchStudentByIdAsync(researchStudentId);
                if (researchStudent == null)
                    throw new InvalidOperationException($"Research student with ID {researchStudentId} not found");

                _unitOfWork.ResearchStudents.Remove(researchStudent);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing research student with ID {Id}", researchStudentId);
                throw;
            }
        }

        public async Task<bool> HasExistingResearch(int researcherId)
        {
            try
            {
                var researches = await _unitOfWork.Researches.GetResearchesByResearcherIdAsync(researcherId);
                return researches.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existing research for researcher ID {Id}", researcherId);
                throw;
            }
        }

        public async Task<Research> GetResearchByIdAsync(int id)
        {
            try
            {
                return await _unitOfWork.Researches.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting research with ID {Id}", id);
                throw;
            }
        }

        public async Task<Research> GetResearchByIdWithDetailsAsync(int id)
        {
            try
            {
                return await _unitOfWork.Researches.GetByIdWithDetailsAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting research details with ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Research>> GetAllResearchesAsync()
        {
            try
            {
                return await _unitOfWork.Researches.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all researches");
                throw;
            }
        }

        public async Task<(IEnumerable<Research> Researches, int TotalCount)> GetPagedResearchesAsync(
            int page, 
            int pageSize, 
            string searchTerm = null, 
            string statusFilter = null, 
            int? researcherId = null)
        {
            try
            {
                return await _unitOfWork.Researches.GetPagedResearchesAsync(
                    page, pageSize, searchTerm, statusFilter, researcherId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged researches. Page: {Page}, PageSize: {PageSize}", 
                    page, pageSize);
                throw;
            }
        }

        public async Task<Research> CreateResearchAsync(Research research)
        {
            try
            {
                // Check if researcher already has a research
                if (await HasExistingResearch(research.ResearcherId))
                {
                    throw new InvalidOperationException("A student can only have one research project at a time.");
                }

                research.CreatedAt = DateTime.UtcNow;
                await _unitOfWork.Researches.AddAsync(research);
                await _unitOfWork.CompleteAsync();
                return research;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating research");
                throw;
            }
        }

        public async Task UpdateResearchAsync(Research research)
        {
            try
            {
                research.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Researches.Update(research);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating research with ID {Id}", research.Id);
                throw;
            }
        }

        public async Task DeleteResearchAsync(int id)
        {
            try
            {
                var research = await GetResearchByIdAsync(id);
                if (research == null)
                    throw new InvalidOperationException($"Research with ID {id} not found");

                _unitOfWork.Researches.Remove(research);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting research with ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Research>> GetResearchesByResearcherIdAsync(int researcherId)
        {
            try
            {
                return await _unitOfWork.Researches.GetResearchesByResearcherIdAsync(researcherId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting researches for researcher ID {Id}", researcherId);
                throw;
            }
        }

        public async Task<IEnumerable<Research>> GetResearchesByAdvisorIdAsync(int advisorId)
        {
            try
            {
                return await _unitOfWork.Researches.GetResearchesByAdvisorIdAsync(advisorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting researches for advisor ID {Id}", advisorId);
                throw;
            }
        }

        public async Task<bool> IsStudentLeaderInAnyResearch(int studentId)
        {
            try
            {
                var researchStudent = await _unitOfWork.ResearchStudents
                    .FirstOrDefaultAsync(rs => rs.StudentId == studentId && rs.IsLeader);
                return researchStudent != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if student is leader in any research");
                throw;
            }
        }

        public async Task<Research> GetCurrentResearchByUserIdAsync(int userId)
        {
            try
            {
                return await _unitOfWork.Researches.GetCurrentResearchByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current research for user ID {Id}", userId);
                throw;
            }
        }
    }
}