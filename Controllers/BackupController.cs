using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceMotoRepuestos.Controllers;

[Authorize(Roles = "Admin")]
public class BackupController(DatabaseBackupService backupService, ILogger<BackupController> logger) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View("~/Views/Backups/Index.cshtml", BuildViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        try
        {
            var filePath = await backupService.CreateBackupAsync(cancellationToken);
            TempData["SuccessMessage"] = $"Backup generado correctamente en: {filePath}";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al generar backup.");
            TempData["ErrorMessage"] = "No se pudo generar el backup de la base de datos.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(CancellationToken cancellationToken)
    {
        try
        {
            await backupService.RestoreBackupAsync(cancellationToken);
            TempData["SuccessMessage"] = "Base de datos restaurada correctamente desde el backup.";
        }
        catch (FileNotFoundException)
        {
            TempData["ErrorMessage"] = "No existe backup para restaurar.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al restaurar backup.");
            TempData["ErrorMessage"] = "No se pudo restaurar la base de datos.";
        }

        return RedirectToAction(nameof(Index));
    }

    private BackupViewModel BuildViewModel()
    {
        var backupPath = backupService.GetBackupFilePath();
        var backupLastWriteUtc = backupService.GetBackupLastWriteUtc();

        return new BackupViewModel
        {
            BackupFilePath = backupPath,
            BackupExists = backupLastWriteUtc.HasValue,
            BackupLastWriteLocal = backupLastWriteUtc?.ToLocalTime()
        };
    }
}
