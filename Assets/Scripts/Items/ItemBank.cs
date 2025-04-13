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

    [Header("UI")]
    public GameObject ui_item_prefab;

    [Header("Debug")]
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
    public GameObject CreateUI_Item(Item item = null)
    {
        // on instancie le prefab
        GameObject ui_item = Instantiate(ui_item_prefab, Vector3.zero, Quaternion.identity);

        // we assign the item to the UI_Item
        if (item != null)
        {
            ui_item.GetComponent<UI_Item>().Store(item);
        }
        else { ui_item.GetComponent<UI_Item>().ClearUI(); }

        if (debug) { Debug.Log("(ItemBank) created ui_item : " + item.name); }

        return ui_item;
    }

    // GETTERS
    public Sprite GetSprite(string item_reference)
    {
        if (!item_sprites.ContainsKey(item_reference))
        {
            Debug.LogError("(ItemBank) cannot find sprite " + item_reference);
            return null;
        }

        return item_sprites[item_reference];
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



    // ! DEPRECATED
    public Sprite getSprite(string item_ref) { return null;}
}