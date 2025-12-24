using System.ComponentModel.DataAnnotations;

namespace DatabaseProject.Models;

public class BuyLicenseModel
{
    [Required]
    public int LicenseTerm { get; set; }

    [Required]
    [Display(Name = "Product Name")]
    public string ProductName { get; set; }

   
}
