namespace EasyContainer.Service.Settings
{
    using Lib;

    public class FreightSmartSettings : ISetting
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string Domain { get; set; }

        public bool AutoLogin { get; set; }

        /// <summary>
        ///Determine how must earlier browsers will be opened
        /// </summary>
        public int PreparationOffset { get; set; }

        /// <summary>
        /// Determine how much earlier than actually ticket release time the program should attempt to purchase tickets
        /// </summary>
        public int PreExecutionOffset { get; set; }

        public bool IsTesting { get; set; }
    }
}