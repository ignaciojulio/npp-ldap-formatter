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

        var compactFilter = RemoveWhitespace(ldapFilter);
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
                    output.Append(' ', Math.Max(0, depth - 1) * IndentSize);
                }

                continue;
            }

            output.Append(currentChar);
        }

        return output.ToString();
    }

    private static string RemoveWhitespace(string value)
    {
        var builder = new StringBuilder(value.Length);

        foreach (var c in value)
        {
            if (!char.IsWhiteSpace(c))
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
