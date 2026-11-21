using System;
using UnityEngine;

public static class StringExtensions
{
    public static string AddColor(this string text, Color col) => $"<color={ColorHexFromUnityColor(col)}>{text}</color>";
    public static string ColorHexFromUnityColor(this Color unityColor) => $"#{ColorUtility.ToHtmlStringRGBA(unityColor)}";

    // SPECIFIC COLORS
    public static string Red(this string text) => AddColor(text, Color.red);
    public static string Green(this string text) => AddColor(text, Color.green);
    public static string Grey(this string text) => AddColor(text, Color.grey);
    public static string Cyan(this string text) => AddColor(text, Color.cyan);
    public static string Magenta(this string text) => AddColor(text, Color.magenta);
    public static string RedIfNull(this string text)
    {
        if (text == null)
        {
            text = "NULL";
            return AddColor(text, Color.red);
        }

        if (text == "")
        {
            text = "EMPTY";
            return AddColor(text, Color.yellow);
        }
        return text;
    }

    // TIME METHODS
    public static string ToHMS(this float sec, string format="F2")
    {
        int min = (int)sec / 60;
        if (min <= 0) { return sec.ToString(format) + "s"; } // only seconds

        // we have some minutes !
        sec -= min * 60;
        int hour = min / 60;
        if (hour <= 0) { return $"{min}min {sec.ToString(format)}s"; }

        // we have some hours !
        min -= hour * 60;
        return $"{hour}h {min}min {sec.ToString(format)}s";
    }
    public static string ToColoredHMS(this float sec, Color highlight, Color base_color, string format = "F2")
    {
        return color_hms(sec.ToHMS(format), highlight, base_color);
    }
    private static string color_hms(string hms, Color highlight, Color base_color)
    {
        // ex : 17h 36min 10,21s
        // we highlight 17 ; 36 & 10,21
        // we set base_color to the rest (h, min & s)
        string colored_hms = "";
        foreach (char c in hms)
        {
            if (c == ' ')
            {
                colored_hms += ' ';
                continue;
            }

            if (Char.IsNumber(c) || c == ',')
            {
                colored_hms += c.ToString().AddColor(highlight);
                continue;
            }

            // else we only have the rest
            colored_hms += c.ToString().AddColor(base_color);
        }
        return colored_hms;
    }



    // NATURAL ORDER SORTING
    private static NaturalSortComparer comparer = new NaturalSortComparer(System.StringComparison.Ordinal);
    public static int NumericOrderingCompare(string a,string b)
    {
        return comparer.Compare(a,b);
    }


    // PREFIX
    public static string GetPrefix(this string id)
    {
        if (id == null) { return null; }
        string[] parts = id.Split('-');
        return parts[0];
    }
}

/*

"<color=#FF0000>"

//Usage
TextMeshProUGUI SomeTMProText;

SomeTMProText.SetText($"" +
            $"{"H".AddColor(Color.red)}" +
            $"{"E".AddColor(Color.blue)}" +
            $"{"L".AddColor(Color.green)}" +
            $"{"L".AddColor(Color.white)}" +
            $"{"O".AddColor(Color.yellow)}");

        --> gives "HELLO" with colored letters

*/