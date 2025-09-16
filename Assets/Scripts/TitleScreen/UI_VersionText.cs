using UnityEngine;

public class UI_VersionText : MonoBehaviour
{

    [Header("APP MANAGER")]
    public string version = "0";
    public string prototype = "none";

    [Header("Version Text Settings")]
    [SerializeField] private string versionPrefix = "Version: ";
    [SerializeField] private bool showPrototype = false;

    [Header("Components")]
    [SerializeField] private TMPro.TextMeshProUGUI text;

    private void Awake()
    {
        // on récupère la version de l'app
        version = Application.version;
    }

    private void Start()
    {
        if (text == null) { return; }

        // on met à jour le texte
        string label = versionPrefix + version;

        if (showPrototype && !string.IsNullOrEmpty(prototype))
        {
            label += "\n(prototype " + prototype + ")";
        }

        text.text = label;
    }

    public string GetVersion()
    {
        return version;
    }
    public string GetFullVersion(bool includePrototype = true)
    {
        string full = versionPrefix + version;

        if (includePrototype && !string.IsNullOrEmpty(prototype))
        {
            full += "\n(prototype " + prototype + ")";
        }

        return full;
    }

}