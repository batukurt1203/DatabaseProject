using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using DatabaseProject.Helpers;
using DatabaseProject.Models;
using DatabaseProject.Services;

namespace DatabaseProject.Controllers;

public class HomeController(SqlHelper sqlHelper, JwtService jwtService) : Controller {
   
    // Navigation to the person type selection page
    public IActionResult ChoosePersonType() {
        return View();
    }

    // Displays the login page and handles auto-login if token exists
    [HttpGet]
    public IActionResult Login() {
        var token = Request.Cookies["JWT"];
        if (token == null) return View();
        var principal = jwtService.ValidateToken(token);
        if (principal == null) return View();
        var role = principal.FindFirst(ClaimTypes.Role);
        if (role == null) return View();
        
        // Redirect based on user role
        return role.Value switch {
            "Employee" => RedirectToAction("EmployeeDashboard", "Employee"),
            "Manager" => RedirectToAction("ManagerDashboard", "Manager"),
            "Customer" => RedirectToAction("CustomerDashboard", "Customer"),
            _ => RedirectToAction("ChoosePersonType", "Home")
        };
    }

    // Handles the login logic and JWT token generation
    [HttpPost]
    public IActionResult Login(LoginViewModel model) {
        if (!ModelState.IsValid) return View(model);

        try {
            using (var connection = sqlHelper.OpenConnection()) {
                string query = @"
            SELECT PersonID 
            FROM [dbo].[PERSON] 
            WHERE Email = @Email AND PASSWORD_ = @Password AND PERSONTYPE = @PersonType";
                
                using (var command = new SqlCommand(query, connection)) {
                    SqlHelper.AddParameter(command, "@Email", SqlDbType.VarChar, model.UserEmail);
                    SqlHelper.AddParameter(command, "@Password", SqlDbType.VarChar, model.Password);
                    SqlHelper.AddParameter(command, "@PersonType", SqlDbType.VarChar, model.UserType);

                    var result = SqlHelper.ExecuteScalar(command);

                    if (result == null) {
                        ModelState.AddModelError(string.Empty, "Invalid email or password. Please try again.");
                        return View(model);
                    }

                    var userId = Convert.ToInt32(result);
                    var token = jwtService.GenerateToken(userId, model.UserEmail, model.UserType);

                    // Set JWT cookie
                    Response.Cookies.Append("JWT", token, new CookieOptions {
                        HttpOnly = true,
                        Secure = HttpContext.Request.IsHttps,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddMinutes(120)
                    });
                    
                    return model.UserType switch {
                        "Employee" => RedirectToAction("EmployeeDashboard", "Employee"),
                        "Manager" => RedirectToAction("ManagerDashboard", "Manager"),
                        "Customer" => RedirectToAction("CustomerDashboard", "Customer"),
                        _ => RedirectToAction("ChoosePersonType", "Home")
                    };
                }
            }
        } catch {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult CustomerRegister() {
        return View();
    }

    // Handles customer registration via Stored Procedure
    [HttpPost]
    public IActionResult CustomerRegister(CustomerRegisterViewModel model) {
        if (!ModelState.IsValid) {
            ViewBag.ErrorMessage = "Lütfen tüm alanları doldurun.";
            return View(model);
        }

        try {
            using (var connection = sqlHelper.OpenConnection()) {
                string storedProcedure = "pro_CreateCustomer";
                using (var command = new SqlCommand(storedProcedure, connection)) {
                    command.CommandType = CommandType.StoredProcedure;
                    // Adding parameters for customer creation
                    SqlHelper.AddParameter(command, "@FullNamePar", SqlDbType.NVarChar, model.FullName);
                    SqlHelper.AddParameter(command, "@HireDatePar", SqlDbType.Date, DateTime.Now);
                    SqlHelper.AddParameter(command, "@EmailPar", SqlDbType.VarChar, model.UserEmail);
                    SqlHelper.AddParameter(command, "@PasswordPar", SqlDbType.VarChar, model.Password);
                    SqlHelper.AddParameter(command, "@CompanyName", SqlDbType.NVarChar, model.CompanyName);

                    if (connection.State != ConnectionState.Open) connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            TempData["SuccessMessage"] = "Kayıt başarılı! Lütfen giriş yapın.";
            return RedirectToAction("Login", "Home");
        } catch (Exception ex) {
            ModelState.AddModelError(string.Empty, "Beklenmedik bir hata oluştu.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult EmployeeRegister() {
        return View();
    }

    // Logs out the user by deleting the cookie
    public IActionResult Logout()
    {
        Response.Cookies.Delete("JWT");
        return RedirectToAction("ChoosePersonType", "Home");
    }

    // Handles employee registration via Stored Procedure
    [HttpPost]
    public IActionResult EmployeeRegister(EmployeeRegisterViewModel model) {
        if (!ModelState.IsValid) {
            ViewBag.ErrorMessage = "Lütfen tüm alanları doldurun.";
            return View(model);
        }

        try {
            using (var connection = sqlHelper.OpenConnection()) {
                string storedProcedure = "pro_CreateEmployee";
                using (var command = new SqlCommand(storedProcedure, connection)) {
                    command.CommandType = CommandType.StoredProcedure;
                    // Adding parameters for employee creation
                    SqlHelper.AddParameter(command, "@FullNamePar", SqlDbType.NVarChar, model.FullName);
                    SqlHelper.AddParameter(command, "@HireDatePar", SqlDbType.Date, DateTime.Now);
                    SqlHelper.AddParameter(command, "@EmailPar", SqlDbType.VarChar, model.UserEmail);
                    SqlHelper.AddParameter(command, "@PasswordPar", SqlDbType.VarChar, model.Password);
                    SqlHelper.AddParameter(command, "@ManagerMail", SqlDbType.VarChar, model.UserEmail);

                    if (connection.State != ConnectionState.Open) connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            TempData["SuccessMessage"] = "Kayıt başarılı! Lütfen giriş yapın.";
            return RedirectToAction("Login", "Home");
        } catch (Exception ex) {
            ModelState.AddModelError(string.Empty, "Beklenmedik bir hata oluştu.");
            return View(model);
        }
    }
}