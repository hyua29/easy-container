namespace EasyContainer.Service.Settings
{
    using System.Collections.Generic;
    using Lib;

    public class TargetRouteSettings : ISetting
    {
        public List<RouteSettings> RouteSettings { get; set; } = new List<RouteSettings>();
    }

    public class RouteSettings : ISetting
    {
        public int WindowCount { get; set; }

        public string TicketReleaseTime { get; set; }
        
        public int ExecutionDuration { get; set; }

        public RouteSettings()
        {
        }

        public RouteSettings(RouteSettings routeSettings)
        {
            WindowCount = routeSettings.WindowCount;
            TicketReleaseTime = routeSettings.TicketReleaseTime;
            ExecutionDuration = routeSettings.ExecutionDuration;
        }
    }
}