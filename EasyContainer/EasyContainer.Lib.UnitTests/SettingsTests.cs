namespace EasyContainer.Lib.UnitTests
{
    using System.Collections.Generic;
    using NUnit.Framework;

    [TestFixture]
    public class SettingsTests
    {
        [Test]
        public void ToString_WithNestedSetting_Test()
        {
            var s1 = new MySettings1 {Message = "m1", MySettings1Nested = { Message = "mn1"}};
            var s2 = new MySettings1 {Message = "m2", MySettings1Nested = { Message = "mn2"}};
            var s3 = new MySettings1 {Message = "m1", MySettings1Nested = { Message = "mn1"}};

            Assert.That(s1.ToString(), Is.EqualTo(s3.ToString()));
            Assert.That(s1.ToString(), Is.Not.EqualTo(s2.ToString()));
        }

        [Test]
        public void ToString_WithNestedSettingList_Test()
        {
            var s1 = new MySettings2 {Message = "m1", MySettings2Nested = new List<MySettings2Nested>() { new MySettings2Nested { Key = "key1"}, new MySettings2Nested { Key = "key1"}}};
            var s2 = new MySettings2 {Message = "m2", MySettings2Nested = new List<MySettings2Nested>() { new MySettings2Nested { Key = "key2"}, new MySettings2Nested { Key = "key2"}}};
            var s3 = new MySettings2 {Message = "m1", MySettings2Nested = new List<MySettings2Nested>() { new MySettings2Nested { Key = "key1"}, new MySettings2Nested { Key = "key1"}}};

            var a = s1.ToString();
            Assert.That(s1.ToString(), Is.EqualTo(s3.ToString()));
            Assert.That(s1.ToString(), Is.Not.EqualTo(s2.ToString()));
        }
    }
}