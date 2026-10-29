using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AppleStore.Models;

public static class EnumExtensions
{
    /// <summary>Lấy tên hiển thị của enum theo [Display(Name = "...")].</summary>
    public static string GetDisplayName(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        var attr = member?.GetCustomAttribute<DisplayAttribute>();
        return attr?.GetName() ?? value.ToString();
    }
}
