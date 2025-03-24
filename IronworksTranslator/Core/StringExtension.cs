namespace IronworksTranslator.Core
{
    public static class StringExtension
    {
        public static string RemoveAfter(this string value, string character)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(character))
                return value;
                
            int index = value.IndexOf(character);
            if (index > 0 && index < value.Length)
            {
                value = value.Substring(0, index);
            }
            return value;
        }

        public static string RemoveBefore(this string value, string character)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(character))
                return value;
                
            int index = value.IndexOf(character);
            if (index >= 0 && index < value.Length - 1)
            {
                value = value.Substring(index + 1);
            }
            return value;
        }
    }
}
