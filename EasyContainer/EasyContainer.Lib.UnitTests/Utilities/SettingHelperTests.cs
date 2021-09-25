namespace EasyContainer.Lib.UnitTests.Utilities
{
    using System;
    using Lib.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class SettingHelperTests
    {
        [Test]
        public void StringToDateTime_Test([Values] DateTimeKind kind)
        {
            var dt = SettingHelper.StringToDateTime("10/12/1995 - 15:30:50", kind);
            var expectedDt = new DateTime(1995, 12, 10, 15, 30, 50, kind);
            Assert.That(dt.Kind, Is.EqualTo(kind));
            Assert.That(dt.Ticks, Is.EqualTo(expectedDt.Ticks));
        }
        
        [Test]
        public void StringToDate_Test([Values] DateTimeKind kind)
        {
            var dt = SettingHelper.StringToDate("10/12/1995", kind);
            var expectedDt = new DateTime(1995, 12, 10, 0, 0, 0, kind);
            Assert.That(dt.Kind, Is.EqualTo(kind));
            Assert.That(dt.Ticks, Is.EqualTo(expectedDt.Ticks));
        }
    }
}