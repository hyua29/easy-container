
namespace EasyContainer.Service.Settings.SettingExtensions
{
    using System;
    using Lib;

    public static class FreightSmartSettingsExtension
    {
        public static TimeSpan GetPreExecutionTimeSpan(this FreightSmartSettings fs) => TimeSpan.FromSeconds(fs.PreExecution);
        
        public static TimeSpan GetPreparationOffsetTimeSpan(this FreightSmartSettings fs) => TimeSpan.FromSeconds(fs.PreparationOffset);
    }
}