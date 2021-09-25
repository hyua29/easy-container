namespace EasyContainer.Lib
{
    using System;

    public interface IDateTimeSupplier
    {
        DateTime UtcNow { get; }

        DateTime Now { get; }
    }

    public class DateTimeSupplier : IDateTimeSupplier
    {
        public DateTime UtcNow => DateTime.UtcNow;

        public DateTime Now => DateTime.Now;
    }
}