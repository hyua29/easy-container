namespace EasyContainer.Service.Settings
{
    using Lib;

    public class SeleniumSettings : ISetting
    {
        public bool AutoLogin { get; set; }
        
        public bool IsTesting { get; set; }
        
        public int PreparationDuration { get; set; }
        
        public int PreExecution { get; set; }
    }
}