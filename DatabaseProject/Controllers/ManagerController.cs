using System.Data;
using System.Security.Claims;
using DatabaseProject.Helpers;
using DatabaseProject.Models;
using DatabaseProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseProject.Controllers;

public class ManagerController(SqlHelper sqlHelper, JwtService jwtService) : Controller
{

    private int? GetManagerPersonId()
    {
        var token = Request.Cookies["JWT"];
        if (token == null) return null;
        var principal = jwtService.ValidateToken(token);
        if (principal == null) return null;

        var managerEmail = principal.FindFirst(ClaimTypes.Email)?.Value;

        using (var command = sqlHelper.CreateCommand("SELECT PersonID FROM Person WHERE Email = @email"))
        {
            SqlHelper.AddParameter(command, "@email", SqlDbType.VarChar, managerEmail);
            sqlHelper.OpenConnection();
            var result = command.ExecuteScalar();
            sqlHelper.CloseConnection();
            return result != null ? Convert.ToInt32(result) : null;
        }
    }

    [HttpGet]
    public IActionResult ManagerDashboard()
    {
        return View();
    }

    public IActionResult GetBugReports()
    {
        var token = Request.Cookies["JWT"];
        if (token == null) return View();
        var principal = jwtService.ValidateToken(token);
        if (principal == null) return View();
        var managerEmail = principal.FindFirst(ClaimTypes.Email)?.Value;
        var bugReports = new List<GetBugReportViewModel>();
        using (var command = sqlHelper.CreateCommand("pro_VIEW_BUG_REPORTS_MANAGER"))
        {
            command.CommandType = CommandType.StoredProcedure;
            SqlHelper.AddParameter(command, "@managerMail", SqlDbType.VarChar, managerEmail);
            sqlHelper.OpenConnection();
            using (var reader = SqlHelper.ExecuteReader(command))
            {
                while (reader.Read())
                {
                    bugReports.Add(new GetBugReportViewModel
                    {
                        ProductName = reader["PName"].ToString(),
                        VersionID = reader["VersionID"].ToString(),
                        Message = reader["Message_"].ToString(),
                        CompanyName = reader["CompanyName"].ToString(),
                        FeedbackDate = Convert.ToDateTime(reader["FeedbackDate"])
                    });
                }
            }
            sqlHelper.CloseConnection();
        }
        return View(bugReports);
    }

    public IActionResult GetFeatureRequests()
    {
        var token = Request.Cookies["JWT"];
        if (token == null) return View();
        var principal = jwtService.ValidateToken(token);
        if (principal == null) return View();
        var managerEmail = principal.FindFirst(ClaimTypes.Email)?.Value;
        var featureRequests = new List<GetFeatureRequestViewModel>();
        using (var command = sqlHelper.CreateCommand("pro_VIEW_FEATURE_REQUESTS_MANAGER"))
        {
            command.CommandType = CommandType.StoredProcedure;
            SqlHelper.AddParameter(command, "@managerMail", SqlDbType.VarChar, managerEmail);
            sqlHelper.OpenConnection();
            using (var reader = SqlHelper.ExecuteReader(command))
            {
                while (reader.Read())
                {
                    featureRequests.Add(new GetFeatureRequestViewModel
                    {
                        ProductName = reader["PName"].ToString(),
                        Rating = Convert.ToInt32(reader["Rating"]),
                        Message = reader["Message_"].ToString(),
                        CompanyName = reader["CompanyName"].ToString(),
                        FeedbackDate = Convert.ToDateTime(reader["FeedbackDate"])
                    });
                }
            }
            sqlHelper.CloseConnection();
        }
        return View(featureRequests);
    }

