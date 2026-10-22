using eCommerceMotoRepuestos.Context;
using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace eCommerceMotoRepuestos.Services;

/// <summary>
/// Puts the public demo back to its initial state: seed database, seed images and known demo passwords.
/// </summary>
public class DemoResetService(
    AppDbContext _dbContext,
    DatabaseBackupService _backupService,
    SqliteDatabasePathProvider _databasePathProvider,
    AppSettingService _appSettingService,
    GenericRepository<User> _userRepository,
    IPasswordHasher<User> _passwordHasher,
    IWebHostEnvironment _environment,
    IOptions<DemoOptions> _demoOptions,
    ILogger<DemoResetService> _logger)
{
    // Only one reset may replace the database file at a time.
    private static readonly SemaphoreSlim ResetLock = new(1, 1);

    public async Task<DateTime> ResetAsync(CancellationToken cancellationToken = default)
    {
        await ResetLock.WaitAsync(cancellationToken);
        try
        {
            await _backupService.RestoreFromFileAsync(DemoSeeder.GetSeedDatabasePath(_databasePathProvider), cancellationToken);

            // Entities tracked before the file was replaced no longer match the database.
            _dbContext.ChangeTracker.Clear();

            await DatabaseInitializer.InitializeAsync(_dbContext, cancellationToken);
            await ApplyDemoPasswordsAsync();
            DemoSeeder.SyncImages(_databasePathProvider, _environment, removeUnknown: true);

            var resetAtUtc = DateTime.UtcNow;
            _appSettingService.InvalidateCache();
            await _appSettingService.SetLastDemoResetAsync(resetAtUtc);

            _logger.LogInformation("Demo data reset at {ResetAtUtc:O}.", resetAtUtc);
            return resetAtUtc;
        }
        finally
        {
            ResetLock.Release();
        }
    }

    /// <summary>
    /// Runs at startup: makes the demo credentials work and records a reset time for the session check.
    /// </summary>
    public async Task InitializeAsync()
    {
        await ApplyDemoPasswordsAsync();

        if (await _appSettingService.GetLastDemoResetAsync() is null)
        {
            await _appSettingService.SetLastDemoResetAsync(DateTime.UtcNow);
        }
    }

    private async Task ApplyDemoPasswordsAsync()
    {
        foreach (var account in _demoOptions.Value.Accounts)
        {
            var user = await _userRepository.GetByFilter([x => x.Email == account.Email]);
            if (user is null)
            {
                _logger.LogWarning("Demo account {Email} does not exist in the database.", account.Email);
                continue;
            }

            if (_passwordHasher.VerifyHashedPassword(user, user.Password, account.Password) == PasswordVerificationResult.Success)
            {
                continue;
            }

            user.Password = _passwordHasher.HashPassword(user, account.Password);
            await _userRepository.EditAsync(user);
        }
    }
}
