using System.Text;
using System.Text.RegularExpressions;

namespace CompressPDF
{
    public class SpecialCharacterConverter
    {
        #region SpecialCharacterMap
        private static readonly Dictionary<string, string> SpecialCharacterMap = new()
        {
            { "€", "EUR" },
            { "$", "USD" },
            { "£", "GBP" },
            { "¥", "JPY" },
            { "₫", "VND" },
            { "₿", "BTC" },
            { "₽", "RUB" }
        };
        #endregion

        #region ReplaceSpecialCharacters
        public static string ReplaceSpecialCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // First, replace specific currency symbols
            foreach (var kvp in SpecialCharacterMap)
            {
                input = input.Replace(kvp.Key, kvp.Value);
            }

            // Remove diacritical marks and normalize to ASCII
            string normalizedString = RemoveDiacritics(input);

            // Remove any remaining non-ASCII characters
            normalizedString = Regex.Replace(normalizedString, @"[^a-zA-Z0-9 _\-\.]", "");

            return normalizedString;
        }
        #endregion

        #region RemoveDiacritics
        private static string RemoveDiacritics(string input)
        {
            var normalizedString = input.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
        #endregion
    }
}