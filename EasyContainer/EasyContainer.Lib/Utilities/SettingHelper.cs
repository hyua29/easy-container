namespace EasyContainer.Lib.Utilities
{
    using System;

    public static class SettingHelper
    {
        public static readonly string DateTimeFormat = "dd/MM/yyyy - HH:mm:ss";

        public static readonly string DateFormat = "dd/MM/yyyy";

        public static DateTime StringToDateTime(
            string datetimeString,
            DateTimeKind kind,
            IFormatProvider provider = null)
        {
            var result = DateTime.ParseExact(datetimeString, DateTimeFormat, provider);
            return DateTime.SpecifyKind(result, kind);
        }

        public static DateTime? StringToDateTimeNoThrow(
            string datetimeString,
            DateTimeKind kind,
            IFormatProvider provider = null)
        {
            try
            {
                return StringToDateTime(datetimeString, kind, provider);
            }
#pragma warning disable CS0168
            catch (FormatException e)
#pragma warning disable CS0168
            {
                return null;
            }
        }
        
        public static DateTime StringToDate(
            string dateString,
            DateTimeKind kind,
            IFormatProvider provider = null)
        {
            var result = DateTime.ParseExact(dateString, DateFormat, provider);
            return DateTime.SpecifyKind(result, kind);
        }
    }
}