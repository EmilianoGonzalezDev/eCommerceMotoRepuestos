using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Repositories;
using eCommerceMotoRepuestos.Utilities;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;

namespace eCommerceMotoRepuestos.Services;

public class AppSettingService(GenericRepository<AppSetting> _appSettingRepository, IMemoryCache _cache)
{
    public const string LowStockThresholdKey = AppSettingsKeys.LowStockThreshold;
    private const int DefaultLowStockThreshold = 5;
    private const string LowStockThresholdCacheKey = "AppSetting:" + LowStockThresholdKey;
    private const string LastDemoResetCacheKey = "AppSetting:" + AppSettingsKeys.LastDemoReset;

    public async Task<int> GetLowStockThresholdAsync()
    {
        if (_cache.TryGetValue(LowStockThresholdCacheKey, out int cachedThreshold))
        {
            return cachedThreshold;
        }

        var setting = await _appSettingRepository.GetByFilter(
            [x => x.Key == LowStockThresholdKey]);

        var isValidValue = int.TryParse(setting?.Value, out var threshold) && threshold > 0;
        var result = isValidValue ? threshold : DefaultLowStockThreshold;

        _cache.Set(LowStockThresholdCacheKey, result);
        return result;
    }

    public async Task SetLowStockThresholdAsync(int threshold)
    {
        await SetValueAsync(LowStockThresholdKey, threshold.ToString());
        InvalidateCache();
    }

    /// <summary>
    /// UTC time of the last demo data reset, or null if the data was never reset.
    /// </summary>
    public async Task<DateTime?> GetLastDemoResetAsync()
    {
        if (_cache.TryGetValue(LastDemoResetCacheKey, out DateTime? cachedValue))
        {
            return cachedValue;
        }

        var setting = await _appSettingRepository.GetByFilter(
            [x => x.Key == AppSettingsKeys.LastDemoReset]);

        DateTime? result = DateTime.TryParse(setting?.Value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var lastReset)
            ? lastReset
            : null;

        _cache.Set(LastDemoResetCacheKey, result);
        return result;
    }

    public async Task SetLastDemoResetAsync(DateTime resetAtUtc)
    {
        await SetValueAsync(AppSettingsKeys.LastDemoReset, resetAtUtc.ToString("O", CultureInfo.InvariantCulture));
        InvalidateCache();
    }

    /// <summary>
    /// Clears cached settings. Call after the database is replaced (e.g. backup restore).
    /// </summary>
    public void InvalidateCache()
    {
        _cache.Remove(LowStockThresholdCacheKey);
        _cache.Remove(LastDemoResetCacheKey);
    }

    private async Task SetValueAsync(string key, string value)
    {
        var setting = await _appSettingRepository.GetByFilter(
            [x => x.Key == key]);

        if (setting is null)
        {
            await _appSettingRepository.AddAsync(new AppSetting
            {
                Key = key,
                Value = value
            });
        }
        else
        {
            setting.Value = value;
            await _appSettingRepository.EditAsync(setting);
        }
    }
}
