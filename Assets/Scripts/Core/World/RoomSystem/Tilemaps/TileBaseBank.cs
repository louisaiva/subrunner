using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileBaseBank : MonoBehaviour
{
    [Header("TileBase Bank")]
    [SerializeField] private Dictionary<string, TileBase> defaults = new Dictionary<string, TileBase>();
    [SerializeField] private Dictionary<string, TileBase> walls = new Dictionary<string, TileBase>();
    [SerializeField] private Dictionary<string, TileBase> grounds = new Dictionary<string, TileBase>();
    [SerializeField] private Dictionary<string, TileBase> ceilings = new Dictionary<string, TileBase>();
    [SerializeField] private Dictionary<string, TileBase> carpets = new Dictionary<string, TileBase>();
    [SerializeField] private TileBase default_tile;
    [SerializeField] private TileBasePaths paths = new TileBasePaths();
    public int Count { get { return walls.Count + grounds.Count + ceilings.Count + carpets.Count + defaults.Count; } }

    [Header("Logs")]
    [SerializeField] private Loggable<TileBaseBank> log;

    private string json_data_path = "data/tilebases";

    private void Awake()
    {
        // we load paths from json data
        string json = AppManager.LoadJsonFromAsset(json_data_path);
        if (string.IsNullOrEmpty(json)) { log.Error($"Failed to load tilebase paths from json data at path: {json_data_path}"); return; }
        paths = JsonUtility.FromJson<TileBasePaths>(json);

        // then we load all tilebases in paths and add them to cache
        TileBase tilebase;
        List<string> ids = paths.Ids;
        string path;
        foreach (string id in ids)
        {
            if (!paths.TryGetPath(id, out path))
            {
                log.Error($"No path found for tilebase with id: {id} in paths data at path: {json_data_path}");
                continue;
            }
            tilebase = load_tile_base_from_path(path);
            if (tilebase == null) { log.Error($"Failed to load tilebase with id: {id} from path: {path}"); continue; }
            add_to_dicts(id, tilebase, path);
            log.LogExtended($"Loaded tilebase with id: {id} from path: {path}");
        }
        log.Log($"Loaded {paths.Count} paths and {Count} tilebases from json data at path: {json_data_path}");
    }
    private void OnDestroy()
    {
        // we write our paths to disk if we are in the editor
        if (Application.isEditor)
        {
            string json = JsonUtility.ToJson(paths, prettyPrint: true);
            log.Log($"Saving {paths.Count} paths as json to path : {json_data_path}\n{json}");
            AppManager.SaveJsonToAsset(json_data_path, json, verbose: log.Verbose);
        }
    }


    // NAME -> TILEBASE
    public TileBase GetTileBaseFromName(string id)
    {
        if (string.IsNullOrEmpty(id)) { log.Warning($"Cannot get tilebase from null or empty id : returning default tile"); return default_tile; }

        if (try_get_in_dicts(id, out TileBase tilebase))
        {
            log.LogExtended($"Found tilebase with id: {id} in cache");
            return tilebase;
        }

        // we don't have the tilebase in cache, we try to load it from path
        // if (paths.TryGetValue(id, out string path))
        if (try_get_path_in_paths(id, out string path))
        {
            tilebase = load_tile_base_from_path(path);
            add_to_dicts(id, tilebase, path);
            log.Log($"Loaded tilebase with id: {id} from path: {path}");
            return tilebase;
        }

        log.Error($"No tilebase path found for id: {id}, returning default tile");
        return default_tile;
    }
    private void add_to_dicts(string id, TileBase tilebase, string path)
    {
        if (path.Contains("wall")) { walls[id] = tilebase; return; }
        if (path.Contains("ground")) { grounds[id] = tilebase; return; }
        if (path.Contains("ceiling")) { ceilings[id] = tilebase; return; }
        if (path.Contains("carpet")) { carpets[id] = tilebase; return; }
        // if we are here, it means that the id was not in any dict, we add it to defaults
        defaults[id] = tilebase;
    }
    private TileBase load_tile_base_from_path(string tilebase_path)
    {
        // we don't have the tilebase in cache, we load it and add it to cache
        TileBase tilebase = Resources.Load<TileBase>(tilebase_path);
        if (tilebase == null) { log.Error($"Failed to load tilebase at path: {tilebase_path}"); }
        return tilebase;
    }

    // TILEBASE -> NAME
    public string GetNameFromTileBase(TileBase tilebase)
    {
        if (try_get_in_dicts(tilebase, out string id))
        {
            log.LogExtended($"Found tilebase with name: {id} in cache");
            return id;
        }

        if (Application.isEditor)
        {
            #if UNITY_EDITOR
            string path = UnityEditor.AssetDatabase.GetAssetPath(tilebase);
            path = path.Replace("Assets/Resources/", "").Replace(".asset", "");

            // we don't have the tilebase in cache, but maybe we can find it in the paths dict ???
            if (try_get_id_in_paths(path, out string id_from_path))
            {
                log.LogExtended($"Found tilebase with name: {id_from_path} in paths dict for path: {path}");
                add_to_dicts(id_from_path, tilebase, path);
                return id_from_path;
            }

            // we still don't have a name for it, we take the last path part as name and add it to paths + cache
            string name = Path.GetFileName(path);
            paths.Add(name, path);
            add_to_dicts(name, tilebase, path);
            log.Log($"Added tilebase with name: {name} to cache from path: {path}");
            return name;

            #endif
        }

        // we don't have the tilebase in cache, which is weird !
        log.Error($"No name found for tilebase: {tilebase}");
        return null;
    }


    // LOW LEVEL GETTERS
    private bool try_get_path_in_paths(string id, out string path) { paths.TryGetPath(id, out path); return path != null; }
    private bool try_get_id_in_paths(string path, out string id) { paths.TryGetId(path, out id); return id != null; }
    private bool try_get_in_dicts(string id, out TileBase tilebase)
    {
        if (walls.TryGetValue(id, out tilebase)) { return true; }
        if (grounds.TryGetValue(id, out tilebase)) { return true; }
        if (ceilings.TryGetValue(id, out tilebase)) { return true; }
        if (carpets.TryGetValue(id, out tilebase)) { return true; }
        if (defaults.TryGetValue(id, out tilebase)) { return true; }
        return false;
    }
    private bool try_get_in_dicts(TileBase tilebase, out string id)
    {
        foreach (var kvp in walls) { if (kvp.Value == tilebase) { id = kvp.Key; return true; } }
        foreach (var kvp in grounds) { if (kvp.Value == tilebase) { id = kvp.Key; return true; } }
        foreach (var kvp in ceilings) { if (kvp.Value == tilebase) { id = kvp.Key; return true; } }
        foreach (var kvp in carpets) { if (kvp.Value == tilebase) { id = kvp.Key; return true; } }
        foreach (var kvp in defaults) { if (kvp.Value == tilebase) { id = kvp.Key; return true; } }
        id = null;
        return false;
    }

}

[Serializable] public class TileBasePaths
{
    [SerializeField] private List<string> ids;
    [SerializeField] private List<string> paths;
    public List<string> Ids => ids;
    public string PathOf(string id)
    {
        if (TryGetPath(id, out string path))
        {
            return path;
        }
        return null;
    }
    public int Count { get { return ids.Count; } }

    public void Add(string id, string path)
    {
        ids.Add(id);
        paths.Add(path);
    }
    public void Clear()
    {
        ids.Clear();
        paths.Clear();
    }

    public bool TryGetPath(string id, out string path)
    {
        int index = ids.IndexOf(id);
        if (index != -1)
        {
            path = paths[index];
            return true;
        }
        path = null;
        return false;
    }
    public bool TryGetId(string path, out string id)
    {
        int index = paths.IndexOf(path);
        if (index != -1)
        {
            id = ids[index];
            return true;
        }
        id = null;
        return false;
    }
}