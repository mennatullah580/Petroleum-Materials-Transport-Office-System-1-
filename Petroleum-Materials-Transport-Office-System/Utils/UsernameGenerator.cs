using System.Text;

namespace Petroleum_Materials_Transport_Office_System.Utils
{
    public static class UsernameGenerator
    {
        private static readonly Dictionary<char, string> ArabicToLatin = new()
        {
            {'ا', "a"}, {'ب', "b"}, {'ت', "t"}, {'ث', "th"}, {'ج', "j"}, {'ح', "h"}, {'خ', "kh"},
            {'د', "d"}, {'ذ', "th"}, {'ر', "r"}, {'ز', "z"}, {'س', "s"}, {'ش', "sh"}, {'ص', "s"},
            {'ض', "d"}, {'ط', "t"}, {'ظ', "th"}, {'ع', "a"}, {'غ', "gh"}, {'ف', "f"}, {'ق', "q"},
            {'ك', "k"}, {'ل', "l"}, {'م', "m"}, {'ن', "n"}, {'ه', "h"}, {'و', "w"}, {'ي', "y"},
            {'ى', "a"}, {'ة', "a"}, {'أ', "a"}, {'إ', "a"}, {'آ', "a"}, {'ئ', "y"}, {'ؤ', "w"}, {'ء', ""},
            {'َ', ""}, {'ً', ""}, {'ُ', ""}, {'ٌ', ""}, {'ِ', ""}, {'ٍ', ""}, {'ّ', ""}, {'ْ', ""}
        };

        // NEW: Generate unique username using DB check
        public static string GenerateUniqueUsername(string fullName, Func<string, bool> isUsernameTaken)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "user_" + Guid.NewGuid().ToString("N").Substring(0, 8);

            string baseName = GenerateBaseUsername(fullName);

            // If base name is available, use it
            if (!isUsernameTaken(baseName))
                return baseName;

            // Try malak_ahmed2, malak_ahmed3, etc.
            int counter = 2;
            string candidate;
            do
            {
                candidate = $"{baseName}{counter}";
                counter++;
            }
            while (isUsernameTaken(candidate));

            return candidate;
        }

        // Helper: Generate base username (without numbers)
        private static string GenerateBaseUsername(string fullName)
        {
            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string firstName = parts.FirstOrDefault() ?? "";
            string lastName = parts.Length > 1 ? parts.Last() : "";

            string latinFirst = Transliterate(firstName);
            string latinLast = Transliterate(lastName);

            if (string.IsNullOrEmpty(latinFirst) && string.IsNullOrEmpty(latinLast))
                return "user_" + Guid.NewGuid().ToString("N").Substring(0, 8);

            if (string.IsNullOrEmpty(latinLast))
                return latinFirst.ToLowerInvariant();

            return (latinFirst + "_" + latinLast).ToLowerInvariant();
        }

        private static string Transliterate(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            var result = new StringBuilder();
            foreach (char c in text)
            {
                if (ArabicToLatin.TryGetValue(c, out string replacement))
                {
                    result.Append(replacement);
                }
                else if (char.IsLetter(c))
                {
                    result.Append(char.ToLowerInvariant(c));
                }
            }

            string clean = result.ToString().Trim('_');
            clean = System.Text.RegularExpressions.Regex.Replace(clean, "_+", "_");
            return clean;
        }
    }
}