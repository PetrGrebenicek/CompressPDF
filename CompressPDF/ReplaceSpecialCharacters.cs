using System.Text;
using System.Globalization;

namespace CompressPDF
{
    public class SpecialCharacterConverter
    {
        #region SpecialCharacterMap
        private static readonly Dictionary<string, string> SpecialCharacterMap = new()
        {
            { "á", "a" },
            { "é", "e" },
            { "í", "i" },
            { "ó", "o" },
            { "ú", "u" },
            { "ý", "y" },
            { "č", "c" },
            { "ď", "d" },
            { "ě", "e" },
            { "ň", "n" },
            { "ř", "r" },
            { "š", "s" },
            { "ť", "t" },
            { "ž", "z" },
            { "ů", "u" },
            { "Á", "A" },
            { "É", "E" },
            { "Í", "I" },
            { "Ó", "O" },
            { "Ú", "U" },
            { "Ý", "Y" },
            { "Č", "C" },
            { "Ď", "D" },
            { "Ě", "E" },
            { "Ň", "N" },
            { "Ř", "R" },
            { "Š", "S" },
            { "Ť", "T" },
            { "Ž", "Z" },
            { "Ů", "U" },
            { "€", "EUR" },
            { "$", "USD" },
            { "£", "GBP" },
            { "¥", "JPY" },
            { "₫", "VND" },
            { "₿", "BTC" },
            { "₽", "RUB" },
            { "%20", " " }
        };
        #endregion

        #region ReplaceSpecialCharacters
        public static string ReplaceSpecialCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            StringBuilder output = new();

            // First pass: Replace special characters based on the map
            foreach (char c in input)
            {
                string normalizedChar = c.ToString().Normalize(NormalizationForm.FormD);
                foreach (char nc in normalizedChar)
                {
                    UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(nc);
                    if (uc != UnicodeCategory.NonSpacingMark)
                    {
                        string key = nc.ToString();
                        if (SpecialCharacterMap.TryGetValue(key, out string? replacement))
                        {
                            output.Append(replacement ?? key);
                        }
                        else
                        {
                            output.Append(nc);
                        }
                    }
                }
            }

            string intermediateResult = output.ToString().Normalize(NormalizationForm.FormC);

            // Clear the output StringBuilder for the second pass
            output.Clear();

            // Second pass: Remove characters not in "a-z", "A-Z", "0-9", " ", "_", "-", "."
            foreach (char c in intermediateResult)
            {
                if (char.IsLetterOrDigit(c) || c == ' ' || c == '_' || c == '-' || c == '.')
                {
                    output.Append(c);
                }
            }

            return output.ToString();
        }
        #endregion
    }
}