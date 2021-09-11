namespace EasyContainer.Lib.UnitTests
{
    using NUnit.Framework;

    [TestFixture]
    public class SettingsTests
    {
        [Test]
        public void ToString_Test()
        {
            var s1 = new MySettings1 {Message = "m1", MySettings1Nested = { Message = "mn1"}};
            var s2 = new MySettings1 {Message = "m2", MySettings1Nested = { Message = "mn2"}};
            var s3 = new MySettings1 {Message = "m1", MySettings1Nested = { Message = "mn1"}};

            Assert.That(s1.ToString(), Is.EqualTo(s3.ToString()));
            Assert.That(s1.ToString(), Is.Not.EqualTo(s2.ToString()));
        }
    }
}