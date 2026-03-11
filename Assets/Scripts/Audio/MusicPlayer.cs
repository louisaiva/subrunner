using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System;
using System.Collections.Generic;


public class MusicPlayer : MonoBehaviour
{
    [Header("Themes")]
    [SerializeField] private string theme_on_start = "";
    [SerializeField] private List<ThemeAudioData> themes_names = new List<ThemeAudioData>();
    private EventInstance? current_theme = null;
    private bool _loop = true;

    [field:Header("Looping parameters")]
    [field:SerializeField] public bool loop
    {
        get { return _loop; }
        set
        {
            _loop = value;
            try
            {
                current_theme.Value.setParameterByName("repeat_38sec", _loop ? 1.0f : 0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("The theme " + name + " does not have the parameter repeat_38sec, looping will not work. Exception: " + e);
            }
        }
    }

    // AWAKE
    public static MusicPlayer Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    // START
    private void Start()
    {
        // we play it
        PlayTheme(theme_on_start);
    }

    // PLAY THEME
    public void PlayTheme(string name)
    {
        // we try to find it
        EventReference? theme_ref = get_theme_ref(name);
        if (theme_ref == null) { return; }

        // we check if we have a current theme playing, if yes we stop it
        if (current_theme != null)
        {
            current_theme.Value.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            current_theme.Value.release();
        }

        // we create an instance
        current_theme = AudioEngine.Instance.CreateInstance(theme_ref.Value);
        current_theme.Value.start();

        // we get the repeat_38sec parameter of the theme
        try
        {
            current_theme.Value.setParameterByName("repeat_38sec", _loop ? 1.0f : 0f);
        }
        catch (Exception e)
        {
            Debug.LogWarning("The theme " + name + " does not have the parameter repeat_38sec, looping will not work. Exception: " + e);
        }
    }
    public void PlayLobbyTheme() { PlayTheme("melancolic lobby"); }

    // utils
    private EventReference? get_theme_ref(string name)
    {
        for (int i=0; i<themes_names.Count; i++)
        {
            if (themes_names[i].name == name) { return themes_names[i].reference; }
        }
        return null;
    }

}

[Serializable] public class ThemeAudioData
{
    public string name;
    public EventReference reference;
}