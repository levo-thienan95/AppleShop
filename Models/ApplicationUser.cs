using Microsoft.AspNetCore.Identity;

namespace AppleStore.Models;

public class ApplicationUser : IdentityUser
{
    [System.ComponentModel.DataAnnotations.Display(Name = "Họ và tên")]
    public string? FullName { get; set; }

    [System.ComponentModel.DataAnnotations.Display(Name = "Địa chỉ")]
    public string? Address { get; set; }
}
