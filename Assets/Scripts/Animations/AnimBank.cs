using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Linq;

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
    public Dictionary<string,Dictionary<string,List<Anim>>> anims = new();
    // store all the animations with the keys : skin, capacity, orientation
    public bool extract_json_on_load = false;
    // if this is true, the bank will load animations from .anim files and store them as .json files
    // if false, the bank will load animations from .json files (/!\ you need to have the .json files in the Resources/anims/ folder)
    public string anims_path = "animations/";
    public string jsons_path = "anims/";


    [Header("Animations parameters")]
    public List<string> capacities_with_parameters_override = new List<string>() { "attack","hurted","die" };
    // store the capacities that have parameters override
    public List<bool> capacities_loops = new List<bool>() { false,false,true};
    // store the loops of the capacities
    public List<int> capacities_priorities = new List<int>() { 1,2,3 };
    // store the priorities of the capacities


    [Header("Sprites")]
    public string spritesheets_path = "spritesheets/";
    public Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    // store all the sprites used in the animations
    // with the key being the spritesheet name (with folders) and the value being the name of the sprite itself



    // INITIALIZATION
    private void Awake()
    {
        LoadAnims();
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

                Debug.Log("(AnimBank - LoadAnims) Extracting animations from AnimationClips and saving them as .json files.");

                // if in the editor, we load AnimationClips and store them as .json files
                string[] anims_paths = Directory.GetFiles("Assets/Resources/" + anims_path, "*.anim", SearchOption.AllDirectories);
                Debug.Log("(AnimBank - LoadAnims) Found " + anims_paths.Length + " animations :\n\t" + string.Join("\n\t", anims_paths));
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

        Debug.Log("(AnimBank - LoadAnims) Loading animations from .json files.");

        // if in the build or not extracting from .anim, we load .json files
        TextAsset[] jsons = Resources.LoadAll<TextAsset>(jsons_path);
        string[] json_paths = new string[jsons.Length];
        for (int i = 0; i < jsons.Length; i++)
        {
            json_paths[i] = jsons[i].name;
        }

        Debug.Log("(AnimBank - LoadAnims) Found " + json_paths.Length + " jsons :\n\t" + string.Join("\n\t", json_paths));
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

        Debug.Log("(AnimBank - LAFAC) Loading animation from AnimationClip : " + path);
        // on charge l'animation depuis le path
        AnimationClip clip = Resources.Load<AnimationClip>("animations/" + path /* + ".anim" */);
        if (!clip)
        {
            Debug.LogError("(AnimBank - LAFAC) AnimationClip NOT found in : animations/" + path);
            return;
        }

        // on trouve le bon nom pour l'animation
        // string[] splitted_path = path.Split('/');
        // string anim_name = splitted_path[splitted_path.Length - 1];

        Anim anim = extractDataFromAnimationClip(clip);
        if (anim == null) { return; }
        if (!anim.isNameCorrect(anim.name))
        {
            Debug.LogWarning("(AnimBank - LAFAC) Animation name format is incorrect : " + anim.name);
            return;
        }

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
        Debug.LogWarning("(AnimBank - ExtractDataFromAnimationClip) No sprite curve found in the animation clip.");
        return null;
    }
    private Anim extractSpriteCurve(AnimationClip clip, EditorCurveBinding binding)
    {

        // Extract the curve for the SpriteRenderer.sprite property
        var spriteCurve = AnimationUtility.GetObjectReferenceCurve(clip, binding);
        int frameCount = spriteCurve.Length;

        string[] sprites_paths = new string[frameCount];
        float[] frame_times = new float[frameCount];

        for (int i = 0; i < frameCount; i++)
        {
            // save the frame time
            frame_times[i] = (i == 0) ? spriteCurve[i].time : spriteCurve[i].time - spriteCurve[i - 1].time;
            
            // save the sprite path
            Sprite sprite = spriteCurve[i].value as Sprite;
            if (sprite == null)
            {
                Debug.LogWarning("(AnimBank - ExtractSpriteCurve) Sprite not found in the animation clip. Skipping Anim : " + clip.name);
                return null;
            }
            string spritePath = AssetDatabase.GetAssetPath(sprite).Replace(".png","").Replace("Assets/Resources/" + spritesheets_path, "");
            spritePath += "." + sprite.name;
            sprites_paths[i] = spritePath;
        }

        // we create a new Anim object
        Anim anim = new Anim(clip.name, sprites_paths, frame_times);
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

        // check if the name format is correct


        // check if the animation already exists
        if (HasAnim(anim.name))
        {
            Debug.LogWarning("(AnimBank - AddAnim) Animation already exists in the bank : " + anim.name);
            return;
        }
        
        // add the animation to the bank
        string[] splitted_name = anim.name.Split('.');
        string skin = splitted_name[0];
        string capacity = splitted_name[1];

        // check if the capacity has parameters override
        int index = capacities_with_parameters_override.IndexOf(capacity);
        if (index != -1)
        {
            anim.loop = capacities_loops[index];
            anim.priority = capacities_priorities[index];
        }

        if (!HasSkin(skin))
        {
            anims.Add(skin, new Dictionary<string, List<Anim>>());
        }
        if (!HasCapacity(skin + "." + capacity))
        {
            anims[skin].Add(capacity, new List<Anim>());
        }

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
        
        Debug.LogWarning("(AnimBank - GetAnim) Animation not found in the bank : " + name);
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


    // SPRITES MANAGEMENT
    public Sprite[] GetSprites(string[] sprites_paths)
    {
        // on cherche les sprites dans la liste
        Sprite[] sprites = new Sprite[sprites_paths.Length];
        for (int i = 0; i < sprites_paths.Length; i++)
        {
            if (this.sprites.ContainsKey(sprites_paths[i]))
            {
                sprites[i] = this.sprites[sprites_paths[i]];
            }
            else
            {
                sprites[i] = LoadSprite(sprites_paths[i]);
            }
        }
        return sprites;
    }
    public Sprite GetSprite(string name)
    {
        // on cherche le sprite dans la liste
        if (sprites.ContainsKey(name))
        {
            return sprites[name];
        }
        Debug.LogWarning("(AnimBank - GetSprite) Sprite not found in the bank : " + name);
        return null;
    }
    private void LoadSpriteSheet(string path)
    {
        // on charge le spritesheet depuis le path
        Sprite[] spritesheet = Resources.LoadAll<Sprite>(spritesheets_path + path);
        if (spritesheet.Length == 0)
        {
            Debug.LogError("(AnimBank - LoadSpriteSheet) Spritesheet NOT found in : " + spritesheets_path + path);
            return;
        }

        // on ajoute chaque sprite du spritesheet à la liste
        foreach (Sprite sprite in spritesheet)
        {
            sprites.Add(path + "." + sprite.name, sprite);
        }
    }
    private Sprite LoadSprite(string path)
    {
        // on récupère le path du spritesheet
        string[] splitted_path = path.Split('.');
        string spritesheet_path = splitted_path[0];
        LoadSpriteSheet(spritesheet_path);

        // on retourne le sprite
        return GetSprite(path);
    }
}

