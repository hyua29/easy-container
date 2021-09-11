namespace EasyContainer.Lib
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IEventDebouncer : IDisposable
    {
        Task InvokeEventAsync(AsyncEventHandler eventHandler);

        Task InvokeEventAsync(EventHandler eventHandler);
    }

    public class EventDebouncer : BaseDisposable, IEventDebouncer
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly TimeSpan _debounceDuration;
        private int _counter;

        public EventDebouncer(TimeSpan? debounceDuration = null)
        {
            _debounceDuration = debounceDuration ?? TimeSpan.FromSeconds(3);
        }

        protected override void DisposeManagedResources()
        {
            _cts.Cancel();
        }

        public async Task InvokeEventAsync(AsyncEventHandler eventHandler)
        {
            var current = Interlocked.Increment(ref _counter);

            await Task.Delay(_debounceDuration, _cts.Token).ConfigureAwait(false);

            // Invoke onChange only once even if current method is called multiple times within waitTime
            if (current == _counter && !_cts.IsCancellationRequested)
            {
                if (eventHandler != null)
                {
                    await eventHandler.Invoke(this, EventArgs.Empty).ConfigureAwait(false);
                }
            }
        }

        public async Task InvokeEventAsync(EventHandler eventHandler)
        {
            var current = Interlocked.Increment(ref _counter);

            await Task.Delay(_debounceDuration, _cts.Token).ConfigureAwait(false);

            // Invoke onChange only once even if current method is called multiple times within waitTime
            if (current == _counter && !_cts.IsCancellationRequested)
            {
                eventHandler?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}