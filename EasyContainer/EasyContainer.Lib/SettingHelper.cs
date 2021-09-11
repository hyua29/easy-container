namespace EasyContainer.Lib
{
    using System;

    public static class SettingHelper
    {
        public static DateTime StringToDateTime(string datetimeString, bool isUtc,
            IFormatProvider provider = null)
        {
            var result = DateTime.ParseExact(datetimeString, "dd/MM/yyyy - HH:mm:ss", provider);

            DateTime.SpecifyKind(result, isUtc ? DateTimeKind.Utc : DateTimeKind.Local);

            return result;
        }

        public static DateTime? StringToDateTimeNoThrow(string datetimeString, bool isUtc,
            IFormatProvider provider = null)
        {
            try
            {
                return StringToDateTime(datetimeString, isUtc, provider);
            }
#pragma warning disable CS0168
            catch (FormatException e)
#pragma warning disable CS0168
            {
                return null;
            }
        }
    }
}