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
                // on ajoute le prefab des item
                item_prefabs.Add(prefab.name, path + "/" + prefab.name);

                // on ajoute le sprite de l'item
                Sprite sprite = prefab.GetComponent<SpriteRenderer>().sprite;
                item_sprites.Add(prefab.name, sprite);

                item_count++;

                if (debug) { Debug.Log("(ItemBank) loaded item : " + prefab.name); }
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
            SetUI_Item(ui_item.GetComponent<UI_Item>(), item);
        }
        else
        {
            ClearUI_Item(ui_item.GetComponent<UI_Item>());
        }

        if (debug) { Debug.Log("(ItemBank) created ui_item : " + item.name); }

        return ui_item;
    }

    public void SetUI_Item(UI_Item ui_item, Item item)
    {
        // on récupère le sprite de l'item
        Sprite sprite = getSprite(item.Reference);

        // on change le sprite de l'image
        Image img = ui_item.transform.Find("item").GetComponent<Image>();
        img.sprite = sprite;
        img.color = new Color(1, 1, 1, 1);

        // on calcule la taille de l'image
        RectTransform rt = img.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);

        // on change le nom du prefab
        ui_item.name = "ui_" + item.name;

        // on met la reference de l'item
        ui_item.item = item;

        // on enable le slot
        ui_item.Enable();

        if (debug) { Debug.Log("(ItemBank) set ui_item : " + item.name); }
    }
    public void ClearUI_Item(UI_Item ui_item)
    {
        // on change le sprite de l'image
        Image img = ui_item.transform.Find("item").GetComponent<Image>();
        img.sprite = null;
        img.color = new Color(0, 0, 0, 0);

        // on change le nom du prefab
        ui_item.name = "ui_empty";

        // on met la reference de l'item
        ui_item.item = null;

        // on disable le slot
        ui_item.Disable();

        if (debug) { Debug.Log("(ItemBank) cleared ui_item"); }
    }

    // GETTERS
    public Sprite getSprite(string item_name)
    {
        if (!item_sprites.ContainsKey(item_name))
        {
            Debug.LogError("(ItemBank) cannot find sprite " + item_name);
            return null;
        }

        return item_sprites[item_name];
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
}