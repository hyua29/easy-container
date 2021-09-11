namespace EasyContainer.Service.Settings
{
    using System.Collections.Generic;
    using Lib;

    public class TargetRouteSettings : Setting
    {
        public List<RouteSettings> RouteSettings { get; set; } = new List<RouteSettings>();
    }

    public class RouteSettings : Setting
    {
        public int WindowCount { get; set; }

        public string ReleaseTime { get; set; }
    }
}