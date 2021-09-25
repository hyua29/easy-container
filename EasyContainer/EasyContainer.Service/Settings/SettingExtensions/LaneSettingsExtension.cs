namespace EasyContainer.Service.Settings.SettingExtensions
{
    using System;
    using Lib.Utilities;
    using Newtonsoft.Json;

    public static class LaneSettingsExtension
    {
        public static DateTime GetReleaseTimeDateTime(this LaneSettings laneSettings) =>
            string.IsNullOrWhiteSpace(laneSettings.TicketReleaseTime)
                ? DateTime.Now
                : SettingHelper.StringToDateTime(laneSettings.TicketReleaseTime, DateTimeKind.Local);

        public static string GetHash(this LaneSettings laneSettings) =>
            HashHelper.ToSHA256(JsonConvert.SerializeObject(laneSettings));

        public static DateTime GetEarliestTimeOfDepartureDate(this LaneSettings laneSettings) =>
            string.IsNullOrWhiteSpace(laneSettings.TicketReleaseTime)
                ? DateTime.Now
                : SettingHelper.StringToDate(laneSettings.EarliestTimeOfDeparture, DateTimeKind.Local);

        public static DateTime GetLatestTimeOfDepartureDate(this LaneSettings laneSettings) =>
            string.IsNullOrWhiteSpace(laneSettings.TicketReleaseTime)
                ? DateTime.Now
                : SettingHelper.StringToDate(laneSettings.LatestTimeOfDeparture, DateTimeKind.Local);
    }
}