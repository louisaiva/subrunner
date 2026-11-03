using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// this Anim class store ONE animation and so it has a skin, a capacity & an orientation.
/// One instance of this class is stored in the big shared AnimBank.anims dict per skin/capacity/orientation.
/// THIS MEANS THAT IF 2 ZOMBIES PLAY THE SAME attack.D ANIM AT THE SAME TIME THEY WILL SHARE THE SAME
/// ANIM INSTANCE, so we sould NOT store in this class any capable-specific related things (speed for example)
/// </summary>
[Serializable] public class Anim
{

    // stocke UNE animation
    /* public string name; // nom de l'animation au format : skin.capacity.orientation
    public string skin { get { return name.Split('.')[0]; } }
    public string capacity { get { return name.Split('.')[1]; } }
    public string orientation { get { return name.Split('.')[2]; } }
    // todo pas opti, faire l'inverse : stocker chaque string skin,capacity,orientation
    // todo et faire une propertie name qui concatene les 3 */
    public string skin;
    public string capacity;
    public string orientation;
    public string name => skin + '.' + capacity + '.' + orientation;

    // stockage utile de l'animation
    // TEMPLATE SPECIFIC
    public List<string> sprites_paths = new();
    public List<Sprite> sprites = new();
    public List<float> sprites_durations = new();


    // parametres utiles à l'AnimPlayer
    // TEMPLATE SPECIFIC
    public readonly bool loop = true; // si c'est false, l'AnimPlayer revient sur l'animation par defaut
    public readonly bool flipX = false; // flip le sprite renderer si besoin
    public float speed = 1f; // vitesse de l'animation // ! todo : remove this since it is capable-instance based --> doit être dans une AnimModifData gérée par AnimPlayer

    public Anim() {}
    public Anim(string name, string[] sprites_paths, float[] sprites_durations)
    {
        // this.name = name;
        string[] splitted = name.Split('.');
        this.skin = splitted[0];
        this.capacity = splitted[1];
        this.orientation = splitted[2];

        this.sprites_paths = sprites_paths.ToList();
        this.sprites_durations = sprites_durations.ToList();
    }
    public Anim(string name, string[] sprites_paths, float[] sprites_durations, ref List<string> no_loop_capa)
    {
        // this.name = name;
        string[] splitted = name.Split('.');
        this.skin = splitted[0];
        this.capacity = splitted[1];
        this.orientation = splitted[2];
        this.sprites_paths = sprites_paths.ToList();
        this.sprites_durations = sprites_durations.ToList();
        this.loop = !no_loop_capa.Contains(capacity);
    }
    public Anim(Anim other, bool? override_flipX=null)
    {
        // this.name = other.name;
        this.skin = other.skin;
        this.capacity = other.capacity;
        this.orientation = other.orientation;
        this.loop = other.loop;
        this.speed = other.speed;
        this.flipX = override_flipX ?? other.flipX;

        this.sprites_paths = new List<string>(other.sprites_paths);
        this.sprites = new List<Sprite>(other.sprites);
        this.sprites_durations = new List<float>(other.sprites_durations);
    }

    // Sprite Loading
    public void LoadSprites(string spritesheets_path)
    {

        // on se prépare à stocker les sprites
        sprites = new List<Sprite>();

        // on prépare la ram pour les spritesheets
        Dictionary<string, Sprite[]> spritesheets = new();

        // on parcours tous les sprites qu'on cherche
        for (int i = 0; i < sprites_paths.Count; i++)
        {
            // on regarde si on a déjà chargé le spritesheet
            string[] splitted_path = sprites_paths[i].Split('.');
            string spritesheet_path = splitted_path[0];
            string sprite_name = splitted_path[1];

            if (!spritesheets.ContainsKey(spritesheet_path))
            {
                // on charge le spritesheet depuis le path
                Sprite[] spritesheet = Resources.LoadAll<Sprite>(spritesheets_path + spritesheet_path);
                if (spritesheet.Length == 0)
                {
                    Debug.LogError("(Anim - LoadSprites) Spritesheet NOT found : " + spritesheets_path + " (spritesheet_path from bank is " + spritesheet_path + ") for the sprite : " + sprite_name);
                    return;
                }

                // on ajoute le spritesheet à la liste
                spritesheets.Add(spritesheet_path, spritesheet);

                // on ajoute le bon sprite à la liste
                sprites.Add(spritesheet.FirstOrDefault(sprite => sprite.name == sprite_name));
                continue;
            }

            // si on est là on a déjà chargé le spritesheet
            // on cherche le bon sprite
            Sprite sprite = spritesheets[spritesheet_path].FirstOrDefault(sprite => sprite.name == sprite_name);

            // on ajoute le bon sprite à la liste
            sprites.Add(sprite);
        }
    }

    // getters
    public static bool IsNameCorrect(string name)
    {
        string[] splitted_name = name.Split('.');
        if (splitted_name.Length != 3) { return false; }
        return true;
    }

    public float GetDuration() { return GetBaseDuration() / speed; }
    public float GetBaseDuration()
    {
        // same as up but without the speed
        float duration = 0f;
        foreach (float d in sprites_durations)
        {
            duration += d;
        }
        return duration;
    }
    public float GetDurationUntilFrame(int frame_index) { return GetBaseDurationUntilFrame(frame_index) / speed; }
    public float GetBaseDurationUntilFrame(int frame_index)
    {
        // same as up but without the speed
        float duration = 0f;
        for (int i = 0; i < frame_index && i < sprites_durations.Count; i++)
        {
            duration += sprites_durations[i];
        }
        return duration;
    }
}
