namespace Schema.NET.Tool;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

public static partial class SourceUtility
{
    private const char Space = ' ';

    public static string Render(string value, int indent = 0)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(value);
#else
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }
#endif

        var indentString = new string(Space, indent);
        return indentString + EnvironmentAgnosticNewLineReplace().Replace(value, "$1" + indentString);
    }

    public static string RenderItems<T>(bool canRender, IReadOnlyCollection<T> items, Func<int, int, T, string> action,
        int indent = 0, SourceDelimeter sourceDelimeter = SourceDelimeter.None)
    {
        if (canRender)
        {
            return RenderItems(items, action, indent, sourceDelimeter);
        }

        return string.Empty;
    }

    public static string RenderItems<T>(IReadOnlyCollection<T> items, Func<int, int, T, string> action, int indent = 0,
        SourceDelimeter sourceDelimeter = SourceDelimeter.None)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(action);
#else
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }
#endif

        var stringBuilder = new StringBuilder();
        for (var i = 0; i < items.Count; ++i)
        {
            var item = items.ElementAt(i);
            var line = EnvironmentAgnosticNewLineReplace().Replace(action(i, indent, item), "$1" + new string(Space, indent));
            var isLast = i == items.Count - 1;

            stringBuilder.Append(Space, indent);

#pragma warning disable IDE0072 // Add missing cases
            stringBuilder = sourceDelimeter switch
            {
                SourceDelimeter.None => stringBuilder.Append(line),
                SourceDelimeter.NewLine => isLast switch
                {
                    false => stringBuilder.AppendLine(line),
                    true => stringBuilder.Append(line),
                },
                SourceDelimeter.NewLineSpace => isLast switch
                {
                    false => stringBuilder.AppendLine(line).AppendLine(),
                    true => stringBuilder.Append(line),
                },
                _ => throw new ArgumentException("Source delimeter not recognised.", nameof(sourceDelimeter)),
            };
#pragma warning restore IDE0072 // Add missing cases
        }

        return stringBuilder.ToString();
    }

    public static string RenderDoc(int indent, string? text)
    {
        if (text == null)
        {
            return string.Empty;
        }

        var escapedValue = XmlEscape(text.Trim());
        return NewLineReplace().Replace(escapedValue, $"\n{new string(Space, indent)}/// ");
    }

    private static string XmlEscape(string value)
    {
        var document = new XmlDocument();
        var element = document.CreateElement("root");
        element.InnerText = value;
        return element.InnerXml;
    }

#if NET8_0_OR_GREATER
    [GeneratedRegex("[\n ]{0,}\n[\n ]{0,}", RegexOptions.Compiled)]
    private static partial Regex NewLineReplace();

    [GeneratedRegex("(\r?\n)", RegexOptions.Compiled)]
    private static partial Regex EnvironmentAgnosticNewLineReplace();
#else
    private static readonly Regex NewLineRegex = new Regex("[\n ]{0,}\n[\n ]{0,}", RegexOptions.Compiled);
    private static Regex NewLineReplace() => NewLineRegex;

    private static readonly Regex EnvironmentAgnosticNewLineRegex = new Regex("(\r?\n)", RegexOptions.Compiled);
    private static Regex EnvironmentAgnosticNewLineReplace() => EnvironmentAgnosticNewLineRegex;
#endif
}
