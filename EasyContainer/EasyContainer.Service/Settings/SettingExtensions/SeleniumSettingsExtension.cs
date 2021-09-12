
namespace EasyContainer.Service.Settings.SettingExtensions
{
    using System;
    using Lib;

    public static class SeleniumSettingsExtension
    {
        public static TimeSpan GetPreExecutionTimeSpan(this SeleniumSettings seleniumSettings) => TimeSpan.FromSeconds(seleniumSettings.PreExecution);
        
        public static TimeSpan GetPreparationDurationTimeSpan(this SeleniumSettings seleniumSettings) => TimeSpan.FromSeconds(seleniumSettings.PreparationDuration);
    }
}