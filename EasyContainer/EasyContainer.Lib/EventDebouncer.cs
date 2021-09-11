namespace EasyContainer.Lib
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class EventDebouncer : IDisposable
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly TimeSpan _waitTime;
        private int _counter;

        public EventDebouncer(TimeSpan? waitTime = null)
        {
            _waitTime = waitTime ?? TimeSpan.FromSeconds(3);
        }

        public void Dispose()
        {
            _cts.Cancel();
        }

        public event AsyncEventHandler Event;

        public virtual async Task InvokeEventAsync()
        {
            var current = Interlocked.Increment(ref _counter);

            await Task.Delay(_waitTime, _cts.Token).ConfigureAwait(false);

            // Invoke onChange only once even if current method is called multiple times within waitTime
            if (current == _counter && !_cts.IsCancellationRequested)
                if (Event != null)
                    await Event.Invoke(this, EventArgs.Empty).ConfigureAwait(false);
        }
    }
}