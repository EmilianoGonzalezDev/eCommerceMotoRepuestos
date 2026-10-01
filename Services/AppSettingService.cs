using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Repositories;
using eCommerceMotoRepuestos.Utilities;
using Microsoft.Extensions.Caching.Memory;

namespace eCommerceMotoRepuestos.Services;

public class AppSettingService(GenericRepository<AppSetting> _appSettingRepository, IMemoryCache _cache)
{
    public const string LowStockThresholdKey = AppSettingsKeys.LowStockThreshold;
    private const int DefaultLowStockThreshold = 5;
    private const string LowStockThresholdCacheKey = "AppSetting:" + LowStockThresholdKey;

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
        var setting = await _appSettingRepository.GetByFilter(
            [x => x.Key == LowStockThresholdKey]);

        if (setting is null)
        {
            await _appSettingRepository.AddAsync(new AppSetting
            {
                Key = LowStockThresholdKey,
                Value = threshold.ToString()
            });
        }
        else
        {
            setting.Value = threshold.ToString();
            await _appSettingRepository.EditAsync(setting);
        }

        InvalidateCache();
    }

    /// <summary>
    /// Clears cached settings. Call after the database is replaced (e.g. backup restore).
    /// </summary>
    public void InvalidateCache()
    {
        _cache.Remove(LowStockThresholdCacheKey);
    }
}
