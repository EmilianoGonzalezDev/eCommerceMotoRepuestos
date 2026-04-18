using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace eCommerceMotoRepuestos.Enums;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        var memberInfo = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        
        if (memberInfo is null)
        {
            return value.ToString();
        }

        var displayAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();
        return displayAttribute?.GetName() ?? value.ToString();
    }
}
