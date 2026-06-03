using UnityEngine;

public static class StringExtensions
{
    public static string AddColor(this string text, Color col) => $"<color={ColorHexFromUnityColor(col)}>{text}</color>";
    public static string ColorHexFromUnityColor(this Color unityColor) => $"#{ColorUtility.ToHtmlStringRGBA(unityColor)}";
}

/*

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