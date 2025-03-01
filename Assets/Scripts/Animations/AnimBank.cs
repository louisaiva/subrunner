using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Linq;
using Unity.Properties;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class AnimBank : MonoBehaviour
{
    // stocke toutes les animations et sprites utilisées dans le jeu
    // permet de les charger et de les stocker pour les utiliser plus tard
    // "remplace" entre gros guillemets les AnimationController.
    // un peu chiant parce qu'on doit extraire les données des AnimationClips afin
    // de les stocker dans des fichiers .json pour pouvoir les récupérer en BUILD
    // mais ça marche bien et c'est assez simple à utiliser


    [Header("Animations")]
    // store all the animations with the keys : skin, capacity, orientation
    public Dictionary<string,Dictionary<string,List<Anim>>> anims = new();
    // if this is true, the bank will load animations from .anim files and store them as .json files
    // if false, the bank will load animations from .json files (/!\ you need to have the .json files in the Resources/anims/ folder)
    public bool extract_json_on_load = false;
    public string anims_path = "animations/";
    public string jsons_path = "anims/";


    [Header("Animations parameters")]
    // store the capacities that have parameters override
    private List<string> capacities_with_parameters_override = new List<string>() { "attack","hurted","dodge" };
    // store the loops of the capacities
    private List<bool> capacities_loops = new List<bool>() { false,false,false };

    [Header("Sprites")]
    public string spritesheets_path = "spritesheets/";


    [Header("Debug")]
    public bool debug = false;
    public bool debug_LAFAC = false;

    // INITIALIZATION
    private void Awake()
    {
        LoadAnims();

        Debug.Log(getAnimsList());
    }
    private void LoadAnims()
    {

        if (extract_json_on_load)
        {
            #if UNITY_EDITOR

                // we remove all the .json files in the Resources/anims/ folder
                // string[] jsons_paths = Directory.GetFiles("Assets/Resources/" + jsons_path, "*.json", SearchOption.AllDirectories);
                FileUtil.DeleteFileOrDirectory("Assets/Resources/" + jsons_path);
                Directory.CreateDirectory("Assets/Resources/" + jsons_path);

                if (debug) {Debug.Log("(AnimBank - LoadAnims) Extracting animations from AnimationClips and saving them as .json files.");}

                // if in the editor, we load AnimationClips and store them as .json files
                string[] anims_paths = Directory.GetFiles("Assets/Resources/" + anims_path, "*.anim", SearchOption.AllDirectories);
                if (debug) {Debug.Log("(AnimBank - LoadAnims) Found " + anims_paths.Length + " animations :\n\t" + string.Join("\n\t", anims_paths));}
                foreach (string path in anims_paths)
                {
                    // we remove Assets/Resources/anims/ from the path
                    string new_path = path.Replace("Assets/Resources/" + anims_path, "");
                    new_path = new_path.Replace(".anim", "");
                    loadAnimFromAnimationClip(new_path);
                }
        
            return;
            #endif
        }

        if (debug) {Debug.Log("(AnimBank - LoadAnims) Loading animations from .json files.");}

        // if in the build or not extracting from .anim, we load .json files
        TextAsset[] jsons = Resources.LoadAll<TextAsset>(jsons_path);
        string[] json_paths = new string[jsons.Length];
        for (int i = 0; i < jsons.Length; i++)
        {
            json_paths[i] = jsons[i].name;
        }

        if (debug) {Debug.Log("(AnimBank - LoadAnims) Found " + json_paths.Length + " jsons :\n\t" + string.Join("\n\t", json_paths));}
        foreach (string path in json_paths)
        {
            // we remove Assets/Resources/anims/ from the path
            string new_path = path.Replace("Assets/Resources/" + jsons_path, "").Replace(".json", "");
            loadAnimFromJson(new_path);
        }
    }


    // ANIMATIONS MANAGEMENT
    private void loadAnimFromJson(string path)
    {
        // LOAD AN ANIMATION FROM A ALREADY EXISTING JSON FILE

        // on charge le json depuis le path
        TextAsset json = Resources.Load<TextAsset>("anims/" + path);
        if (!json)
        {
            Debug.LogError("(AnimBank - LAFJ) JSON NOT found : " + path);
            return;
        }

        // on désérialise le json
        Anim anim = JsonConvert.DeserializeObject<Anim>(json.text);
        if (anim == null)
        {
            Debug.LogError("(AnimBank - LAFJ) JSON deserialization failed : " + path);
            return;
        }

        // on l'ajoute à la banque anims
        AddAnim(anim);
    }
    #if UNITY_EDITOR
    private void loadAnimFromAnimationClip(string path)
    {
        // LOAD AN ANIMATION FROM AN ANIMATION CLIP
        // AND THEN EXTRACT THE DATA AND SAVE IT AS A JSON FILE
        // only works in the editor

        if (debug_LAFAC) {Debug.Log("(AnimBank - LAFAC) Loading animation from AnimationClip : " + path);}

        // on charge l'animation depuis le path
        AnimationClip clip = Resources.Load<AnimationClip>("animations/" + path /* + ".anim" */);
        if (!clip)
        {
            Debug.LogError("(AnimBank - LAFAC) AnimationClip NOT found in : animations/" + path);
            return;
        }

        // on extrait les données de l'animation
        Anim anim = extractDataFromAnimationClip(clip);
        if (anim == null) { return; }
        if (!anim.IsNameCorrect(anim.name))
        {
            if (debug_LAFAC) {Debug.LogWarning("(AnimBank - LAFAC) Animation name format is incorrect : " + anim.name);}
            return;
        }

        // otherwise we simply save the animation
        saveAnimToJson(anim);
        AddAnim(anim);
    }
    private Anim extractDataFromAnimationClip(AnimationClip clip)
    {
        // on extrait les keyframes de chaque binding d'une animation
        // les bindings sont les propriétés animées
        // ex : SpriteRenderer.sprite
        // ex2: Transform.position
        // .. etc dcp ce qui nous intéresse ici c le sprite on s'en fout du reste
        var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);

        // on récupère le binding du spriterenderer.sprite
        foreach (var binding in bindings)
        {
            if (binding.propertyName == "m_Sprite")
            {
                return extractSpriteCurve(clip, binding);
            }
        }

        // si on arrive ici c'est qu'on a pas trouvé de sprite curve
        if (debug_LAFAC) {Debug.LogWarning("(AnimBank - ExtractDataFromAnimationClip) No sprite curve found in the animation clip.");}
        return null;
    }
    private Anim extractSpriteCurve(AnimationClip clip, EditorCurveBinding binding)
    {

        // Extract the curve for the SpriteRenderer.sprite property
        var spriteCurve = AnimationUtility.GetObjectReferenceCurve(clip, binding);
        int frameCount = spriteCurve.Length;

        string[] sprites_paths = new string[frameCount];
        float[] sprites_durations = new float[frameCount];

        for (int i = 0; i < frameCount; i++)
        {
            // save the frame time
            sprites_durations[i] = (i == 0) ? spriteCurve[i].time : spriteCurve[i].time - spriteCurve[i - 1].time;
            
            // save the sprite path
            Sprite sprite = spriteCurve[i].value as Sprite;
            if (sprite == null)
            {
                if (debug_LAFAC) {Debug.LogWarning("(AnimBank - ExtractSpriteCurve) Sprite not found in the animation clip. Skipping Anim : " + clip.name);}
                return null;
            }
            string spritePath = AssetDatabase.GetAssetPath(sprite).Replace(".png","").Replace("Assets/Resources/" + spritesheets_path, "");
            spritePath += "." + sprite.name;
            sprites_paths[i] = spritePath;
        }

        // we create a new Anim object
        Anim anim = new Anim(clip.name, sprites_paths, sprites_durations);
        return anim;
    }
    private void saveAnimToJson(Anim anim)
    {
        string json = JsonConvert.SerializeObject(anim);
        string path = "Assets/Resources/" + jsons_path + anim.name + ".json";
        System.IO.File.WriteAllText(path, json);
    }
    #endif



    // BANK MANAGEMENT
    private void AddAnim(Anim anim)
    {
        // check if the animation already exists
        if (HasAnim(anim.name))
        {
            if (debug) {Debug.LogWarning("(AnimBank - AddAnim) Animation already exists in the bank : " + anim.name);}
            return;
        }
        
        // prepare the skin and capacity
        string[] splitted_name = anim.name.Split('.');
        string skin = splitted_name[0];
        string capacity = splitted_name[1];

        // check if the capacity has parameters override
        int index = capacities_with_parameters_override.IndexOf(capacity);
        if (index != -1)
        {
            anim.loop = capacities_loops[index];
            // anim.priority = capacities_priorities[index];
        }

        // add the skin & capacity to the bank if they don't exist
        if (!HasSkin(skin))
        {
            anims.Add(skin, new Dictionary<string, List<Anim>>());
        }
        if (!HasCapacity(skin + "." + capacity))
        {
            anims[skin].Add(capacity, new List<Anim>());
        }


        // we check if the orientation has "LR" in it.
        // if it does, we add the animation to the bank with "LR" replaced by "L" and "R"
        if (splitted_name[2].Contains("LR"))
        {
            // we copy the animation
            Anim anim_R = new Anim(anim);
            anim_R.name = skin + "." + capacity + "." + splitted_name[2].Replace("LR", "R");
            anim_R.flipX = true;

            // load the sprites
            anim_R.LoadSprites(spritesheets_path);

            // we add the animation to the bank
            anims[skin][capacity].Add(anim_R);

            // we change the anim name
            anim.name = skin + "." + capacity + "." + splitted_name[2].Replace("LR", "L");
        }

        // load the sprites
        anim.LoadSprites(spritesheets_path);

        // add the animation to the bank
        anims[skin][capacity].Add(anim);
    }
    public Anim GetAnim(string name)
    {
        string[] splitted_name = name.Split('.');
        string skin = splitted_name[0];
        string capacity = splitted_name[1];

        if (HasAnim(name))
        {
            return anims[skin][capacity].Find(anim => anim.name == name);
        }
        
        if (HasCapacity(name))
        {
            return anims[skin][capacity][0];
        }

        if (HasSkin(skin))
        {
            if (anims[skin].ContainsKey("idle"))
            {
                return anims[skin]["idle"][0];
            }
            return anims[skin].Values.ToList()[0][0];
        }
        
        if (debug) {Debug.LogWarning("(AnimBank - GetAnim) Animation not found in the bank : " + name);}
        return null;
    }
    public bool HasAnim(string name)
    {
        string skin = name.Split('.')[0];
        string capacity = name.Split('.')[1];

        if (!HasSkin(skin)) { return false; }
        if (!HasCapacity(skin + "." + capacity)) { return false; }
        return anims[skin][capacity].Exists(anim => anim.name == name);
    }
    public bool HasCapacity(string skin_and_capacity)
    {
        string skin = skin_and_capacity.Split('.')[0];
        string capacity = skin_and_capacity.Split('.')[1];

        if (!HasSkin(skin)) { return false; }
        return anims[skin].ContainsKey(capacity);
    }
    public bool HasSkin(string skin)
    {
        return anims.ContainsKey(skin);
    }

    // DEBUG
    private string getAnimsList()
    {
        string title = "ANIMS in the bank - Total ";
        string list = "";
        int count = 0;
        foreach (string skin in anims.Keys)
        {
            list += skin + " :\n";
            foreach (string capacity in anims[skin].Keys)
            {
                list += "\t" + capacity + " :\n";
                foreach (Anim anim in anims[skin][capacity])
                {
                    list += "\t\t" + anim.name + "\n";
                    count++;
                }
            }
        }
        return title + count + " animations\n" + list;
    }
}

