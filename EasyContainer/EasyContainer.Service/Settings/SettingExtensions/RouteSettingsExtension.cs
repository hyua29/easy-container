namespace EasyContainer.Service.Settings.SettingExtensions
{
    using System;
    using Lib;

    public static class RouteSettingsExtension
    {
        public static DateTime GetReleaseTimeDateTime(this RouteSettings routeSettings) =>
            string.IsNullOrWhiteSpace(routeSettings.TicketReleaseTime)
                ? DateTime.Now
                : SettingHelper.StringToDateTime(routeSettings.TicketReleaseTime, isUtc: false);
    }
}