namespace EasyContainer.Lib.UnitTests.Utilities
{
    using System;
    using Lib.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class SettingHelperTests
    {
        [Test]
        public void StringToDateTime_Test([Values] bool isUtc)
        {
            var dt = SettingHelper.StringToDateTime("10/12/1995 - 15:30:50", isUtc);
            var expectedDt = new DateTime(1995, 12, 10, 15, 30, 50, isUtc ? DateTimeKind.Utc : DateTimeKind.Local);
            Assert.That(dt.Kind == DateTimeKind.Utc, Is.EqualTo(isUtc));
            Assert.That(dt.Ticks, Is.EqualTo(expectedDt.Ticks));
        }
    }
}