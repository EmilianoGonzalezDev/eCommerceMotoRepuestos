using System.ComponentModel.DataAnnotations;

namespace eCommerceMotoRepuestos.Models;

public class StockSettingsViewModel
{
    [Display(Name = "Stock Bajo")]
    [Range(1, 9999, ErrorMessage = "Ingrese un valor entre 1 y 9999")]
    public int LowStockThreshold { get; set; }
}
