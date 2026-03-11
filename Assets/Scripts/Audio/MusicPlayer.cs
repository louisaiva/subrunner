using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System;
using System.Collections.Generic;
using System.Collections;


public class MusicPlayer : MonoBehaviour
{
    [Header("Themes")]
    [SerializeField] private string theme_on_start = "";
    [SerializeField] private List<ThemeAudioData> themes_names = new List<ThemeAudioData>();
    private EventInstance current_theme;
    private bool _loop = true;

    [field:Header("Looping parameters")]
    [field:SerializeField] public bool loop
    {
        get { return _loop; }
        set
        {
            _loop = value;
            current_theme.setParameterByName("repeat_38sec", _loop ? 1.0f : 0f);
        }
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

        // we create an instance
        current_theme = AudioEngine.Instance.CreateInstance(theme_ref.Value);
        current_theme.start();

        // we get the repeat_38sec parameter of the theme
        current_theme.setParameterByName("repeat_38sec",_loop ? 1.0f : 0f);
    }

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