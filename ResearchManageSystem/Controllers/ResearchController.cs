using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ResearchManageSystem.Data.Entities;
using ResearchManageSystem.Enums;
using ResearchManageSystem.Models;
using ResearchManageSystem.Services;
using System.Security.Claims;

namespace ResearchManageSystem.Controllers
{
    [Authorize(Roles = "Student")]
    public class ResearchController : Controller
    {
        private readonly IResearchService _researchService;
        private readonly IUserService _userService;

        public ResearchController(
            IResearchService researchService,
            IUserService userService)
        {
            _researchService = researchService;
            _userService = userService;
        }

        // GET: Research
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // ดึงงานวิจัยของนักศึกษาคนนี้ (จะมีได้แค่ 1 งาน)
            var research = await _researchService.GetCurrentResearchByUserIdAsync(userId);

            if (research == null)
            {
                // ถ้ายังไม่มีงานวิจัย แสดงหน้าว่าง
                return View(null);
            }

            // สร้าง ViewModel สำหรับแสดงผล
            var viewModel = new ResearchViewModel
            {
                Id = research.Id,
                Title = research.Title,
                Status = research.Status,
                ResearcherName = research.Researcher?.FullName,
                AdvisorName = research.Advisor?.FullName,
                CreatedAt = research.CreatedAt,
                UpdatedAt = research.UpdatedAt,
                Students = research.ResearchStudents?
                    .Select(rs => new ResearchStudentViewModel
                    {
                        Id = rs.Id,
                        StudentId = rs.StudentId,
                        StudentName = rs.Student?.FullName,
                        IsLeader = rs.IsLeader,
                        JoinedDate = rs.JoinedDate
                    })
                    .ToList() ?? new List<ResearchStudentViewModel>()
            };

            return View(viewModel);
        }

        // GET: Research/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            
            if (await _researchService.HasExistingResearch(userId))
            {
                TempData["ErrorMessage"] = "You already have an active research project.";
                return RedirectToAction(nameof(Index));
            }

            var advisors = await GetAdvisors();
            var viewModel = new ResearchViewModel
            {
                Advisors = new SelectList(advisors, "Id", "FullName"),
                CreatedAt = DateTime.UtcNow
            };

