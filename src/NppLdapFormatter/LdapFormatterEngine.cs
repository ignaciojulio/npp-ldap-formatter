using System;
using System.Text;

namespace NppLdapFormatter;

public static class LdapFormatterEngine
{
    private const int IndentSize = 4;

    public static string Format(string? ldapFilter)
    {
        if (string.IsNullOrWhiteSpace(ldapFilter))
        {
            return string.Empty;
        }

        var compactFilter = NormalizeStructuralWhitespace(ldapFilter);
        var output = new StringBuilder(compactFilter.Length * 2);
        var depth = 0;

        for (var index = 0; index < compactFilter.Length; index++)
        {
            var currentChar = compactFilter[index];

            if (currentChar == '(')
            {
                if (output.Length > 0)
                {
                    output.AppendLine();
                }

                output.Append(' ', depth * IndentSize);
                output.Append(currentChar);
                depth++;
                continue;
            }

            if (currentChar == ')')
            {
                output.Append(currentChar);
                depth = Math.Max(0, depth - 1);

                if (index < compactFilter.Length - 1 && compactFilter[index + 1] == ')')
                {
                    output.AppendLine();
                    output.Append(' ', depth * IndentSize);
                }

                continue;
            }

            output.Append(currentChar);
        }

        return output.ToString();
    }

    private static string NormalizeStructuralWhitespace(string value)
    {
        var builder = new StringBuilder(value.Length);

        for (var i = 0; i < value.Length; i++)
        {
            var current = value[i];
            if (!char.IsWhiteSpace(current))
            {
                builder.Append(current);
                continue;
            }

            var previous = GetPreviousNonWhitespace(value, i - 1);
            var next = GetNextNonWhitespace(value, i + 1);
            var shouldRemove = previous == ')' || next == '(' || previous == '\0' || next == '\0';
            if (shouldRemove)
            {
                continue;
            }

            if (builder.Length == 0 || builder[^1] != ' ')
            {
                builder.Append(' ');
            }
        }

        return builder.ToString();
    }

    private static char GetPreviousNonWhitespace(string value, int index)
    {
        for (var i = index; i >= 0; i--)
        {
            if (!char.IsWhiteSpace(value[i]))
            {
                return value[i];
            }
        }

        return '\0';
    }

    private static char GetNextNonWhitespace(string value, int index)
    {
        for (var i = index; i < value.Length; i++)
        {
            if (!char.IsWhiteSpace(value[i]))
            {
                return value[i];
            }
        }

        return '\0';
    }
}
