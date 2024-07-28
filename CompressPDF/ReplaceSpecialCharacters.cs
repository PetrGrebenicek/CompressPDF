using System.Text;
using System.Globalization;

namespace CompressPDF
{
    public class SpecialCharacterConverter
    {
        private static readonly Dictionary<string, string> SpecialCharacterMap = new Dictionary<string, string>
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

        public static string ReplaceSpecialCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            StringBuilder output = new StringBuilder();
            foreach (char c in input)
            {
                string normalizedChar = c.ToString().Normalize(NormalizationForm.FormD);
                foreach (char nc in normalizedChar)
                {
                    UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(nc);
                    if (uc != UnicodeCategory.NonSpacingMark)
                    {
                        string key = nc.ToString();
                        if (SpecialCharacterMap.TryGetValue(key, out string replacement))
                        {
                            output.Append(replacement);
                        }
                        else
                        {
                            output.Append(nc);
                        }
                    }
                }
            }

            return output.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}