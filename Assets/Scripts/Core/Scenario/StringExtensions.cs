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