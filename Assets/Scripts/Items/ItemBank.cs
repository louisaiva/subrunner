using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class ItemBank : MonoBehaviour
{ 

    [Header("Item Bank")]
    public List<string> items_path = new List<string>() { "prefabs/items" };
    public Dictionary<string, Sprite> item_sprites = new Dictionary<string, Sprite>();
    public Dictionary<string, string> item_prefabs = new Dictionary<string, string>();

    [Header("UI Icons")]
    public List<Sprite> ui_icons = new List<Sprite>();
    public List<string> ui_icons_names = new List<string>();

    [Header("Module Sprites")]
    public List<Sprite> module_sprites = new List<Sprite>();
    public List<string> module_references = new List<string>();

    [Header("UI")]
    public GameObject ui_item_prefab;
    public GameObject ui_module_prefab;

    [Header("Logs")]
    public bool debug = false;


    // constructor
    void Awake()
    {
        // on vérifie qu'on a un prefab pour l'UI
        if (ui_item_prefab == null)
        {
            Debug.LogError("(ItemBank) missing ui_item_prefab, you need to set it in the inspector");
        }

        // on charge les items
        loadItems();
        Debug.Log(getItemsList());
    }
    public void init(Sprite[] fake_sprites) {}
    public void loadItems()
    {
        int item_count = 0;

        // on récupère les prefabs des items
        foreach (string path in items_path)
        {
            // on récupère les prefabs
            GameObject[] prefabs = Resources.LoadAll<GameObject>(path);

            foreach (GameObject prefab in prefabs)
            {
                // on récupère la reference de l'item
                Item item = prefab.GetComponent<Item>();
                if (item == null)
                {
                    if (debug) { Debug.LogWarning("(ItemBank) prefab " + prefab.name + " has no Item component, skipping it");}
                    continue;
                }
                string reference = item.Reference;

                // on ajoute le prefab des item
                item_prefabs.Add(reference, path + "/" + prefab.name);

                // on ajoute le sprite de l'item
                Sprite sprite = prefab.GetComponent<SpriteRenderer>().sprite;
                item_sprites.Add(reference, sprite);

                item_count++;

                if (debug) { Debug.Log("(ItemBank) loaded item : " + reference +
                        (reference == prefab.name ? "" : " (prefab name is " + prefab.name + ")")); }
            }
        }

        if (debug) { Debug.Log("(ItemBank) loaded " + item_count + " items"); }
    }

    // ITEM GENERATOR
    public GameObject CreateItem(string item_name)
    {
        // on récupère le prefab de l'item
        if (!item_prefabs.ContainsKey(item_name))
        {
            Debug.LogError("(ItemBank) cannot find prefab " + item_name);
            return null;
        }

        string prefab_path = item_prefabs[item_name];
        GameObject prefab = Resources.Load<GameObject>(prefab_path);

        // on instancie le prefab
        GameObject item = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        // on change le nom du prefab
        item.name = item_name;

        if (debug) { Debug.Log("(ItemBank) created item : " + item_name); }

        return item;
    }

    // UI_ITEM GENERATOR
    public GameObject CreateUI_Item()
    {
        // on instancie le prefab
        GameObject ui_item = Instantiate(ui_item_prefab, Vector3.zero, Quaternion.identity);

        return ui_item;
    }
    public GameObject CreateUI_Module()
    {
        // we create the module
        GameObject module = Instantiate(ui_module_prefab, Vector3.zero, Quaternion.identity);

        return module;
    }

    // GETTERS
    public Sprite GetSprite(string item_reference)
    {
        if (item_reference.Contains("paper:"))
        {
            return GetSprite("other:paper");
        }

        if (!item_sprites.ContainsKey(item_reference))
            {
                Debug.LogError("(ItemBank) cannot find sprite " + item_reference
                    + ". are you sure its corresponding item prefab is in the " + items_path + " folder?");
                return null;
            }

        return item_sprites[item_reference];
    }
    public Sprite GetUI_Icon(string icon_name)
    {
        // we check if the icon exists
        if (!ui_icons_names.Contains(icon_name))
        {
            Debug.LogError("(ItemBank) cannot find UI icon for " + icon_name);
            return null;
        }
        int index = ui_icons_names.IndexOf(icon_name);

        if (index >= ui_icons.Count)
        {
            Debug.LogError("(ItemBank) UI icons list is not initialized correctly, check the inspector");
            return null;
        }

        // we get the sprite
        return ui_icons[index];
    }
    public Sprite GetModuleSprite(string module_reference)
    {
        // we check if the module exists
        if (!module_references.Contains(module_reference))
        {
            Debug.LogError("(ItemBank) cannot find module sprite for " + module_reference);
            return null;
        }
        int index = module_references.IndexOf(module_reference);

        if (index >= module_sprites.Count)
        {
            Debug.LogError("(ItemBank) Module sprites list is not initialized correctly, check the inspector");
            return null;
        }

        // we get the sprite
        return module_sprites[index];
    }

    // DEBUG
    private string getItemsList()
    {
        string title = "ITEMS in the bank - Total ";
        string list = "";
        int count = 0;
        foreach (KeyValuePair<string, Sprite> item in item_sprites)
        {
            list += item.Key + "\n";
            count++;
        }
        return title + count + " items\n" + list;
    }



    [Obsolete("Use GetSprite(string item_reference) instead.")]
    public Sprite getSprite(string item_ref) { return null;}
}