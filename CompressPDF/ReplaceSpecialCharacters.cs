using System.Collections.Generic;
using System.Text;

namespace CompressPDF
{
    public class SpecialCharacterConverter
    {
        private static readonly Dictionary<string, string> SpecialCharacterMap = new Dictionary<string, string>
        {
            // Lower case czech characters
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
            // Upper case czech characters
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
            // Currency characters
            { "€", "EUR" },
            { "$", "USD" },
            { "£", "GBP" },
            { "¥", "JPY" },
            { "₫", "VND" },
            { "₿", "BTC" },
            { "₽", "RUB" }
            // Add more special character mappings as needed
        };

        public static string ReplaceSpecialCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            StringBuilder output = new StringBuilder(input);
            foreach (var pair in SpecialCharacterMap)
            {
                output.Replace(pair.Key, pair.Value);
            }

            return output.ToString();
        }
    }
}