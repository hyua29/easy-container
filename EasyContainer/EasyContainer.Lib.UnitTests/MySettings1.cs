namespace EasyContainer.Lib.UnitTests
{
    public class MySettings1 : ISetting
    {
        public MySettings1Nested MySettings1Nested { get; set; } = new MySettings1Nested();

        public string Message { get; set; }
        
        public string Name { get; set; }
    }

    public class MySettings1Nested : ISetting
    {
        public string Message { get; set; }
    }
}