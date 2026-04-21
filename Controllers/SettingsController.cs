using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceMotoRepuestos.Controllers;

[Authorize(Roles = "Admin")]
public class SettingsController(AppSettingService _appSettingService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var viewModel = new StockSettingsViewModel
        {
            LowStockThreshold = await _appSettingService.GetLowStockThresholdAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(StockSettingsViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        await _appSettingService.SetLowStockThresholdAsync(viewModel.LowStockThreshold);
        TempData["SuccessMessage"] = "Configuración guardada.";
        return RedirectToAction(nameof(Index));
    }
}
