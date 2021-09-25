namespace EasyContainer.Lib.UnitTests.Utilities
{
    using Lib.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class HashHelperTests
    {
        [Test]
        public void ToSHA256_Test()
        {
            // Action
            var source = "source";
            var hash1 = HashHelper.ToSHA256(source);
            var sameHash = HashHelper.ToSHA256(source);
            var hash2 = HashHelper.ToSHA256(source + "diff");

            // Post-conditions
            Assert.That(hash1, Is.EqualTo(sameHash));
            Assert.That(hash1, Is.Not.EqualTo(hash2));
        }
    }
}