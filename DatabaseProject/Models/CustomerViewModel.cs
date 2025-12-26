using System.ComponentModel.DataAnnotations;

namespace DatabaseProject.Models;

public class CustomerViewModel
{
    public int PersonID { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
}