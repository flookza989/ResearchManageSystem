using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResearchManageSystem.Models;
using ResearchManageSystem.Services;
using System.Security.Claims;

namespace ResearchManageSystem.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(IUserService userService, IWebHostEnvironment webHostEnvironment)
        {
            _userService = userService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _userService.GetUserByIdWithDepartmentAndFacAsync(userId);

            if (user == null)
                return NotFound();

            var viewModel = new ProfileViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DepartmentName = user.Department?.Name,
                FacultyName = user.Department?.Faculty?.Name,
                Role = user.Role.ToString(),
                CurrentProfileImage = user.ProfileImagePath
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            var user = await _userService.GetUserByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            // ตรวจสอบการเปลี่ยนรหัสผ่าน
            if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                // ตรวจสอบรหัสผ่านปัจจุบัน
                if (!_userService.VerifyPassword(user, model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect");
                    
                    // ดึงข้อมูลอื่นๆ กลับมาแสดง
                    model.DepartmentName = user.Department?.Name;
                    model.FacultyName = user.Department?.Faculty?.Name;
                    model.Role = user.Role.ToString();
                    model.CurrentProfileImage = user.ProfileImagePath;
                    
                    return View("Index", model);
                }

                // อัพเดตรหัสผ่านใหม่
                user.Password = _userService.HashPassword(model.NewPassword);
            }

            // Update user information
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;

            // Handle profile image upload
            if (model.ProfileImage != null)
            {
                // ลบรูปเก่าถ้ามี
                if (!string.IsNullOrEmpty(user.ProfileImagePath))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath,
                        user.ProfileImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                        catch (Exception ex)
                        {
                            // Log error if needed
                        }
                    }
                }

                // อัพโหลดรูปใหม่
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfileImage.FileName;
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(fileStream);
                }

                // Update profile image path
                user.ProfileImagePath = "/uploads/profiles/" + uniqueFileName;
            }

            await _userService.UpdateUserAsync(user);

            // อัพเดท claims
            var newClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("FullName", user.FullName),
                new Claim("ProfileImage", user.ProfileImagePath ?? "/images/default-profile.png")
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(newClaims, CookieAuthenticationDefaults.AuthenticationScheme))
            );

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}