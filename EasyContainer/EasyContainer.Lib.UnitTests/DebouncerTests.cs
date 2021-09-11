namespace EasyContainer.Lib.UnitTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class DebouncerTests
    {
        private readonly TimeSpan _waitTime = TimeSpan.FromMilliseconds(500);
        private EventDebouncer _eventDebouncer;

        [SetUp]
        public void Setup()
        {
            _eventDebouncer = new EventDebouncer(_waitTime);
        }

        [Test]
        public async Task Debouce_HandleChange_Test()
        {
            long callCount = 0;

            Task AsyncEventHandler(object o, EventArgs arg)
            {
                // ReSharper disable once AccessToModifiedClosure - Intentionally modifying callCount
                Interlocked.Increment(ref callCount);
                return Task.CompletedTask;
            }

            for (var i = 0; i < 10; i++)
            {
#pragma warning disable 4014
                _eventDebouncer.InvokeEventAsync(AsyncEventHandler);
#pragma warning restore 4014
            }

            await Task.Delay(_waitTime + _waitTime).ConfigureAwait(false);

            Assert.That(Interlocked.Read(ref callCount), Is.EqualTo(1));
        }

        [TearDown]
        public void TearDown()
        {
            _eventDebouncer?.Dispose();
        }
    }
}