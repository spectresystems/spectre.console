using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Spectre.Console.Internal;
using Spectre.Console.Rendering;

namespace Spectre.Console
{
    /// <summary>
    /// Contains extension methods for <see cref="string"/>.
    /// </summary>
    public static class StringExtensions
    {
        // Cache whether or not internally normalized line endings
        // already are normalized. No reason to do yet another replace if it is.
        private static readonly bool _alreadyNormalized
            = Environment.NewLine.Equals("\n", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Escapes text so that it won’t be interpreted as markup.
        /// </summary>
        /// <param name="text">The text to escape.</param>
        /// <returns>A string that is safe to use in markup.</returns>
        public static string EscapeMarkup(this string text)
        {
            if (text == null)
            {
                return string.Empty;
            }

            return text
                .ReplaceExact("[", "[[")
                .ReplaceExact("]", "]]");
        }

        internal static int CellLength(this string text, RenderContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Cell.GetCellLength(context, text);
        }

        internal static string Capitalize(this string text, CultureInfo? culture = null)
        {
            if (text == null)
            {
                return string.Empty;
            }

            culture ??= CultureInfo.InvariantCulture;

            if (text.Length > 0 && char.IsLower(text[0]))
            {
                text = string.Format(culture, "{0}{1}", char.ToUpper(text[0], culture), text.Substring(1));
            }

            return text;
        }

        internal static string? RemoveNewLines(this string? text)
        {
            return text?.ReplaceExact("\r\n", string.Empty)
                ?.ReplaceExact("\n", string.Empty);
        }

        internal static string NormalizeNewLines(this string? text, bool native = false)
        {
            text = text?.ReplaceExact("\r\n", "\n");
            text ??= string.Empty;

            if (native && !_alreadyNormalized)
            {
                text = text.ReplaceExact("\n", Environment.NewLine);
            }

            return text;
        }

        internal static string[] SplitLines(this string text)
        {
            var result = text?.NormalizeNewLines()?.Split(new[] { '\n' }, StringSplitOptions.None);
            return result ?? Array.Empty<string>();
        }

        internal static string[] SplitWords(this string word, StringSplitOptions options = StringSplitOptions.None)
        {
            var result = new List<string>();

            static string Read(StringBuffer reader, Func<char, bool> criteria)
            {
                var buffer = new StringBuilder();
                while (!reader.Eof)
                {
                    var current = reader.Peek();
                    if (!criteria(current))
                    {
                        break;
                    }

                    buffer.Append(reader.Read());
                }

                return buffer.ToString();
            }

            using (var reader = new StringBuffer(word))
            {
                while (!reader.Eof)
                {
                    var current = reader.Peek();
                    if (char.IsWhiteSpace(current))
                    {
                        var x = Read(reader, c => char.IsWhiteSpace(c));
                        if (options != StringSplitOptions.RemoveEmptyEntries)
                        {
                            result.Add(x);
                        }
                    }
                    else
                    {
                        result.Add(Read(reader, c => !char.IsWhiteSpace(c)));
                    }
                }
            }

            return result.ToArray();
        }

        internal static string Repeat(this string text, int count)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (count <= 0)
            {
                return string.Empty;
            }

            if (count == 1)
            {
                return text;
            }

            return string.Concat(Enumerable.Repeat(text, count));
        }

        internal static string ReplaceExact(this string text, string oldValue, string? newValue)
        {
#if NET5_0
            return text.Replace(oldValue, newValue, StringComparison.Ordinal);
#else
            return text.Replace(oldValue, newValue);
#endif
        }

        internal static bool ContainsExact(this string text, string value)
        {
#if NET5_0
            return text.Contains(value, StringComparison.Ordinal);
#else
            return text.Contains(value);
#endif
        }
    }
}
