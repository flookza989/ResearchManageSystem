using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResearchManageSystem.Models;
using ResearchManageSystem.Services;
using OfficeOpenXml;
using System.Security.Claims;
using ResearchManageSystem.Data.Entities;
using ResearchManageSystem.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ResearchManageSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserManagementController(IUserService userService, IWebHostEnvironment webHostEnvironment)
        {
            _userService = userService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;

            var (users, totalCount) = await _userService.GetPagedUsersWithDetailsAsync(page, pageSize);

            var viewModel = new UserManagementViewModel
            {
                Users = users.Select(u => new UserListItemViewModel
                {
                    Id = u.Id,
                    Username = u.Username,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    DepartmentName = u.Department?.Name ?? string.Empty,
                    FacultyName = u.Department?.Faculty?.Name ?? string.Empty,
                    ProfileImagePath = u.ProfileImagePath ?? string.Empty
                }),
                Pagination = new PaginationInfo
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            };

            return View(viewModel);
        }

        public async Task<IActionResult> DownloadTemplate()
        {
            var departments = await _userService.GetAllDepartmentsWithFacultyAsync();

            var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                // สร้าง worksheet สำหรับ template users
                var userSheet = package.Workbook.Worksheets.Add("Users Template");

                // Add headers และ comments
                userSheet.Cells["A1"].Value = "Username";
                userSheet.Cells["A1"].AddComment("Required: Unique username for login", "System");

                userSheet.Cells["B1"].Value = "Password";
                userSheet.Cells["B1"].AddComment("Required: Initial password for user", "System");

                userSheet.Cells["C1"].Value = "FirstName";
                userSheet.Cells["D1"].Value = "LastName";
                userSheet.Cells["E1"].Value = "Email";
                userSheet.Cells["E1"].AddComment("Must be a valid email format", "System");

                userSheet.Cells["F1"].Value = "Role";
                userSheet.Cells["F1"].AddComment("Select role: 'Admin' , 'Advisor' , 'Student'", "System");

                userSheet.Cells["G1"].Value = "DepartmentId";
                userSheet.Cells["G1"].AddComment("Select department from Departments List sheet", "System");

                // กำหนดให้คอลัมน์เป็น Text/String
                for (int rowIndex = 2; rowIndex <= 1000; rowIndex++)
                {
                    // Username - Column A
                    userSheet.Cells[$"A{rowIndex}"].Style.Numberformat.Format = "@";
                    var usernameValidation = userSheet.DataValidations.AddCustomValidation($"A{rowIndex}");
                    usernameValidation.ShowInputMessage = true;
                    usernameValidation.PromptTitle = "Username";
                    usernameValidation.Prompt = "Enter username as text";
                    usernameValidation.ShowErrorMessage = true;
                    usernameValidation.ErrorTitle = "Invalid Username";
                    usernameValidation.Error = "Username must not be empty";
                    usernameValidation.Formula.ExcelFormula = $"AND(ISTEXT(A{rowIndex}),LEN(A{rowIndex})>0)";

                    // Password - Column B
                    userSheet.Cells[$"B{rowIndex}"].Style.Numberformat.Format = "@";
                    var passwordValidation = userSheet.DataValidations.AddCustomValidation($"B{rowIndex}");
                    passwordValidation.ShowInputMessage = true;
                    passwordValidation.PromptTitle = "Password";
                    passwordValidation.Prompt = "Enter password as text";
                    passwordValidation.ShowErrorMessage = true;
                    passwordValidation.ErrorTitle = "Invalid Password";
                    passwordValidation.Error = "Password must not be empty";
                    passwordValidation.Formula.ExcelFormula = $"AND(ISTEXT(B{rowIndex}),LEN(B{rowIndex})>0)";

                    // FirstName - Column C
                    userSheet.Cells[$"C{rowIndex}"].Style.Numberformat.Format = "@";
                    var firstNameValidation = userSheet.DataValidations.AddCustomValidation($"C{rowIndex}");
                    firstNameValidation.ShowInputMessage = true;
                    firstNameValidation.PromptTitle = "First Name";
                    firstNameValidation.Prompt = "Enter first name as text";
                    firstNameValidation.ShowErrorMessage = true;
                    firstNameValidation.ErrorTitle = "Invalid First Name";
                    firstNameValidation.Error = "First name must be text";
                    firstNameValidation.Formula.ExcelFormula = $"ISTEXT(C{rowIndex})";

                    // LastName - Column D
                    userSheet.Cells[$"D{rowIndex}"].Style.Numberformat.Format = "@";
                    var lastNameValidation = userSheet.DataValidations.AddCustomValidation($"D{rowIndex}");
                    lastNameValidation.ShowInputMessage = true;
                    lastNameValidation.PromptTitle = "Last Name";
                    lastNameValidation.Prompt = "Enter last name as text";
                    lastNameValidation.ShowErrorMessage = true;
                    lastNameValidation.ErrorTitle = "Invalid Last Name";
                    lastNameValidation.Error = "Last name must be text";
                    lastNameValidation.Formula.ExcelFormula = $"ISTEXT(D{rowIndex})";

                    // Email - Column E
                    userSheet.Cells[$"E{rowIndex}"].Style.Numberformat.Format = "@";
                    var emailValidation = userSheet.DataValidations.AddCustomValidation($"E{rowIndex}");
                    emailValidation.ShowInputMessage = true;
                    emailValidation.PromptTitle = "Email";
                    emailValidation.Prompt = "Enter valid email address";
                    emailValidation.ShowErrorMessage = true;
                    emailValidation.ErrorTitle = "Invalid Email";
                    emailValidation.Error = "Email must be in valid format";
                    emailValidation.Formula.ExcelFormula = $"AND(ISTEXT(E{rowIndex}),NOT(ISERROR(FIND(\"@\",E{rowIndex}))))";
                }

                // Styling headers
                var headerRange = userSheet.Cells["A1:G1"];
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                headerRange.Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);

                // Add validation dropdown for Role
                var rolesList = userSheet.DataValidations.AddListValidation("F2:F1000");
                rolesList.Formula.Values.Add("Admin");
                rolesList.Formula.Values.Add("Advisor");
                rolesList.Formula.Values.Add("Student");
                rolesList.ShowInputMessage = true;
                rolesList.PromptTitle = "Select Role";
                rolesList.Prompt = "Please select either 'Admin' , 'Advisor' , 'Student'";
                rolesList.ShowErrorMessage = true;
                rolesList.ErrorTitle = "Invalid Role";
                rolesList.Error = "Must be either 'Admin' , 'Advisor' , 'Student'";

                // สร้าง worksheet สำหรับ departments
                var deptSheet = package.Workbook.Worksheets.Add("Departments List");
                deptSheet.Cells["A1"].Value = "DepartmentId";
                deptSheet.Cells["B1"].Value = "Department Name";
                deptSheet.Cells["C1"].Value = "Faculty";

                // Styling department headers
                var deptHeaderRange = deptSheet.Cells["A1:C1"];
                deptHeaderRange.Style.Font.Bold = true;
                deptHeaderRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                deptHeaderRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                deptHeaderRange.Style.Font.Color.SetColor(System.Drawing.Color.DarkGreen);

                // เพิ่มข้อมูล departments
                int row = 2;
                foreach (var dept in departments)
                {
                    deptSheet.Cells[row, 1].Value = dept.Id;
                    deptSheet.Cells[row, 2].Value = dept.Name;
                    deptSheet.Cells[row, 3].Value = dept.Faculty?.Name;

                    // Alternate row coloring
                    if (row % 2 == 0)
                    {
                        deptSheet.Cells[$"A{row}:C{row}"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        deptSheet.Cells[$"A{row}:C{row}"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(242, 242, 242));
                    }

                    row++;
                }

                // Create named range for departments
                var deptRange = deptSheet.Cells[$"A2:A{row - 1}"];
                var deptName = package.Workbook.Names.Add("DepartmentIds", deptRange);

                // Add validation for DepartmentId using named range
                var deptValidation = userSheet.DataValidations.AddListValidation("G2:G1000");
                deptValidation.Formula.ExcelFormula = "DepartmentIds";
                deptValidation.ShowInputMessage = true;
                deptValidation.PromptTitle = "Select Department";
                deptValidation.Prompt = "Please select a department from the list";
                deptValidation.ShowErrorMessage = true;
                deptValidation.ErrorTitle = "Invalid Department";
                deptValidation.Error = "Must select a valid department from the list";

                // Border settings
                var borderStyle = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                var borderColor = System.Drawing.Color.Black;

                // Add borders to User Template
                var userRange = userSheet.Cells[userSheet.Dimension.Address];
                userRange.Style.Border.Top.Style = borderStyle;
                userRange.Style.Border.Left.Style = borderStyle;
                userRange.Style.Border.Right.Style = borderStyle;
                userRange.Style.Border.Bottom.Style = borderStyle;

                // Add borders to Departments List
                var deptFullRange = deptSheet.Cells[deptSheet.Dimension.Address];
                deptFullRange.Style.Border.Top.Style = borderStyle;
                deptFullRange.Style.Border.Left.Style = borderStyle;
                deptFullRange.Style.Border.Right.Style = borderStyle;
                deptFullRange.Style.Border.Bottom.Style = borderStyle;

                // Auto-fit columns
                userSheet.Cells[userSheet.Dimension.Address].AutoFitColumns();
                deptSheet.Cells[deptSheet.Dimension.Address].AutoFitColumns();

                // Freeze top row
                userSheet.View.FreezePanes(2, 1);
                deptSheet.View.FreezePanes(2, 1);

                // Add instruction sheet
                var instructionSheet = package.Workbook.Worksheets.Add("Instructions");
                instructionSheet.Cells["A1"].Value = "Instructions for User Import";
                instructionSheet.Cells["A1"].Style.Font.Bold = true;
                instructionSheet.Cells["A1"].Style.Font.Size = 14;

                string[] instructions = {
           "1. Fill user information in 'Users Template' sheet",
           "2. Username and Password are required",
           "3. Email must be in valid format",
           "4. Role must be either 'Admin' , 'Advisor' , 'Student'",
           "5. Select DepartmentId from dropdown (see 'Departments List' sheet)",
           "6. Do not modify the 'Departments List' sheet",
           "7. All columns must maintain their original format"
       };

                for (int i = 0; i < instructions.Length; i++)
                {
                    instructionSheet.Cells[i + 3, 1].Value = instructions[i];
                }

                instructionSheet.Column(1).AutoFit();

                package.Save();
            }
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "UserTemplate.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> UploadUsers(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a file to upload.";
                return RedirectToAction(nameof(Index));
            }

            var result = new UserImportResult();

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension?.Rows ?? 0;

                        // Skip if no data
                        if (rowCount <= 1)
                        {
                            TempData["Error"] = "The file contains no data.";
                            return RedirectToAction(nameof(Index));
                        }

                        // Process each row
                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var username = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                var password = worksheet.Cells[row, 2].Value?.ToString()?.Trim();

                                // Skip empty rows
                                if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
                                {
                                    continue;
                                }

                                // Validate required fields
                                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                                {
                                    result.Errors.Add($"Row {row}: Username and Password are required.");
                                    result.ErrorCount++;
                                    continue;
                                }

                                var user = new User
                                {
                                    Username = username,
                                    Password = password,
                                    FirstName = worksheet.Cells[row, 3].Value?.ToString()?.Trim() ?? string.Empty,
                                    LastName = worksheet.Cells[row, 4].Value?.ToString()?.Trim() ?? string.Empty,
                                    Email = worksheet.Cells[row, 5].Value?.ToString()?.Trim() ?? string.Empty,
                                    Role = Enum.Parse<UserRole>(
                                        worksheet.Cells[row, 6].Value?.ToString()?.Trim() ?? "User",
                                        true
                                    ),
                                    DepartmentId = int.Parse(worksheet.Cells[row, 7].Value?.ToString() ?? "0")
                                };

                                await _userService.CreateUserAsync(user);
                                result.SuccessCount++;
                            }
                            catch (Exception ex)
                            {
                                result.Errors.Add($"Row {row}: {ex.Message}");
                                result.ErrorCount++;
                            }
                        }
                    }
                }

                // Set messages
                if (result.SuccessCount > 0)
                {
                    TempData["SuccessMessage"] = $"Successfully imported {result.SuccessCount} users.";
                }

                if (result.ErrorCount > 0)
                {
                    TempData["Error"] = $"Failed to import {result.ErrorCount} users. Errors: " +
                        string.Join(" | ", result.Errors.Take(5)) +
                        (result.Errors.Count > 5 ? " | ..." : "");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error processing file: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserByIdWithDepartmentAndFacAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var departments = await _userService.GetAllDepartmentsWithFacultyAsync();

            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                DepartmentId = user.DepartmentId,
                Departments = departments.Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = $"{d.Faculty?.Name} - {d.Name}"
                })
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload departments for the view
                var departments = await _userService.GetAllDepartmentsWithFacultyAsync();
                model.Departments = departments.Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = $"{d.Faculty?.Name} - {d.Name}"
                });
                return View(model);
            }

            try
            {
                var existingUser = await _userService.GetUserByIdAsync(model.Id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                // Check if trying to change last admin's role
                if (existingUser.Role == UserRole.Admin && model.Role != UserRole.Admin)
                {
                    if (await _userService.IsLastAdminAsync(model.Id))
                    {
                        ModelState.AddModelError("", "Cannot change role of the last admin user.");
                        var departments = await _userService.GetAllDepartmentsWithFacultyAsync();
                        model.Departments = departments.Select(d => new SelectListItem
                        {
                            Value = d.Id.ToString(),
                            Text = $"{d.Faculty?.Name} - {d.Name}"
                        });
                        return View(model);
                    }
                }

                // Update user properties
                existingUser.FirstName = model.FirstName;
                existingUser.LastName = model.LastName;
                existingUser.Email = model.Email;
                existingUser.Role = model.Role;
                existingUser.DepartmentId = model.DepartmentId;

                // Update password only if provided
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    existingUser.Password = _userService.HashPassword(model.NewPassword);
                }

                await _userService.UpdateUserAsync(existingUser);
                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating user: {ex.Message}");
                var departments = await _userService.GetAllDepartmentsWithFacultyAsync();
                model.Departments = departments.Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = $"{d.Faculty?.Name} - {d.Name}"
                });
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                // Get current user's ID from claims
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                // Check if trying to delete own account
                if (id == currentUserId)
                {
                    TempData["Error"] = "You cannot delete your own account.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if trying to delete last admin
                if (await _userService.IsLastAdminAsync(id))
                {
                    TempData["Error"] = "Cannot delete the last admin user.";
                    return RedirectToAction(nameof(Index));
                }

                await _userService.DeleteUserAsync(id);
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting user: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: UserManagement/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var departments = await _userService.GetAllDepartmentsWithFacultyAsync();
            var viewModel = new CreateUserViewModel
            {
                Departments = new SelectList(departments, "Id", "Name")
            };
            return View(viewModel);
        }

        // POST: UserManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Departments = new SelectList(await _userService.GetAllDepartmentsWithFacultyAsync(), "Id", "Name");
                return View(model);
            }

            try
            {
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Role = model.Role,
                    DepartmentId = model.DepartmentId,
                    Password = _userService.HashPassword(model.Password)  // ใช้รหัสผ่านที่ผู้ใช้กำหนด
                };

                await _userService.CreateUserAsync(user);
                TempData["SuccessMessage"] = "User created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating user: " + ex.Message);
                model.Departments = new SelectList(await _userService.GetAllDepartmentsWithFacultyAsync(), "Id", "Name");
                return View(model);
            }
        }
    }
}