    // EMPLOYEE MANAGEMENT
    [HttpGet]
    public IActionResult ManageEmployees()
    {
        var managerPersonId = GetManagerPersonId();
        if (managerPersonId == null) return RedirectToAction("ChoosePersonType", "Home");

        var employees = new List<EmployeeViewModel>();

        try
        {
            using (var command = sqlHelper.CreateCommand(@"
                SELECT PersonID, FullName, Email
                FROM Person
                WHERE PersonType = 'Employee'
            "))
            {
                sqlHelper.OpenConnection();
                using (var reader = SqlHelper.ExecuteReader(command))
                {
                    while (reader.Read())
                    {
                        employees.Add(new EmployeeViewModel
                        {
                            PersonID = Convert.ToInt32(reader["PersonID"]),
                            FullName = reader["FullName"]?.ToString() ?? "",
                            Email = reader["Email"]?.ToString() ?? ""
                        });
                    }
                }
                sqlHelper.CloseConnection();
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Employee listesi yüklenirken hata oluştu: " + ex.Message;
        }

        return View(employees);
    }

    [HttpPost]
    public IActionResult AddEmployee(string fullName, string email)
    {
        var managerPersonId = GetManagerPersonId();
        if (managerPersonId == null) return RedirectToAction("ChoosePersonType", "Home");

        try
        {
            using (var command = sqlHelper.CreateCommand(@"
                INSERT INTO Person (FullName, Email, Password, PersonType, HireDate)
                VALUES (@fullName, @email, '0000', 'Employee', GETDATE());
            "))
            {
                SqlHelper.AddParameter(command, "@fullName", SqlDbType.VarChar, fullName);
                SqlHelper.AddParameter(command, "@email", SqlDbType.VarChar, email);

                sqlHelper.OpenConnection();
                command.ExecuteNonQuery();
                sqlHelper.CloseConnection();
            }
            TempData["SuccessMessage"] = "Employee başarıyla eklendi. Varsayılan şifre: 0000";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Employee eklenirken hata oluştu: " + ex.Message;
        }

        return RedirectToAction("ManageEmployees");
    }

    [HttpPost]
    public IActionResult DeleteEmployee(int personId)
    {
        var managerPersonId = GetManagerPersonId();
        if (managerPersonId == null) return RedirectToAction("ChoosePersonType", "Home");

        try
        {
            using (var command = sqlHelper.CreateCommand(@"
                DELETE FROM Employee WHERE EmployeeID = @personId;
                DELETE FROM Person WHERE PersonID = @personId AND PersonType = 'Employee';
            "))
            {
                SqlHelper.AddParameter(command, "@personId", SqlDbType.Int, personId);

                sqlHelper.OpenConnection();
                var rowsAffected = command.ExecuteNonQuery();
                sqlHelper.CloseConnection();

                if (rowsAffected > 0)
                {
                    TempData["SuccessMessage"] = "Employee başarıyla silindi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Employee bulunamadı.";
                }
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Employee silinirken hata oluştu: " + ex.Message;
        }

        return RedirectToAction("ManageEmployees");
    }

    // CUSTOMER MANAGEMENT
    [HttpGet]
    public IActionResult ManageCustomers()
    {
        var managerPersonId = GetManagerPersonId();
        if (managerPersonId == null) return RedirectToAction("ChoosePersonType", "Home");

        var customers = new List<CustomerViewModel>();

        try
        {
            using (var command = sqlHelper.CreateCommand(@"
                SELECT PersonID, FullName, Email
                FROM Person
                WHERE PersonType = 'Customer'
            "))
            {
                sqlHelper.OpenConnection();
                using (var reader = SqlHelper.ExecuteReader(command))
                {
                    while (reader.Read())
                    {
                        customers.Add(new CustomerViewModel
                        {
                            PersonID = Convert.ToInt32(reader["PersonID"]),
                            FullName = reader["FullName"]?.ToString() ?? "",
                            Email = reader["Email"]?.ToString() ?? ""
                        });
                    }
                }
                sqlHelper.CloseConnection();
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Customer listesi yüklenirken hata oluştu: " + ex.Message;
        }

        return View(customers);
    }

    [HttpPost]
    public IActionResult AddCustomer(string fullName, string email)
    {
        var managerPersonId = GetManagerPersonId();
        if (managerPersonId == null) return RedirectToAction("ChoosePersonType", "Home");

        try
        {
            using (var command = sqlHelper.CreateCommand(@"
                INSERT INTO Person (FullName, Email, Password, PersonType, HireDate)
                VALUES (@fullName, @email, '0000', 'Customer', GETDATE());
            "))
            {
                SqlHelper.AddParameter(command, "@fullName", SqlDbType.VarChar, fullName);
                SqlHelper.AddParameter(command, "@email", SqlDbType.VarChar, email);

                sqlHelper.OpenConnection();
                command.ExecuteNonQuery();
                sqlHelper.CloseConnection();
            }
            TempData["SuccessMessage"] = "Customer başarıyla eklendi. Varsayılan şifre: 0000";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Customer eklenirken hata oluştu: " + ex.Message;
        }

        return RedirectToAction("ManageCustomers");
    }

    [HttpPost]
    public IActionResult DeleteCustomer(int personId)
    {
        var managerPersonId = GetManagerPersonId();
        if (managerPersonId == null) return RedirectToAction("ChoosePersonType", "Home");

        try
        {
            using (var command = sqlHelper.CreateCommand(@"
                DELETE FROM Customer WHERE CustomerID = @personId;
                DELETE FROM Person WHERE PersonID = @personId AND PersonType = 'Customer';
            "))
            {
                SqlHelper.AddParameter(command, "@personId", SqlDbType.Int, personId);

                sqlHelper.OpenConnection();
                var rowsAffected = command.ExecuteNonQuery();
                sqlHelper.CloseConnection();

                if (rowsAffected > 0)
                {
                    TempData["SuccessMessage"] = "Customer başarıyla silindi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Customer bulunamadı.";
                }
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Customer silinirken hata oluştu: " + ex.Message;
        }

        return RedirectToAction("ManageCustomers");
    }

    public IActionResult Logout()
    {
        Response.Cookies.Delete("JWT");
        return RedirectToAction("ChoosePersonType", "Home");
    }
}