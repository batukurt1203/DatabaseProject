using System.Data;
using System.Security.Claims;
using DatabaseProject.Helpers;
using DatabaseProject.Models;
using DatabaseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseProject.Controllers;

[Authorize(Roles = "Employee")]
public class EmployeeController(SqlHelper sqlHelper, JwtService jwtService) : Controller {
    
    [HttpGet]
    public IActionResult EmployeeDashboard() {
        return View();
    }
    
    
    [HttpGet]
    public IActionResult GetProjects() {
        var token = Request.Cookies["JWT"];
        if (token == null) return View();

        var principal = jwtService.ValidateToken(token);
        if (principal == null) return View();


        var employeeEmail = principal.FindFirst(ClaimTypes.Email)?.Value;
        var projects = new List<EmployeeProjectsViewModel>();

        using (var command = sqlHelper.CreateCommand("pro_VIEW_ALL_PROJECT_FOR_EMPLOYEE")) {
            command.CommandType = CommandType.StoredProcedure;
            SqlHelper.AddParameter(command, "@empMail", SqlDbType.NVarChar, employeeEmail);
            sqlHelper.OpenConnection();

            using (var reader = SqlHelper.ExecuteReader(command)) {
                while (reader.Read()) {
                    projects.Add(new EmployeeProjectsViewModel {
                        Name = reader["PName"].ToString(),
                        Description = reader["PDescription"].ToString(),
                        ReleaseDate = Convert.ToDateTime(reader["ReleaseDate"])
                    });
                }
            }

            sqlHelper.CloseConnection();
        }

        return View(projects);
    }

    [HttpGet]
    public IActionResult ViewTickets()
    {
        // 1. Token ve Kimlik Doğrulama
        var token = Request.Cookies["JWT"];
        if (token == null) return RedirectToAction("Login", "Account");

        var principal = jwtService.ValidateToken(token);
        if (principal == null) return RedirectToAction("Login", "Account");

        var employeeEmail = principal.FindFirst(ClaimTypes.Email)?.Value;

        // 2. Verileri Çekme
        var tickets = new List<EmployeeTicketViewModel>();

        using (var command = sqlHelper.CreateCommand("EXEC pro_GET_ASSIGNED_TICKETS @EmployeeEmail"))
        {
            SqlHelper.AddParameter(command, "@EmployeeEmail", SqlDbType.NVarChar, employeeEmail);

            sqlHelper.OpenConnection();
            using (var reader = SqlHelper.ExecuteReader(command))
            {
                while (reader.Read())
                {
                    tickets.Add(new EmployeeTicketViewModel
                    {
                        TicketID = Convert.ToInt32(reader["TicketID"]),
                        ProductName = reader["ProductName"].ToString(),
                        CustomerName = reader["CustomerName"].ToString(),
                        DateOpened = Convert.ToDateTime(reader["DateOpened"]),
                        Status = reader["Status"].ToString(),
                        IssueDescription = reader["IssueDescription"].ToString()
                    });
                }
            }
            sqlHelper.CloseConnection();
        }

        return View(tickets);
    }


    public IActionResult Logout()
    {
            
        Response.Cookies.Delete("JWT");
            
        return RedirectToAction("ChoosePersonType", "Home");
    }
  
}