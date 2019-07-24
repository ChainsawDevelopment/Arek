namespace Arek.Engine
{
    static class StringExtensions
    {
        public static int ToInt(this string value) => int.Parse(value);

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }


}