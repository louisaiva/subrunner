using UnityEngine;
using System;
using UnityEngine.Audio;


public class AudioManager : MonoBehaviour
{

    public Sound[] sounds;

    // public static AudioManager instance;


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

    public void init(string path, float volume = 1f, bool loop = false, bool play_on_awake = false)
    {
        // on récupère tous les sons du dossier et on les ajoute à la liste des sons
        AudioClip[] clips = Resources.LoadAll<AudioClip>(path);
        foreach (AudioClip clip in clips)
        {
            Sound s = new Sound();
            s.name = clip.name;
            s.clip = clip;
            s.volume = volume;
            s.loop = loop;
            s.play_on_awake = play_on_awake;
            Array.Resize(ref sounds, sounds.Length + 1);
            sounds[sounds.Length - 1] = s;

            LoadSound(s);
        }

    }

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