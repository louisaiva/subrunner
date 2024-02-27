using UnityEngine;
using System;
using UnityEngine.Audio;


public class AudioManager : MonoBehaviour
{
    // [Header("Path")]
    [SerializeField] private string path = "audio/";

    [Header("Sounds")]
    public Sound[] sounds;

    // INITIALISATION

    void Awake()
    {
        // Create an instance of the AudioManager if it doesn't exist
        /* if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject); */

        // Create an AudioSource for each sound
        foreach (Sound s in sounds)
        {
            LoadSound(s);
        }
    }

    void LoadSound(Sound s)
    {
        s.source = gameObject.AddComponent<AudioSource>();
        s.source.clip = s.clip;
        s.source.volume = s.volume;
        s.source.loop = s.loop;
    }

    void Start()
    {
        foreach (Sound s in sounds)
        {
            if (s.play_on_awake)
            {
                s.source.Play();
            }
        }        
    }


    // MAIN FUNCTIONS

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
    }



    // LOADING SOUNDS

    public void AddSoundsFromPath(string path, float volume = 1f, bool loop = false, bool play_on_awake = false)
    {
        // on récupère tous les sons du dossier et on les ajoute à la liste des sons
        AudioClip[] clips = Resources.LoadAll<AudioClip>(path);
        foreach (AudioClip clip in clips)
        {
            Sound s = new Sound();
            s.name = clip.name;

            if (Array.Exists(sounds, sound => sound.name == s.name))
            {
                // Debug.LogWarning("Sound: " + s.name + " already exists!");
                continue;
            }

            s.clip = clip;
            s.volume = volume;
            s.loop = loop;
            s.play_on_awake = play_on_awake;
            Array.Resize(ref sounds, sounds.Length + 1);
            sounds[sounds.Length - 1] = s;

            // LoadSound(s);
        }

    }
    public void AddSoundsFromDefaultPath()
    {
        if (path == "" || path == null)
        {
            Debug.LogWarning("(AudioManager) - Trying to load sounds from no path!");
            return;
        }

        AddSoundsFromPath(path);
    }

}


[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    public float volume;

    public bool loop;

    public bool play_on_awake = false;

    [HideInInspector]
    public AudioSource source;
}