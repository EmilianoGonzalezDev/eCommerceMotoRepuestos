using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using eCommerceMotoRepuestos.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
    [EnableRateLimiting(RateLimitPolicies.Writes)]
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
