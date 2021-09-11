namespace EasyContainer
{
    using Settings;

    public class AppSettings
    {
        public LoggingOptions Logging { get; set; }

        public FreightSmartOptions FreightSmart { get; set; }

        public SeleniumOptions Selenium { get; set; }
    }
}