using System;
using System.Text.RegularExpressions;

namespace Petroleum_Materials_Transport_Office_System.Utils
{
    public class UsernameGenerator
    {
        /// <summary>
        /// Generates a unique username based on the full name.
        /// </summary>
        /// <param name="fullName">The full name of the user (e.g. "Ahmed Ali")</param>
        /// <param name="isUsernameTaken">A function that returns TRUE if the username exists in the DB</param>
        /// <returns>A unique username (e.g. "ahmedali" or "ahmedali1")</returns>
        internal static string GenerateUniqueUsername(string fullName, Func<string, bool> isUsernameTaken)
        {
            // 1. Handle empty names
            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = "user";
            }

            // 2. Sanitize: Lowercase, remove spaces, keep only letters/numbers (optional)
            // Example: "Ahmed Ali" -> "ahmedali"
            string baseUsername = fullName.Trim().ToLower();
            baseUsername = Regex.Replace(baseUsername, @"\s+", "");

            // Ensure we didn't delete everything (e.g. if name was just spaces)
            if (string.IsNullOrEmpty(baseUsername))
            {
                baseUsername = "user";
            }

            // 3. Check Uniqueness
            string currentCandidate = baseUsername;
            int counter = 1;

            // Loop while the checking function returns true (meaning username is taken)
            while (isUsernameTaken(currentCandidate))
            {
                currentCandidate = $"{baseUsername}{counter}";
                counter++;
            }

            return currentCandidate;
        }
    }
}