namespace EasyContainer.Service.Settings
{
    using System.Collections.Generic;
    using Lib;

    public class TargetLaneSettings : ISetting
    {
        public List<LaneSettings> LaneSettings { get; set; } = new List<LaneSettings>();
    }

    public class LaneSettings : ISetting
    {
        public string LaneId { get; set; }

        public int WindowCount { get; set; }

        public string TicketReleaseTime { get; set; }

        public int ExecutionDuration { get; set; }

        public string PortOfLoading { get; set; }

        public string PortOfDestination { get; set; }

        public string EarliestTimeOfDeparture { get; set; }

        public string LatestTimeOfDeparture { get; set; }

        public int HqQuantity { get; set; }

        public LaneSettings()
        {
        }

        public LaneSettings(LaneSettings laneSettings)
        {
            WindowCount = laneSettings.WindowCount;
            TicketReleaseTime = laneSettings.TicketReleaseTime;
            ExecutionDuration = laneSettings.ExecutionDuration;
        }
    }
}