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
    public Dictionary<string, Dictionary<string, List<Anim>>> anims = new();
    // if this is true, the bank will load animations from .anim files and store them as .json files
    // if false, the bank will load animations from .json files (/!\ you need to have the .json files in the Resources/anims/ folder)
    public bool extract_json_on_load = false;
    public string anims_path = "animations/";
    public string jsons_path = "anims/";


    [Header("Animations parameters")]
    // store the capacities that have parameters override
    // private List<string> capacities_with_parameters_override = new List<string>() { "attack","hurted","dodge" }; // todo obsolete ??
    // store the loops of the capacities
    [SerializeField] private List<string> capacities_with_no_loops = new List<string>() { "attack", "hurted", "dodge" };

    [Header("Sprites")]
    public string spritesheets_path = "spritesheets/";


    [Header("Skins management")]
    public List<SkinVariant> skin_variants = new List<SkinVariant>();

    [Header("Logs")]
    public bool log = false;
    public bool log_LAFAC = false;
    public bool log_variant_skins = false;

    [Header("Logs Runtime")]
    public bool log_get_anim = false;

    // AWAKE & SINGLETON LOGIC
    public static AnimBank Instance { get; private set; }
    private void Awake()
    {
        // singleton logic
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        // load anims
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

            if (log) { Debug.Log("(AnimBank - LoadAnims) Extracting animations from AnimationClips and saving them as .json files."); }

            // if in the editor, we load AnimationClips and store them as .json files
            string[] anims_paths = Directory.GetFiles("Assets/Resources/" + anims_path, "*.anim", SearchOption.AllDirectories);
            if (log) { Debug.Log("(AnimBank - LoadAnims) Found " + anims_paths.Length + " animations :\n\t" + string.Join("\n\t", anims_paths)); }
            foreach (string path in anims_paths)
            {
                // we remove Assets/Resources/anims/ from the path
                string new_path = path.Replace("Assets/Resources/" + anims_path, "");
                new_path = new_path.Replace(".anim", "");
                loadAnimFromAnimationClip(new_path);
            }

            // we generate the skins variants if we have some
            foreach (SkinVariant skin_variant in skin_variants)
            {
                generateVariantSkin(skin_variant);
            }

            return;
#endif
        }

        if (log) { Debug.Log("(AnimBank - LoadAnims) Loading animations from .json files."); }

        // if in the build or not extracting from .anim, we load .json files
        TextAsset[] jsons = Resources.LoadAll<TextAsset>(jsons_path);
        string[] json_paths = new string[jsons.Length];
        for (int i = 0; i < jsons.Length; i++)
        {
            json_paths[i] = jsons[i].name;
        }

        if (log) { Debug.Log("(AnimBank - LoadAnims) Found " + json_paths.Length + " jsons :\n\t" + string.Join("\n\t", json_paths)); }
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

        if (log_LAFAC) { Debug.Log("(AnimBank - LAFAC) Loading animation from AnimationClip : " + path); }

        // on charge l'animation depuis le path
        AnimationClip clip = Resources.Load<AnimationClip>("animations/" + path);
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
            if (log_LAFAC) { Debug.LogWarning("(AnimBank - LAFAC) Animation name format is incorrect : " + anim.name); }
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
        if (log_LAFAC) { Debug.LogWarning("(AnimBank - ExtractDataFromAnimationClip) No sprite curve found in the animation clip."); }
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
            sprites_durations[i] = (i == frameCount - 1)
                    ? 0f // last frame duration is 0
                    : spriteCurve[i + 1].time - spriteCurve[i].time; // other frames duration are calculed with the next frame time - actual frame time

            // we are extracting data in this sense for it to be the same as Unity default AnimationEditor
            // before we did it in the opposite (frame 0 has a 0f duration) but the result is then different from the editor

            // save the sprite path
            Sprite sprite = spriteCurve[i].value as Sprite;
            if (sprite == null)
            {
                if (log_LAFAC) { Debug.LogWarning("(AnimBank - ExtractSpriteCurve) Sprite not found in the animation clip. Skipping Anim : " + clip.name); }
                return null;
            }
            string spritePath = AssetDatabase.GetAssetPath(sprite).Replace(".png", "").Replace("Assets/Resources/" + spritesheets_path, "");
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




    // SKIN VARIANT MANAGEMENT
    private void generateVariantSkin(SkinVariant skin_variant)
    {
        // we check if the base skin exists
        if (!HasSkin(skin_variant.base_skin))
        {
            if (log_variant_skins) { Debug.LogWarning("(AnimBank - GenerateVariantSkin) Base skin not found in the bank : " + skin_variant.base_skin); }
            return;
        }

        // we check if the variant skin already exists
        if (HasSkin(skin_variant.variant_name))
        {
            if (log_variant_skins) { Debug.LogWarning("(AnimBank - GenerateVariantSkin) Variant skin already exists in the bank : " + skin_variant.variant_name); }
            return;
        }

        // we create the variant skin
        anims.Add(skin_variant.variant_name, new Dictionary<string, List<Anim>>());

        // we prepare the List<Sprite[]> (list of spritesheets)
        List<Sprite[]> base_sprites = new List<Sprite[]>();
        List<Sprite[]> variant_sprites = new List<Sprite[]>();

        // we load base spritesheets
        foreach (string spritesheet in skin_variant.base_spritesheets)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(spritesheets_path + spritesheet);
            if (sprites == null || sprites.Length == 0)
            {
                if (log_variant_skins) { Debug.LogWarning("(AnimBank - GenerateVariantSkin) Base spritesheet not found : " + spritesheet); }
                return;
            }
            base_sprites.Add(sprites);
        }

        // we load variant spritesheets
        foreach (string spritesheet in skin_variant.variant_spritesheets)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(spritesheets_path + spritesheet);
            if (sprites == null || sprites.Length == 0)
            {
                if (log_variant_skins) { Debug.LogWarning("(AnimBank - GenerateVariantSkin) Variant spritesheet not found : " + spritesheet); }
                return;
            }
            variant_sprites.Add(sprites);
        }



        // // we load the sprites
        // Sprite[] variant_sprites = Resources.LoadAll<Sprite>(skin_variant.variant_sprite_path);
        // if (variant_sprites == null || variant_sprites.Length == 0)
        // {
        //     if (log_variant_skins) { Debug.LogWarning("(AnimBank - GenerateVariantSkin) Variant sprite not found : " + skin_variant.variant_sprite_path); }
        //     return;
        // }
        // Sprite[] base_sprites = Resources.LoadAll<Sprite>(skin_variant.base_sprite_path);
        // if (base_sprites == null || base_sprites.Length == 0)
        // {
        //     if (log_variant_skins) { Debug.LogWarning("(AnimBank - GenerateVariantSkin) Base sprite not found : " + skin_variant.base_sprite_path); }
        //     return;
        // }

        // we copy all the animations from the base skin to the variant skin
        foreach (string capacity in anims[skin_variant.base_skin].Keys)
        {
            anims[skin_variant.variant_name].Add(capacity, new List<Anim>());

            foreach (Anim base_anim in anims[skin_variant.base_skin][capacity])
            {
                create_variant_anim(skin_variant, base_anim, base_sprites, variant_sprites);
            }
        }

        if (log_variant_skins) { Debug.Log("(AnimBank - GenerateVariantSkin) Variant skin generated : " + skin_variant.variant_name); }
    }
    private Anim create_variant_anim(SkinVariant skin_variant, Anim anim, List<Sprite[]> base_sprites, List<Sprite[]> variant_sprites)
    {
        // creates a variant animation based on the anim anim and with skin variant
        if (anim.skin != skin_variant.base_skin)
        {
            if (log_variant_skins) { Debug.LogWarning("(AnimBank - CreateVariantAnim) The anim skin is not the same as the base skin of the variant : " + anim.name); }
            return null;
        }

        // we copy the animation
        Anim variant_anim = new Anim(anim);
        variant_anim.name = skin_variant.variant_name + "." + anim.capacity + "." + anim.orientation;

        // we prepare for checking in which Sprite[] is every sprite
        int preferred_spritesheet_index = 0;
        Sprite[] preferred_spritesheet = base_sprites[preferred_spritesheet_index];


        // we replace the sprites in the animation by the variant sprite
        for (int i = 0; i < variant_anim.sprites.Length; i++)
        {
            // we check in which spritesheet the sprite is
            if (!preferred_spritesheet.Contains(variant_anim.sprites[i]))
            {
                // we look for the right spritesheet
                bool found = false;
                for (int j = 0; j < base_sprites.Count; j++)
                {
                    if (base_sprites[j].Contains(variant_anim.sprites[i]))
                    {
                        preferred_spritesheet_index = j;
                        preferred_spritesheet = base_sprites[preferred_spritesheet_index];
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    if (log_variant_skins) { Debug.LogWarning("(AnimBank - CreateVariantAnim) Sprite not found in any base spritesheet : " + variant_anim.sprites[i].name + " in anim " + anim.name); }
                    continue;
                }
            }

            // we get the index in the base_sprite list
            int sprite_index = Array.IndexOf(preferred_spritesheet, variant_anim.sprites[i]);
            if (sprite_index == -1) { continue; } // if the sprite is not found in the base_sprites, we skip it
            Sprite variant_sprite = variant_sprites[preferred_spritesheet_index][sprite_index];
            // variant_anim.sprites[i] = variant_sprite;

            // we replace the sprite path
            string spritePath = AssetDatabase.GetAssetPath(variant_sprite).Replace(".png", "").Replace("Assets/Resources/" + spritesheets_path, "");
            spritePath += "." + variant_sprite.name;
            variant_anim.sprites_paths[i] = spritePath;
        }

        // we clean the sprites
        variant_anim.sprites = null;

        // we save the json
        saveAnimToJson(variant_anim);

        // we add the anim (and so load the sprites)
        AddAnim(variant_anim);

        return variant_anim;
    }

#endif


    // BANK MANAGEMENT
    private void AddAnim(Anim anim)
    {
        // check if the animation already exists
        if (HasAnim(anim.name))
        {
            if (log) { Debug.LogWarning("(AnimBank - AddAnim) Animation already exists in the bank : " + anim.name); }
            return;
        }

        // prepare the skin and capacity
        string[] splitted_name = anim.name.Split('.');
        string skin = splitted_name[0];
        string capacity = splitted_name[1];

        // check if the capacity has parameters override
        /* int index = capacities_with_parameters_override.IndexOf(capacity);
        if (index != -1)
        {
            anim.loop = capacities_loops[index];
            // anim.priority = capacities_priorities[index];
        } */
        if (capacities_with_no_loops.Contains(capacity))
        {
            anim.loop = false; // if the capacity is in the no loops list, we set the loop to false
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


        // we check if the orientation has "LR" or "RL" in it.
        // if it does, we add the animation to the bank with "LR" replaced by "L" and "R" & "RL" replaced by "R" & "L"
        // "LR" stands for basic anim is facing L & "RL" is for basic anim is facing R
        if (splitted_name[2].Contains("LR") || splitted_name[2].Contains("RL"))
        {
            // we copy the animation
            Anim anim_R = new Anim(anim);
            anim_R.name = skin + "." + capacity + "." + splitted_name[2].Replace("LR", "R").Replace("RL", "L");
            anim_R.flipX = true;

            // load the sprites
            anim_R.LoadSprites(spritesheets_path);

            // we add the animation to the bank
            anims[skin][capacity].Add(anim_R);

            // we change the anim name
            anim.name = skin + "." + capacity + "." + splitted_name[2].Replace("LR", "L").Replace("RL", "R");
        }

        // load the sprites
        anim.LoadSprites(spritesheets_path);

        // add the animation to the bank
        anims[skin][capacity].Add(anim);
    }
    public Anim GetAnim(string name)
    {
        // if (log_get_anim) { Debug.Log("(AnimBank - GetAnim) Getting animation : " + name); }
        string[] splitted_name = name.Split('.');
        string skin = splitted_name[0];
        string capacity = splitted_name[1];
        string orientation = splitted_name[2];

        // check if we do not have the skin
        if (!anims.ContainsKey(skin))
        {
            if (log_get_anim) { Debug.LogWarning($"(AnimBank - GetAnim : {skin}.{capacity}.{orientation} ) Skin not found, returning sphere anim"); }
            return anims["sphere"]["idle"][0]; // return the sphere anim of the sphere skin
        }

        // check if we do not have the capacity
        if (!anims[skin].ContainsKey(capacity) || anims[skin][capacity].Count == 0)
        {
            // return the idle anim of the skin
            if (!anims[skin].ContainsKey("idle") || anims[skin]["idle"].Count == 0)
            {
                if (log_get_anim) { Debug.LogWarning($"(AnimBank - GetAnim : {skin}.{capacity}.{orientation} ) Idle anim not found for skin, returning sphere anim"); }
                return anims["sphere"]["idle"][0]; // return the sphere anim of the sphere skin
            }
            if (log_get_anim) { Debug.LogWarning($"(AnimBank - GetAnim : {skin}.{capacity}.{orientation} ) Capacity not found, returning idle"); }
            return GetAnim(skin + ".idle." + orientation);
        }

        // return the best orientation recursively
        return get_closest_orientation_anim(skin, capacity, orientation);
    }
    private Anim get_closest_orientation_anim(string skin, string capacity, string orientation)
    {
        // checks if we have the perfect animation (orientation)
        Anim exact_anim = anims[skin][capacity].Find(anim => anim.orientation == orientation);
        if (exact_anim != null) { return exact_anim; }

        if (log_get_anim) { Debug.LogWarning($"(AnimBank - GetClosestOrientationAnim : {skin}.{capacity}.{orientation} ) Orientation not found, trying to find the closest one"); }

        // todo improve this
        // if we have only one letter in the orientation (L,R,U or D) we turn to find the closest other one letter
        if (orientation == "U") { return get_closest_orientation_anim(skin, capacity, "L"); }
        else if (orientation == "L") { return get_closest_orientation_anim(skin, capacity, "D"); }
        else if (orientation == "D") { return get_closest_orientation_anim(skin, capacity, "R"); }
        else if (orientation == "R") { return get_closest_orientation_anim(skin, capacity, "U"); }

        // checks some special cases
        if (skin == "zombo" && capacity == "attack" && (orientation == "LD" || orientation == "RD"))
        { return get_closest_orientation_anim(skin, capacity, "D"); }

        // if we have a 2 letters orientation (LU,LD,RU,RD) we delete the 2nd letter (and so we look either for L or R)
        else { return get_closest_orientation_anim(skin, capacity, orientation[0].ToString()); }
    }

    /// <summary>
    /// get all animations of a specific skin & specific capacity.
    /// basically all animations that matches skin.capacity.*
    /// 
    /// this method does not try to find the *best match* as GetAnim(string name) does. it only returns
    /// the list of anim inside of our anims[skin][capacity]. it means that in the return list it will only be single animations.
    /// can't be double, but some orientations can miss
    /// </summary>
    /// <param name="skin">the skin we want the animations from</param>
    /// <param name="capacity">and the capacity</param>
    /// <returns>the list of all animations found, </returns>
    public List<Anim> GetOrientationAnims(string skin, string capacity)
    {
        // returns all the animations of a skin and capacity with all orientations
        if (!HasSkin(skin))
        {
            if (log) { Debug.LogWarning("(AnimBank - GetOrientationAnims) Skin not found in the bank : " + skin); }
            return new List<Anim>();
        }
        if (!HasCapacity(skin + "." + capacity))
        {
            if (log) { Debug.LogWarning("(AnimBank - GetOrientationAnims) Capacity not found in the bank : " + skin + "." + capacity); }
            return new List<Anim>();
        }
        return anims[skin][capacity];
    }

    // PUBLIC GETTERS
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


    // SKINS MANAGEMENT
    [Header("Skins management")]
    public List<string> skins = new List<string>() { "perso", "cat", "zombo", "robot", "rat", "nobody" };
    public List<float> head_offset_per_skin = new List<float>() { 0.7f, 0.7f, 0.7f, 0.7f, 0.7f, 0.7f };
    public List<float> body_offset_per_skin = new List<float>() { 0.4f, 0.15f, 0.4f, 0.2f, 0.1f, 0.42f };
    public float GetHeadOffset(string skin)
    {
        int index = skins.IndexOf(skin);
        if (index == -1)
        {
            // checks if it's in the variant skins -> try to return the base skin head offset
            if (skin_variants.Exists(variant => variant.variant_name == skin))
            {
                SkinVariant variant = skin_variants.Find(variant => variant.variant_name == skin);
                return GetHeadOffset(variant.base_skin);
            }

            return 0f;
        }
        return head_offset_per_skin[index];
    }
    public float GetBodyOffset(string skin)
    {
        int index = skins.IndexOf(skin);
        if (index == -1)
        {
            // checks if it's in the variant skins -> try to return the base skin head offset
            if (skin_variants.Exists(variant => variant.variant_name == skin))
            {
                SkinVariant variant = skin_variants.Find(variant => variant.variant_name == skin);
                return GetBodyOffset(variant.base_skin);
            }

            return 0f;
        }
        return body_offset_per_skin[index];
    }
    public Sprite GetDefaultSprite(string skin)
    {
        if (!HasSkin(skin))
        {
            if (log) { Debug.LogWarning("(AnimBank - GetDefaultSprite) Skin not found in the bank : " + skin); }
            return null;
        }

        if (!HasCapacity(skin + ".idle"))
        {
            if (log) { Debug.LogWarning("(AnimBank - GetDefaultSprite) Capacity idle not found in the bank for skin : " + skin); }
            return null;
        }

        Anim anim = anims[skin]["idle"].Find(a => a.orientation == "D");
        if (anim == null)
        {
            if (log) { Debug.LogWarning("(AnimBank - GetDefaultSprite) Default animation not found for skin : " + skin); }
            return null;
        }

        return anim.sprites[0];
    }

    // DEBUG
    private string getAnimsList()
    {
        string title = "(AnimBank) ANIMS in the bank - Total ";
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

[Serializable]
public class Anim
{
    // stocke UNE animation
    // ainsi que quelques parametres utiles au AnimHandler
    public string name; // nom de l'animation au format : skin.capacity.orientation
    public string skin { get { return name.Split('.')[0]; } }
    public string capacity { get { return name.Split('.')[1]; } }
    public string orientation { get { return name.Split('.')[2]; } }
    // todo pas opti, faire l'inverse : stocker chaque string skin,capacity,orientation
    // todo et faire une propertie name qui concatene les 3

    // stockage utile de l'animation
    public string[] sprites_paths;
    public Sprite[] sprites;
    public float[] sprites_durations;


    // parametres utiles à l'AnimPlayer
    public bool loop = true; // si c'est false, l'AnimPlayer revient sur l'animation par defaut
    public float speed = 1f; // vitesse de l'animation
    public bool flipX = false; // flip le sprite renderer si besoin

    public Anim() { }
    public Anim(string name, string[] sprites_paths, float[] sprites_durations)
    {
        this.name = name;

        this.sprites_paths = sprites_paths;
        this.sprites_durations = sprites_durations;
    }
    public Anim(string name, string[] sprites_paths, float[] sprites_durations, bool loop = true, float speed = 1f, /* int priority=0,  */bool flipX = false)
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
        this.sprites = other.sprites;
        this.sprites_durations = other.sprites_durations;
    }

    // Sprite Loading
    public void LoadSprites(string spritesheets_path)
    {

        // on se prépare à stocker les sprites
        sprites = new Sprite[sprites_paths.Length];

        // on prépare la ram pour les spritesheets
        Dictionary<string, Sprite[]> spritesheets = new();

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

}


[Serializable]
public class SkinVariant
{
    public string variant_name;
    public List<string> variant_spritesheets;
    public string base_skin;
    public List<string> base_spritesheets;
}