            return View(viewModel);
        }

        // POST: Research/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ResearchViewModel model)
        {
            ModelState.Clear();

            if (string.IsNullOrEmpty(model.Title))
            {
                ModelState.AddModelError("Title", "Title is required");
            }
            if (!model.AdvisorId.HasValue)
            {
                ModelState.AddModelError("AdvisorId", "Advisor is required");
            }

            if (!ModelState.IsValid)
            {
                model.Advisors = new SelectList(await GetAdvisors(), "Id", "FullName");
                return View(model);
            }

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                if (await _researchService.HasExistingResearch(userId))
                {
                    TempData["ErrorMessage"] = "You already have an active research project.";
                    return RedirectToAction(nameof(Index));
                }

                // Create research
                var research = new Research
                {
                    Title = model.Title,
                    Status = ResearchStatus.InProgress.ToString(),
                    ResearcherId = userId,
                    AdvisorId = model.AdvisorId,
                    CreatedAt = DateTime.Now
                };

                research = await _researchService.CreateResearchAsync(research);

                // Add the creator as research leader
                var researchStudent = new ResearchStudent
                {
                    ResearchId = research.Id,
                    StudentId = userId,
                    IsLeader = true,
                    JoinedDate = DateTime.UtcNow
                };

                await _researchService.AddResearchStudentAsync(researchStudent);

                TempData["SuccessMessage"] = "Research project created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating research: {ex.Message}");
                model.Advisors = new SelectList(await GetAdvisors(), "Id", "FullName");
                return View(model);
            }
        }

        // GET: Research/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var research = await _researchService.GetResearchByIdWithDetailsAsync(id);
            if (research == null)
                return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (research.ResearcherId != userId)
                return Forbid();

            // Get all students except those already in the research
            var allStudents = await _userService.GetStudentsWithoutResearchAsync();
            var currentStudentIds = research.ResearchStudents.Select(rs => rs.StudentId).ToList();
            var availableStudents = allStudents.Where(s => !currentStudentIds.Contains(s.Id));

            var viewModel = new ResearchViewModel
            {
                Id = research.Id,
                Title = research.Title,
                Status = research.Status,
                AdvisorId = research.AdvisorId,
                ResearcherName = research.Researcher?.FullName,
                AdvisorName = research.Advisor?.FullName,
                CreatedAt = research.CreatedAt,
                UpdatedAt = research.UpdatedAt,
                Advisors = new SelectList(await GetAdvisors(), "Id", "FullName"),
                Students = research.ResearchStudents.Select(rs => new ResearchStudentViewModel
                {
                    Id = rs.Id,
                    StudentId = rs.StudentId,
                    StudentName = rs.Student?.FullName,
                    IsLeader = rs.IsLeader,
                    JoinedDate = rs.JoinedDate
                }).ToList(),
                AvailableStudents = new SelectList(availableStudents, "Id", "FullName")
            };

            return View(viewModel);
        }

        // POST: Research/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ResearchViewModel model)
        {
            ModelState.Clear();

            if (string.IsNullOrEmpty(model.Title))
            {
                ModelState.AddModelError("Title", "Title is required");
            }
            if (!model.AdvisorId.HasValue)
            {
                ModelState.AddModelError("AdvisorId", "Advisor is required");
            }

            if (!ModelState.IsValid)
            {
                model.Advisors = new SelectList(await GetAdvisors(), "Id", "FullName");
                return View(model);
            }

            try
            {
                var research = await _researchService.GetResearchByIdAsync(model.Id);
                if (research == null)
                    return NotFound();

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                if (research.ResearcherId != userId)
                    return Forbid();

                research.Title = model.Title;
                research.AdvisorId = model.AdvisorId;
                research.UpdatedAt = DateTime.UtcNow;

                await _researchService.UpdateResearchAsync(research);
                TempData["SuccessMessage"] = "Research project updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating research: {ex.Message}");
                model.Advisors = new SelectList(await GetAdvisors(), "Id", "FullName");
                return View(model);
            }
        }

        // POST: Research/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var research = await _researchService.GetResearchByIdAsync(id);
                if (research == null)
                    return NotFound();

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                if (research.ResearcherId != userId)
                    return Forbid();

                await _researchService.DeleteResearchAsync(id);
                TempData["SuccessMessage"] = "Research project deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting research: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Research/AddStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent([FromBody] AddStudentRequest request)
        {
            try
            {
                var research = await _researchService.GetResearchByIdAsync(request.ResearchId);
                if (research == null)
                    return NotFound();

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (research.ResearcherId != userId)
                    return Forbid();

                // ตรวจสอบว่านักศึกษาไม่มีกลุ่มจริงๆ
                var student = await _userService.GetUserByIdWithResearchStudentAsync(request.StudentId);
                if (student.ResearchParticipations.Any())
                    return BadRequest("This student already belongs to a research project.");

                // เช็คว่านักศึกษาอยู่ในงานวิจัยนี้แล้วหรือไม่
                var existingStudent = await _researchService.GetResearchStudentAsync(request.ResearchId, request.StudentId);
                if (existingStudent != null)
                    return BadRequest("Student is already in this research project.");

                // ถ้าจะตั้งเป็นหัวหน้า ต้องตรวจสอบว่าไม่ได้เป็นหัวหน้าที่อื่น
                if (request.IsLeader)
                {
                    var isLeaderInOtherResearch = await _researchService.IsStudentLeaderInAnyResearch(request.StudentId);
                    if (isLeaderInOtherResearch)
                        return BadRequest("This student is already a leader in another research project.");
                }

                var newResearchStudent = new ResearchStudent
                {
                    ResearchId = request.ResearchId,
                    StudentId = request.StudentId,
                    IsLeader = request.IsLeader,
                    JoinedDate = DateTime.Now
                };

                await _researchService.AddResearchStudentAsync(newResearchStudent);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error adding student: {ex.Message}");
            }
        }

        // POST: Research/RemoveStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStudent([FromBody] RemoveStudentRequest request)
        {
            try
            {
                var researchStudent = await _researchService.GetResearchStudentByIdAsync(request.ResearchStudentId);
                if (researchStudent == null)
                    return NotFound();

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var research = await _researchService.GetResearchByIdAsync(researchStudent.ResearchId);

                if (research.ResearcherId != userId)
                    return Forbid();

                // ไม่อนุญาตให้ลบหัวหน้ากลุ่ม
                if (researchStudent.IsLeader)
                    return BadRequest("Cannot remove research leader.");

                await _researchService.RemoveResearchStudentAsync(request.ResearchStudentId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error removing student: {ex.Message}");
            }
        }

        private async Task<IEnumerable<User>> GetAdvisors()
        {
            var users = await _userService.GetAllUsersWithDetailsAsync();
            return users.Where(u => u.Role == Enums.UserRole.Advisor)
                       .OrderBy(u => u.FullName);
        }
    }

    public class AddStudentRequest
    {
        public int ResearchId { get; set; }
        public int StudentId { get; set; }
        public bool IsLeader { get; set; }
    }

    public class RemoveStudentRequest
    {
        public int ResearchStudentId { get; set; }
    }
}