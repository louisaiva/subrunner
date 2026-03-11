
using System;
using System.Collections.Generic;
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

    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
    }
    private void Start()
    {
        // if we have a MusicPlayer Instance
        MusicPlayer.Instance.PlayTheme(game_theme_to_play);
    }


    // USEFUL GLOBAL METHODS
    public bool IsKind(Type kind, Type ref_kind)
    {
        bool is_same_or_subclass = kind == ref_kind || kind.IsSubclassOf(ref_kind);
        return is_same_or_subclass;
    }
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
    public string[] LoadJsons(string data_folder)
    {
        TextAsset[] json_assets = Resources.LoadAll<TextAsset>(data_folder);
        string[] jsons = new string[json_assets.Length];
        for (int i=0; i<json_assets.Length; i++)
        {
            jsons[i] = json_assets[i].text;
        }
        return jsons;
    }

}