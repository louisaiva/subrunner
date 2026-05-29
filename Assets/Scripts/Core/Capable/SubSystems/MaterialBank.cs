using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class MaterialBank : MonoBehaviour
{
    private static MaterialBank _instance;
    private static MaterialBank instance
    {
        get
        {
            if (_instance == null) { _instance = FindFirstObjectByType<MaterialBank>(FindObjectsInactive.Include); }
            return _instance;
        }
    }

    [Header("Material Bank")]
    [SerializeField] private MaterialsPaths paths = new MaterialsPaths();
    private string json_data_path = "data/materials";
    private string materials_path = Path.Combine("Assets", "Resources", "materials");
    private Dictionary<string, Material> materials = new Dictionary<string, Material>();

    [Header("Defaults")]
    [SerializeField] private Material default_material;

    [Header("Logs")]
    [SerializeField] private Loggable<MaterialBank> log_loading;
    private static Loggable<MaterialBank> LogLoading => instance.log_loading;
    private static Loggable<MaterialBank> LogGetMaterial => instance.log_get_material;
    [SerializeField] private Loggable<MaterialBank> log_get_material;


    ///
    //
    ///  AWAKE & LOAD MATERIALS & ON DESTROY
    //
    ///

    private void Awake()
    {
        if (Application.isEditor)
        {
            load_materials_from_files();
        }
        else
        {
            load_materials_from_json();
        }

        log_loading.Log($"Loaded {paths.Count} paths and {materials.Count} materials into cache");
    }
    private void OnDestroy()
    {
        // we write our paths to disk if we are in the editor
        if (!Application.isEditor) { return; }
        
        string json = JsonUtility.ToJson(paths, prettyPrint: true);
        log_loading.Log($"Saving {paths.Count} paths as json to path : {json_data_path}\n{json}");
        AppManager.SaveJsonToAsset(json_data_path, json, verbose: log_loading.Verbose);
    }
    private void load_materials_from_files()
    {
        #if UNITY_EDITOR
        log_loading.Log($"[EDITOR] Loading materials from files in path: {materials_path}");

        // 
        List<string> directories = Directory.GetDirectories(materials_path, "*", SearchOption.AllDirectories).ToList();
        // List<string> directories = new List<string>();
        directories.Add(materials_path); // we also want to load materials directly in the materials folder
        foreach (string dir in directories)
        {
            string path = dir.Replace("Assets/Resources/", "").Replace("Assets\\Resources\\", "");
            log_loading.LogExtended($"Loading materials from directory: {dir}, path in resources: {path}");

            // we get all files in the directory and we try to load them as materials
            List<string> files = Directory.GetFiles(dir, "*.mat").ToList();
            log_loading.LogSpecific($"Found {files.Count} material files in directory: {dir}");
            foreach (string file in files)
            {
                string file_name = Path.GetFileNameWithoutExtension(file);
                log_loading.LogVerySpecific($"Loading material: {file_name} from path: {path}");
                string material_path = Path.Combine(path, file_name);
                Material material = Resources.Load<Material>(material_path);
                
                paths.Add(material.name, material_path);
                this.materials.Add(material.name, material);
                log_loading.LogOMGThatsVeryVerySpecific($"Loaded material: {material.name} situated at: {material_path}");
            }
        }

        #endif
    }
    private void load_materials_from_json()
    {
        log_loading.Log($"Loading materials from JSON in path: {json_data_path}");

        // we load paths from json data
        string json = AppManager.LoadJsonFromAsset(json_data_path);
        if (string.IsNullOrEmpty(json)) { log_loading.Error($"Failed to load materials paths from json data at path: {json_data_path}"); return; }
        paths = JsonUtility.FromJson<MaterialsPaths>(json);

        // then we load all materials in paths and add them to cache
        Material material;
        List<string> ids = paths.Ids;
        string path;
        foreach (string id in ids)
        {
            if (!paths.TryGetPath(id, out path))
            {
                log_loading.Error($"No path found for material with id: {id} in paths data at path: {json_data_path}");
                continue;
            }
            material = Resources.Load<Material>(path);
            if (material == null) { log_loading.Error($"Failed to load material with id: {id} from path: {path}"); continue; }
            this.materials.Add(id, material);
            log_loading.LogExtended($"Loaded material with id: {id} from path: {path}");
        }
    }


    ///
    //
    ///  MATERIAL GETTERS
    //
    ///

    public static string GetMaterialName(Material material)
    {
        string material_name = material.name.Replace("(Instance)", "").Trim();

        // check if we have the name in cache
        if (Application.isPlaying && !instance.paths.HasID(material_name))
        {
            LogGetMaterial.Warning($"The material with name : {material.name} was not found in paths even with trimmed name '{material_name}', returning default name");
            return instance.default_material.name;
        }
        else if (!Application.isPlaying)
        {
            LogGetMaterial.LogExtended($"[EDITOR] Getting material name for material: {material.name}, trimmed name: {material_name}");
        }
        return material_name;
    }
    public static Material GetMaterial(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            LogGetMaterial.Warning($"GetMaterial called with null or empty id, returning default material");
            return instance.default_material;
        }

        // try to find the material in cache
        if (instance.materials.TryGetValue(id, out Material material))
        {
            LogGetMaterial.LogSpecific($"Found material with id: {id} in cache");
            return material;
        }

        LogGetMaterial.Error($"Material with id: {id} not found in cache, returning default material");
        return instance.default_material;
    }

    // MATERIAL GETTER
    public static string GetMaterialName(SpriteRenderer sr, string debug_cap_id = "not specified")
    {
        if (sr == null || sr.sharedMaterial == null)
        {
            LogGetMaterial.Error($"[capable : '{debug_cap_id}'] The SpriteRenderer or its material is null, returning empty material name");
            return "";
        }

        string mat_name = GetMaterialName(sr.sharedMaterial);
        LogGetMaterial.Log($"[capable : '{debug_cap_id}'] get_material_name found the material name: {mat_name}");

        return mat_name;
    }
}

[Serializable] public class MaterialsPaths
{
    [SerializeField] private List<string> ids;
    [SerializeField] private List<string> paths;
    public List<string> Ids => ids;
    public int Count { get { return ids.Count; } }

    public void Add(string id, string path)
    {
        ids.Add(id);
        paths.Add(path);
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
    public bool HasID(string id)
    {
        return ids.Contains(id);
    }
}