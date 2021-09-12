namespace EasyContainer.Service.Settings
{
    using Lib;

    public class FreightSmartSettings : ISetting
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string Domain { get; set; }
        
        public bool AutoLogin { get; set; }
    }
}