[Serializable] public class Anim 
{
    // stocke UNE animation
    // ainsi que quelques parametres utiles au AnimHandler
    public string name; // nom de l'animation au format : skin.capacity.orientation
    public string skin { get { return name.Split('.')[0]; } }
    public string capacity { get { return name.Split('.')[1]; } }
    public string orientation { get { return name.Split('.')[2]; } }
    
    // stockage utile de l'animation
    public string[] sprites_paths;
    public Sprite[] sprites;
    public float[] sprites_durations;
    

    // parametres utiles à l'AnimPlayer
    public bool loop = true; // si c'est false, l'AnimHandler revient sur l'animation par defaut
    public float speed = 1f; // vitesse de l'animation
    public bool flipX = false; // flip le sprite renderer si besoin

    public Anim() { }
    public Anim(string name, string[] sprites_paths, float[] sprites_durations)
    {
        this.name = name;

        this.sprites_paths = sprites_paths;
        this.sprites_durations = sprites_durations;
    }
    public Anim(string name, string[] sprites_paths, float[] sprites_durations, bool loop=true, float speed=1f, /* int priority=0,  */bool flipX=false)
    {
        this.name = name;
        this.loop = loop;
        this.speed = speed;
        // this.priority = priority;

        this.sprites_paths = sprites_paths;
        this.sprites_durations = sprites_durations;
    }

    public Anim(Anim other)
    {
        this.name = other.name;
        this.loop = other.loop;
        this.speed = other.speed;
        // this.priority = other.priority;
        this.flipX = other.flipX;

        this.sprites_paths = other.sprites_paths;
        this.sprites_durations = other.sprites_durations;
    }

    // Sprite Loading
    public void LoadSprites(string spritesheets_path)
    {

        // on se prépare à stocker les sprites
        sprites = new Sprite[sprites_paths.Length];

        // on prépare la ram pour les spritesheets
        Dictionary<string,Sprite[]> spritesheets = new();

        // on parcours tous les sprites qu'on cherche
        for (int i = 0; i < sprites_paths.Length; i++)
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
                sprites[i] = spritesheet.FirstOrDefault(sprite => sprite.name == sprite_name);
                continue;
            }

            // si on est là on a déjà chargé le spritesheet
            // on cherche le bon sprite
            Sprite sprite = spritesheets[spritesheet_path].FirstOrDefault(sprite => sprite.name == sprite_name);

            // on ajoute le bon sprite à la liste
            sprites[i] = sprite;
        }
    }

    // getters
    public bool IsNameCorrect(string name)
    {
        string[] splitted_name = name.Split('.');
        if (splitted_name.Length != 3) { return false; }
        return true;
    }

    public float GetDuration()
    {
        float duration = 0f;
        foreach (float d in sprites_durations)
        {
            duration += d;
        }
        return duration / speed;
    }
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

}