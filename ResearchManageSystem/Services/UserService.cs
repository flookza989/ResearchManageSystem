using Microsoft.Extensions.Logging;
using ResearchManageSystem.Data.Entities;
using ResearchManageSystem.Enums;
using ResearchManageSystem.Repositories.Interfaces;

namespace ResearchManageSystem.Services
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> GetUserByIdWithDepartmentAndFacAsync(int id);
        Task<IEnumerable<User>> GetAllUsersWithDetailsAsync();
        Task<User> CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
        Task<bool> ValidateUserCredentialsAsync(string username, string password);
        Task<IEnumerable<Department>> GetAllDepartmentsWithFacultyAsync();
        Task<(IEnumerable<User> Users, int TotalCount)> GetPagedUsersWithDetailsAsync(int page, int pageSize);
        Task<int> GetAdminCountAsync();
        Task<bool> IsLastAdminAsync(int userId);
        bool VerifyPassword(User user, string password);
        string HashPassword(string password);
        Task<IEnumerable<User>> GetStudentsAsync();
        Task<IEnumerable<User>> GetStudentsWithoutResearchAsync();
        Task<User> GetUserByIdWithResearchStudentAsync(int userId);
    }

    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUnitOfWork unitOfWork,
            ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetStudentsAsync() 
        {
            try
            {
                var users = await _unitOfWork.Users.GetAllUsersWithDetailsAsync();
                return users.Where(u => u.Role == UserRole.Student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting students");
                throw;
            }
        }

        // ... เมธอดอื่นๆ คงเดิม ...

        public bool VerifyPassword(User user, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, user.Password);
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            try
            {
                return await _unitOfWork.Users.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user with ID {Id}", id);
                throw;
            }
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            try
            {
                return await _unitOfWork.Users.GetUserByUsernameAsync(username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user with username {Username}", username);
                throw;
            }
        }

        public async Task<User> GetUserByIdWithDepartmentAndFacAsync(int id)
        {
            try
            {
                return await _unitOfWork.Users.GetUserByIdWithDepartmentAndFacAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user details with ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersWithDetailsAsync()
        {
            try
            {
                return await _unitOfWork.Users.GetAllUsersWithDetailsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users with details");
                throw;
            }
        }

        public async Task<User> CreateUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            try
            {
                // ตรวจสอบ username ซ้ำ
                var existingUser = await _unitOfWork.Users.GetUserByUsernameAsync(user.Username);
                if (existingUser != null)
                {
                    throw new InvalidOperationException($"Username '{user.Username}' already exists.");
                }

                // Hash password
                user.Password = HashPassword(user.Password);

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("User created successfully: {Username}", user.Username);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Username}", user.Username);
                throw;
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            try
            {
                var existingUser = await _unitOfWork.Users.GetByIdAsync(user.Id);
                if (existingUser == null)
                {
                    throw new InvalidOperationException($"User with ID {user.Id} not found.");
                }

                // Update password if changed
                if (!string.IsNullOrEmpty(user.Password) && user.Password != existingUser.Password)
                {
                    // Check if the password is already hashed
                    if (!user.Password.StartsWith("$2a$") && !user.Password.StartsWith("$2b$") && !user.Password.StartsWith("$2y$"))
                    {
                        user.Password = HashPassword(user.Password);
                    }
                }
                else
                {
                    user.Password = existingUser.Password;
                }

                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("User updated successfully: {Username}", user.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {Id}", user.Id);
                throw;
            }
        }

        public async Task DeleteUserAsync(int id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                {
                    throw new InvalidOperationException($"User with ID {id} not found.");
                }

                // Check if this is the last admin
                if (await IsLastAdminAsync(id))
                {
                    throw new InvalidOperationException("Cannot delete the last admin user.");
                }

                // Delete profile image if exists
                if (!string.IsNullOrEmpty(user.ProfileImagePath))
                {
                    await DeleteProfileImageAsync(user.ProfileImagePath);
                }

                _unitOfWork.Users.Remove(user);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("User deleted successfully: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {Id}", id);
                throw;
            }
        }

        public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
        {
            try
            {
                var user = await _unitOfWork.Users.GetUserByUsernameAsync(username);
                if (user == null) return false;

                return VerifyPassword(user, password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating credentials for user: {Username}", username);
                throw;
            }
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsWithFacultyAsync()
        {
            try
            {
                return await _unitOfWork.Users.GetAllDepartmentsWithFacultyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting departments with faculty");
                throw;
            }
        }

        public async Task<(IEnumerable<User> Users, int TotalCount)> GetPagedUsersWithDetailsAsync(int page, int pageSize)
        {
            try
            {
                return await _unitOfWork.Users.GetPagedUsersWithDetailsAsync(page, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged users. Page: {Page}, PageSize: {PageSize}", page, pageSize);
                throw;
            }
        }

        public async Task<int> GetAdminCountAsync()
        {
            try
            {
                return await _unitOfWork.Users.CountAsync(u => u.Role == UserRole.Admin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin count");
                throw;
            }
        }

        public async Task<bool> IsLastAdminAsync(int userId)
        {
            try
            {
                var adminCount = await GetAdminCountAsync();
                if (adminCount > 1) return false;

                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                return user?.Role == UserRole.Admin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user is last admin: {Id}", userId);
                throw;
            }
        }

        private async Task DeleteProfileImageAsync(string profileImagePath)
        {
            try
            {
                var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var imagePath = Path.Combine(webRootPath, profileImagePath.TrimStart('/'));
                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete profile image: {Path}", profileImagePath);
            }
        }

        public async Task<IEnumerable<User>> GetStudentsWithoutResearchAsync()
        {
            return await _unitOfWork.Users.GetStudentsWithoutResearchAsync();
        }

        public async Task<User> GetUserByIdWithResearchStudentAsync(int userId)
        {
            return await _unitOfWork.Users.GetUserByIdWithResearchStudentAsync(userId);
        }
    }
}