[Serializable] public class Anim 
{
    // stocke UNE animation
    // ainsi que quelques parametres utiles au AnimHandler
    public string name; // nom de l'animation au format : skin.capacity.orientation
    
    
    // stockage utile de l'animation
    public string[] sprites_paths;
    public float[] frame_times;
    
    // public AnimationClip clip; 
    public bool loop = true; // si c'est false, l'AnimHandler revient sur l'animation par defaut
    public float speed = 1f; // vitesse de l'animation
    public int priority = 0; // si on demande de jouer une animation, celle-ci sera jouée si sa priorité est plus haute que celle en cours

    public Anim() { }
    public Anim(string name, string[] sprites_paths, float[] frame_times)
    {
        this.name = name;

        this.sprites_paths = sprites_paths;
        this.frame_times = frame_times;
    }
    public Anim(string name, string[] sprites_paths, float[] frame_times, bool loop=true, float speed=1f, int priority=0)
    {
        this.name = name;
        this.loop = loop;
        this.speed = speed;
        this.priority = priority;

        this.sprites_paths = sprites_paths;
        this.frame_times = frame_times;
    }


    // getters
    public bool isNameCorrect(string name)
    {
        string[] splitted_name = name.Split('.');
        if (splitted_name.Length != 3) { return false; }
        return true;
    }
}