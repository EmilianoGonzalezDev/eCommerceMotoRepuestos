using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Repositories;
using eCommerceMotoRepuestos.Utilities;

namespace eCommerceMotoRepuestos.Services;

public class AppSettingService(GenericRepository<AppSetting> _appSettingRepository)
{
    public const string LowStockThresholdKey = AppSettingsKeys.LowStockThreshold;
    private const int DefaultLowStockThreshold = 5;

    public async Task<int> GetLowStockThresholdAsync()
    {
        var setting = await _appSettingRepository.GetByFilter(
            [x => x.Key == LowStockThresholdKey]);

        if (setting is null)
        {
            return DefaultLowStockThreshold;
        }

        var isValidValue = int.TryParse(setting.Value, out var threshold) && threshold > 0;
        return isValidValue ? threshold : DefaultLowStockThreshold;
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
            return;
        }

        setting.Value = threshold.ToString();
        await _appSettingRepository.EditAsync(setting);
    }
}
