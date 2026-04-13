using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>
/// マップ文字列の圧縮/展開ユーティリティ
/// 形式: M2|{width}|{height}|{charHex}:{countHex};...
/// </summary>
public static class MapTextCodec
{
    private const string Prefix = "M2|";

    public static bool IsCompressedFormat(string text)
    {
        return !string.IsNullOrEmpty(text) && text.StartsWith(Prefix, StringComparison.Ordinal);
    }

    public static string Encode(string mapText)
    {
        if (string.IsNullOrEmpty(mapText)) return string.Empty;

        string normalized = mapText.Replace("\r\n", "\n").Replace('\r', '\n');
        string[] lines = normalized.Split('\n');
        int height = lines.Length;
        int width = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Length > width)
            {
                width = lines[i].Length;
            }
        }

        if (width <= 0 || height <= 0)
        {
            return string.Empty;
        }

        StringBuilder flatBuilder = new StringBuilder(width * height);
        for (int y = 0; y < height; y++)
        {
            string line = lines[y];
            for (int x = 0; x < width; x++)
            {
                flatBuilder.Append(x < line.Length ? line[x] : ' ');
            }
        }

        string flat = flatBuilder.ToString();
        if (flat.Length == 0)
        {
            return string.Empty;
        }

        StringBuilder encoded = new StringBuilder(flat.Length + 32);
        encoded.Append(Prefix)
            .Append(width.ToString(CultureInfo.InvariantCulture))
            .Append('|')
            .Append(height.ToString(CultureInfo.InvariantCulture))
            .Append('|');

        char current = flat[0];
        int run = 1;
        for (int i = 1; i < flat.Length; i++)
        {
            char c = flat[i];
            if (c == current)
            {
                run++;
                continue;
            }

            AppendRun(encoded, current, run);
            current = c;
            run = 1;
        }
        AppendRun(encoded, current, run);

        return encoded.ToString();
    }

    public static string EncodeIfSmaller(string mapText)
    {
        if (string.IsNullOrEmpty(mapText)) return string.Empty;

        string encoded = Encode(mapText);
        if (string.IsNullOrEmpty(encoded))
        {
            return mapText;
        }

        return encoded.Length < mapText.Length ? encoded : mapText;
    }

    public static string DecodeIfNeeded(string mapText)
    {
        if (string.IsNullOrEmpty(mapText)) return mapText;

        if (!TryDecode(mapText, out string decoded))
        {
            return mapText;
        }

        return decoded;
    }

    public static bool TryDecode(string mapText, out string decoded)
    {
        decoded = mapText;

        if (!IsCompressedFormat(mapText))
        {
            return false;
        }

        if (!TrySplitHeader(mapText, out int width, out int height, out string payload))
        {
            return false;
        }

        if (width <= 0 || height <= 0)
        {
            return false;
        }

        List<char> flat = new List<char>(width * height);
        string[] tokens = payload.Split(';');
        for (int i = 0; i < tokens.Length; i++)
        {
            string token = tokens[i];
            if (string.IsNullOrEmpty(token))
            {
                continue;
            }

            int sep = token.IndexOf(':');
            if (sep <= 0 || sep >= token.Length - 1)
            {
                return false;
            }

            string charHex = token.Substring(0, sep);
            string countHex = token.Substring(sep + 1);

            if (!int.TryParse(charHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int charCode))
            {
                return false;
            }
            if (!int.TryParse(countHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int count))
            {
                return false;
            }
            if (count <= 0)
            {
                return false;
            }

            char c = (char)charCode;
            for (int n = 0; n < count; n++)
            {
                flat.Add(c);
            }
        }

        int expected = width * height;
        if (flat.Count != expected)
        {
            return false;
        }

        StringBuilder sb = new StringBuilder(expected + height);
        for (int y = 0; y < height; y++)
        {
            int rowStart = y * width;
            int rowEnd = rowStart + width - 1;
            while (rowEnd >= rowStart && flat[rowEnd] == ' ')
            {
                rowEnd--;
            }

            for (int x = rowStart; x <= rowEnd; x++)
            {
                sb.Append(flat[x]);
            }

            if (y < height - 1)
            {
                sb.Append('\n');
            }
        }

        decoded = sb.ToString();
        return true;
    }

    private static void AppendRun(StringBuilder builder, char c, int count)
    {
        builder
            .Append(((int)c).ToString("X", CultureInfo.InvariantCulture))
            .Append(':')
            .Append(count.ToString("X", CultureInfo.InvariantCulture))
            .Append(';');
    }

    private static bool TrySplitHeader(string mapText, out int width, out int height, out string payload)
    {
        width = 0;
        height = 0;
        payload = string.Empty;

        int first = mapText.IndexOf('|');
        if (first < 0) return false;
        int second = mapText.IndexOf('|', first + 1);
        if (second < 0) return false;
        int third = mapText.IndexOf('|', second + 1);
        if (third < 0) return false;

        string header = mapText.Substring(0, first + 1);
        if (!string.Equals(header, Prefix, StringComparison.Ordinal))
        {
            return false;
        }

        string widthText = mapText.Substring(first + 1, second - first - 1);
        string heightText = mapText.Substring(second + 1, third - second - 1);
        payload = mapText.Substring(third + 1);

        return int.TryParse(widthText, NumberStyles.Integer, CultureInfo.InvariantCulture, out width)
               && int.TryParse(heightText, NumberStyles.Integer, CultureInfo.InvariantCulture, out height);
    }
}