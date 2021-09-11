namespace EasyContainer.Service.Settings.SettingExtensions
{
    using System;
    using Lib;

    public static class RouteSettingsExtension
    {
        public static DateTime GetReleaseTimeDateTime(this RouteSettings routeSettings) =>
            string.IsNullOrWhiteSpace(routeSettings.ReleaseTime)
                ? DateTime.Now
                : SettingHelper.StringToDateTime(routeSettings.ReleaseTime, isUtc: false);
    }
}