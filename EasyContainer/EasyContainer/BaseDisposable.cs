namespace EasyContainer
{
    using System;

    public abstract class BaseDisposable : IDisposable
    {
        protected BaseDisposable()
        {
            IsDisposed = false;
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BaseDisposable()
        {
            Dispose(false);
        }

        protected void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            IsDisposed = true;

            if (disposing) DisposeManagedResources();

            DisposeUnmanagedResources();
        }

        protected abstract void DisposeManagedResources();

        protected virtual void DisposeUnmanagedResources()
        {
        }
    }
}