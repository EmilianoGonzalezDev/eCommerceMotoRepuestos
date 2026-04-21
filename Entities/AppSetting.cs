using System.ComponentModel.DataAnnotations;

namespace eCommerceMotoRepuestos.Entities;

public class AppSetting
{
    public int AppSettingId { get; set; }

    [Required]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;
}
