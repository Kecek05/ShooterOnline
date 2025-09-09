using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AssetInventory
{
    public static class StringUtils
    {
        private const long SEC = TimeSpan.TicksPerSecond;
        private const long MIN = TimeSpan.TicksPerMinute;
        private const long HOUR = TimeSpan.TicksPerHour;
        private const long DAY = TimeSpan.TicksPerDay;
        private static readonly Regex CAMEL_CASE_R1 = new Regex(@"(?<=[a-z])(?=[A-Z])|(?<=[0-9])(?=[A-Z])|(?<=[A-Z])(?=[0-9])|(?<=[0-9])(?=[a-z])", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex CAMEL_CASE_R2 = new Regex(@"(?<= [A-Z])(?=[A-Z][a-z])", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex CAMEL_CASE_R3 = new Regex(@"(?<=[^\s])(?=[(])|(?<=[)])(?=[^\s])", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string ExtractTokens(string input, string tokenName, List<string> tokenValues)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(tokenName)) return input;

            // Pattern to match tokens with different value formats:
            // 1. Quoted strings (single or double quotes) - handles escaped tokens with spaces
            // 2. Non-whitespace sequences - handles simple tokens without spaces
            // 3. Empty tokens (just the token name followed by colon) - should be removed but not added to values
            string pattern = $@"\b{Regex.Escape(tokenName)}:((?:'[^']*'|""[^""]*""|\S+)?)";

            // Use a MatchEvaluator to both capture the token and remove it in one go.
            string result = Regex.Replace(input, pattern, match =>
            {
                string value = match.Groups[1].Value;
                
                // Skip empty tokens (don't add them to the values list)
                if (string.IsNullOrEmpty(value))
                {
                    return string.Empty;
                }
                
                // Remove quotes if present (for escaped tokens)
                if ((value.StartsWith("'") && value.EndsWith("'")) || 
                    (value.StartsWith("\"") && value.EndsWith("\"")))
                {
                    value = value.Substring(1, value.Length - 2);
                }
                
                tokenValues.Add(value);

                // Return an empty string to remove this token from the original text.
                return string.Empty;
            });

            // remove any excess whitespace created by token removal
            result = Regex.Replace(result, @"\s+", " ").Trim();

            return result;
        }

        public static string GetRelativeTimeDifference(DateTime date)
        {
            return GetRelativeTimeDifference(date, DateTime.Now);
        }

        public static string GetRelativeTimeDifference(DateTime date1, DateTime date2)
        {
            long ticks = date2.Ticks - date1.Ticks;
            if (ticks < 0) ticks = -ticks;

            if (ticks >= DAY)
            {
                int v = (int)(ticks / DAY);
                return v == 1 ? "1 day ago" : v.ToString(CultureInfo.InvariantCulture) + " days ago";
            }
            if (ticks >= HOUR)
            {
                int v = (int)(ticks / HOUR);
                return v == 1 ? "1 hour ago" : v.ToString(CultureInfo.InvariantCulture) + " hours ago";
            }
            if (ticks >= MIN)
            {
                int v = (int)(ticks / MIN);
                return v == 1 ? "1 minute ago" : v.ToString(CultureInfo.InvariantCulture) + " minutes ago";
            }

            int s = (int)(ticks / SEC);
            return s == 1 ? "1 second ago" : s.ToString(CultureInfo.InvariantCulture) + " seconds ago";
        }

        public static string EscapeSQL(string input)
        {
            // Pattern to find 'like' clauses
            string pattern = @"(like\s+'[^']*)";
            string escapePattern = @"(like\s+'[^']*')";

            // Replace underscores with escaped underscores inside 'like' clauses
            input = Regex.Replace(input, pattern, m =>
            {
                string likeClause = m.Groups[1].Value;
                likeClause = likeClause.Replace("_", "\\_");
                return likeClause;
            }, RegexOptions.IgnoreCase);

            // Add ESCAPE '\' behind each 'like' clause
            input = Regex.Replace(input, escapePattern, "$1 ESCAPE '\\'", RegexOptions.IgnoreCase);

            return input;
        }

        // drop-in for Unity 2019 where splitting is only possible by char and Contains does not support StringComparison
#if !UNITY_2021_2_OR_NEWER
        public static string[] Split(this string source, string separator, StringSplitOptions options = StringSplitOptions.None)
            => source.Split(new[] {separator}, options);

        public static bool Contains(this string source, string toCheck, StringComparison comparison)
        {
            if (source == null || toCheck == null) return false;
            return source.IndexOf(toCheck, comparison) >= 0;
        }
#endif

        public static string Truncate(this string value, int maxLength)
        {
            if (value == null) return null;

            return value.Length <= maxLength
                ? value
                : value.Substring(0, maxLength);
        }

        public static string[] Split(string input, char[] separators)
        {
            if (string.IsNullOrEmpty(input)) return Array.Empty<string>();

            string[] parts = input.Split(separators, StringSplitOptions.None);

            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Trim();
            }

            return parts;
        }

        public static string CamelCaseToWords(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            string result = CAMEL_CASE_R1.Replace(input, " ");
            result = CAMEL_CASE_R2.Replace(result, " ");
            result = CAMEL_CASE_R3.Replace(result, " ");

            string[] words = result.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = CapitalizeFirstLetter(words[i]);
            }

            return string.Join(" ", words);
        }

        private static string CapitalizeFirstLetter(string word)
        {
            if (string.IsNullOrEmpty(word)) return word;

            // Preserve the case of the rest of the word
            return char.ToUpper(word[0]) + word.Substring(1);
        }

        public static string GetShortHash(string input, int length = 6)
        {
            if (length < 1 || length > 10)
            {
                throw new ArgumentOutOfRangeException(nameof (length), "Length must be between 1 and 10.");
            }

            // Compute a simple hash from the input string.
            int hash = 0;
            foreach (char c in input)
            {
                hash = (hash * 31 + c); // Use a prime number multiplier
            }

            // Calculate the modulus based on the desired length
            int mod = (int)Math.Pow(10, length);

            // Reduce the hash to a number with the desired length
            int shortHash = Math.Abs(hash) % mod;

            // Return the hash as a string, padded with leading zeros if necessary
            return shortHash.ToString($"D{length}");
        }

        public static bool IsUrl(string url)
        {
            return Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        public static bool IsUnicode(this string input)
        {
            return input.ToCharArray().Any(c => c > 255);
        }

        public static string StripTags(string input, bool removeContentBetweenTags = false)
        {
            if (removeContentBetweenTags)
            {
                return Regex.Replace(input, "<[^>]+?>.*?</[^>]+?>", string.Empty, RegexOptions.Singleline);
            }
            return Regex.Replace(input, "<.*?>", string.Empty);
        }

        public static string StripUnicode(string input)
        {
            return Regex.Replace(input, "&#.*?;", string.Empty);
        }

        public static string RemoveTrailing(this string source, string text)
        {
            if (source == null)
            {
                Debug.LogError("This should not happen, source is null");
                return null;
            }

            while (source.EndsWith(text)) source = source.Substring(0, source.Length - text.Length);
            return source;
        }

        public static string ToLowercaseFirstLetter(this string input)
        {
            if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
            {
                return input;
            }

            return char.ToLower(input[0]) + input.Substring(1);
        }

        public static string ToLabel(string input)
        {
            string result = input;

            // Normalize line breaks to \n
            result = Regex.Replace(result, @"\r\n?|\n", "\n");

            // Translate some HTML tags
            result = result.Replace("<br>", "\n");
            result = result.Replace("</br>", "\n");
            result = result.Replace("<p>", "\n\n");
            result = result.Replace("<p >", "\n\n");
            result = result.Replace("<li>", "\n* ");
            result = result.Replace("<li >", "\n* ");
            result = result.Replace("&nbsp;", " ");
            result = result.Replace("&amp;", "&");

            // Remove remaining tags and also unicode tags
            result = StripUnicode(StripTags(result));

            // Remove whitespace from empty lines
            result = Regex.Replace(result, @"[ \t]+\n", "\n");

            // Ensure at max two consecutive line breaks
            result = Regex.Replace(result, @"\n{3,}", "\n\n");

            return result.Trim();
        }

        public static string GetEnvVar(string key)
        {
            string value = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
            if (string.IsNullOrWhiteSpace(value)) value = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
            if (string.IsNullOrWhiteSpace(value)) value = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Machine);

            return value;
        }
    }
}