
namespace EasyContainer.Service.Settings.SettingExtensions
{
    using System;
    using Lib;

    public static class FreightSmartSettingsExtension
    {
        public static TimeSpan GetPreExecutionOffsetTimeSpan(this FreightSmartSettings fs) => TimeSpan.FromSeconds(fs.PreExecutionOffset);
        
        public static TimeSpan GetPreparationOffsetTimeSpan(this FreightSmartSettings fs) => TimeSpan.FromSeconds(fs.PreparationOffset);
    }
}