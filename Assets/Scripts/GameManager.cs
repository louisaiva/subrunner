
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;



/// <summary>
/// this class handles few global things related
/// to the game. It means it is created only when entering
/// a game scene (it does not exist in the main menu)
/// It is situated on "/game" gameObject
/// </summary>
public class GameManager : MonoBehaviour
{

    [Header("Game Music Theme")]
    [SerializeField] private string game_theme_to_play = "i'm so hungry";
    [SerializeField] private string world_to_load = "";

    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        // we instantly load the world even if World.Instance is not defined yet.
        // so we use World.StaticInstance which will find the world instance with FindObjectByType
        World world = World.StaticInstance;
        if (world == null) { return; }
        if (world_to_load != "") { world.LoadWorld(world_to_load); }
    }
    private void Start()
    {
        // if we have a MusicPlayer Instance
        MusicPlayer.Instance.PlayTheme(game_theme_to_play);
    }


    // USEFUL GLOBAL METHODS


    // KIND & TYPE CHECKING
    public static bool IsKind(Type kind, Type ref_kind)
    {
        bool is_same_or_subclass = kind == ref_kind || kind.IsSubclassOf(ref_kind);
        return is_same_or_subclass;
    }
    public static bool IsKind(string kind, string ref_kind)
    {
        Type type_kind = Type.GetType(kind);
        Type type_ref_kind = Type.GetType(ref_kind);
        if (type_kind == null || type_ref_kind == null) { return false; }
        return IsKind(type_kind, type_ref_kind);
    }
    public static bool IsKind(string kind, string ref_kind, out int inheritance_distance)
    {
        if (!IsKind(kind, ref_kind)) { inheritance_distance = -1; return false; }
        inheritance_distance = calculate_type_distance(Type.GetType(kind), Type.GetType(ref_kind));
        return true;
    }
    private static int calculate_type_distance_one_way(Type firstType, Type secondType)
    {
        var chain = new List<Type>();
        while (firstType != typeof(object))
        {
            chain.Add(firstType);
            firstType = firstType.BaseType;
        }

        return chain.IndexOf(secondType);
    }
    public static int calculate_type_distance(Type firstType, Type secondType)
    {
        int result = calculate_type_distance_one_way(firstType, secondType);
        if (result >= 0)
        {
            return result;
        }

        return calculate_type_distance_one_way(secondType, firstType);
    }


    // ID GENERATION
    [SerializeField] private List<string> generated_ids = new List<string>();
    public string GenerateUniqueID(string base_id)
    {
        // todo if we have perf issues when spawning capables this can be the issue
        // then we just need to have a static int that we increment so it's faster
        // or have a dict with max ids per prefix

        // we check if the base_id can be splitted with "_"
        string[] parts = base_id.Split('-');
        string suffix = parts.Length > 1 ? parts[parts.Length - 1] : "";
        string prefix = base_id.Substring(0, base_id.Length - suffix.Length);
        if (suffix == "") { prefix += "-"; } // if we got no suffix, we add a _ to the prefix so it will be alrgiht next time

        // we go through all generated_ids and memorize all the ids that have the same prefix and check the suffix int is greater or not
        int max_suffix = 0;
        foreach (string id in generated_ids)
        {
            if (id.StartsWith(prefix))
            {
                string id_suffix = id.Substring(prefix.Length);
                if (int.TryParse(id_suffix, out int id_suffix_int))
                {
                    if (id_suffix_int > max_suffix)
                    {
                        max_suffix = id_suffix_int;
                    }
                }
            }
        }

        // construct final id
        string new_id = prefix + (max_suffix + 1);
        generated_ids.Add(new_id);
        return new_id;
    }
    public void ClearGeneratedIDs() { generated_ids.Clear(); }


    // JSON DATA LOADING
    public string[] LoadJsonsFromAssets(string data_folder)
    {
        TextAsset[] json_assets = Resources.LoadAll<TextAsset>(data_folder);
        string[] jsons = new string[json_assets.Length];
        for (int i=0; i<json_assets.Length; i++)
        {
            jsons[i] = json_assets[i].text;
        }
        return jsons;
    }
    /* public string[] LoadJsonsFromPath(string data_folder)
    {
        // we get all the json files in the data folder and load them as strings
        string[] file_paths = System.IO.Directory.GetFiles(data_folder, "*.json");
        string[] jsons = new string[file_paths.Length];
        for (int i=0; i<file_paths.Length; i++)
        {
            jsons[i] = System.IO.File.ReadAllText(file_paths[i]);
        }
        return jsons;
    } */
    public string[] LoadJsonsFromWorldDataPath(string data_folder)
    {
        // loads jsons from the current world data path (which is in the persistent data path) instead of the assets
        // data_folder should be like "levels" for levels or "capables"
        string world_data_path = World.CurrentStaticWorldDataPath;
        string jsons_path = Path.Combine(world_data_path, data_folder);

        // we get all the json files in the data folder and load them as strings
        string[] file_paths = Directory.GetFiles(jsons_path, "*.json");
        string[] jsons = new string[file_paths.Length];
        for (int i = 0; i < file_paths.Length; i++)
        {
            jsons[i] = System.IO.File.ReadAllText(file_paths[i]);
        }
        return jsons;
    }
}