using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Inventory : MonoBehaviour, I_UI_Slottable
{

    // PERSO
    public GameObject perso;

    // AFFICHAGE
    [SerializeField] private bool is_showed = false;
    private GameObject ui_bg;
    private GameObject hc;
    private UI_MainUI main_ui;

    // DESCRIPTION
    public UI_HooverDescriptionHandler description_ui;

    // ITEMS
    public ItemBank bank;

    // LEGENDARY SPOTS
    private GameObject leg_slots;
    protected GameObject ui_leg_item_prefab;

    // CURRENT ITEMS
    protected Dictionary<string, Item> leg_items = new Dictionary<string, Item>();

    // ITEMS SLOTS
    protected Dictionary<string, GameObject> item_slots = new Dictionary<string, GameObject>();
    protected Dictionary<string,bool> item_slots_showed = new Dictionary<string, bool>();

    protected Dictionary<Item, GameObject> item_ui = new Dictionary<Item, GameObject>();
    protected GameObject ui_item_prefab;

    [Header("Inputs")]
    [SerializeField] private UI_XboxNavigator xbox_manager;
    [SerializeField] private InputManager input_manager;

    // unity functions
    void Start()
    {
        bank = GameObject.Find("/utils/bank").GetComponent<ItemBank>();
        
        // on récupère le perso
        perso = GameObject.Find("/perso");

        // on récupère le ui_bg
        ui_bg = transform.parent.Find("bg").gameObject;
        hc = transform.parent.Find("hc").gameObject;

        // on récupère le main_ui
        main_ui = GameObject.Find("/ui").GetComponent<UI_MainUI>();

        // on récupère le description_ui
        description_ui = GameObject.Find("/ui/hoover_description").GetComponent<UI_HooverDescriptionHandler>();

        // on initialise les items
        bank.init(Resources.LoadAll<Sprite>("spritesheets/items"));

        // on récupère les slots des items légendaires
        leg_slots = transform.Find("leg_slots").gameObject;

        // on récupère le prefab des items légendaires
        ui_leg_item_prefab = Resources.Load("prefabs/ui/ui_leg_item") as GameObject;

        // on récupère les slots des items
        item_slots.Add("hack", transform.Find("hack_slots").Find("slots").gameObject);
        item_slots.Add("item", transform.Find("item_slots").Find("slots").gameObject);
        item_slots_showed.Add("hack", false);
        item_slots_showed.Add("item", false);

        // on récupère le prefab des items
        ui_item_prefab = Resources.Load("prefabs/ui/ui_item") as GameObject;

        // on récupère le xbox_manager
        xbox_manager = GameObject.Find("/ui").GetComponent<UI_XboxNavigator>();

        // on récupère l'input_manager
        input_manager = GameObject.Find("/utils/input_manager").GetComponent<InputManager>();

        // on cache l'inventaire
        hide();
    }
    
    // SHOWING
    public void show()
    {
        // on cache le main_ui
        main_ui.show();
        main_ui.showOnly(transform.parent.gameObject);

        // on affiche l'inventaire
        is_showed = true;

        // on affiche les slots des légendaires
        leg_slots.SetActive(true);

        // on affiche les slots des items
        foreach (KeyValuePair<string, GameObject> entry in item_slots)
        {
            if (!item_slots_showed[entry.Key]) { continue; }
            entry.Value.transform.parent.gameObject.SetActive(true);
        }

        // on affiche le bg
        ui_bg.SetActive(true);
        hc.SetActive(true);

        // on affiche le texte au bon emplacement
        // Vector3 text_position = new Vector3(Screen.width/2f, Screen.height, 0f);
        transform.parent.Find("text").gameObject.SetActive(true);
        // transform.Find("text").GetComponent<RectTransform>().position = text_position;

        // on active le xbox_manager
        if (input_manager.isUsingGamepad()) { xbox_manager.enable(this); }
    }

    public void hide()
    {
        // on cache l'inventaire
        is_showed = false;
        
        // on reset le hoover de tous les slots
        resetAllSlotsHoover();

        // on cache les slots des légendaires
        leg_slots.SetActive(false);

        // on cache les slots des items
        foreach (KeyValuePair<string, GameObject> entry in item_slots)
        {
            entry.Value.transform.parent.gameObject.SetActive(false);
        }

        // on cache la description
        description_ui.removeAllDescriptions();

        // on cache le bg
        ui_bg.SetActive(false);
        hc.SetActive(false);

        // on cache le texte
        transform.parent.Find("text").gameObject.SetActive(false);

        // on désactive le xbox_manager
        xbox_manager.disable();

        // on affiche le main_ui
        main_ui.show();
    }

    public void rollShow()
    {
        // on affiche l'inventaire
        if (is_showed) { hide(); }
        else { show(); }
    }

    public bool isShowed()
    {
        return is_showed;
    }

    // LEGENDARY ITEMS
    public void grabLeg(Item item)
    {
        // on vérifie si on a déjà un item de ce type
        if (!leg_items.ContainsKey(item.item_type))
        {
            // on ajoute l'item
            addLeg(item);
        }
        else if (leg_items[item.item_type].level < item.level)
        {
            // on remplace l'item
            replaceLeg(item, item.item_type);
        }
    }

    public void dropLeg(Item item)
    {
        // on regarde dans l'inventaire du perso si on a un item de ce type

        // on récupère les items légendaires du perso
        List<Item> perso_leg_items = perso.GetComponent<Perso>().inventory.getLegendaryItems();
        List<Item> same_type_items = perso_leg_items.FindAll(x => x.item_type == item.item_type);

        if (same_type_items.Count > 0)
        {
            // on récupère le meilleur item
            Item best_item = same_type_items[0];
            foreach (Item same_type_item in same_type_items)
            {
                if (same_type_item.level > best_item.level)
                {
                    best_item = same_type_item;
                }
            }

            // on remplace l'item
            replaceLeg(best_item, item.item_type);
        }
        else
        {
            // on supprime l'item
            delLeg(item);
        }
    }

    private void addLeg(Item item)
    {
        string type = item.item_type;

        // on met à jour le sprite de l'item
        setSpriteToLegSlot(type, item);

        // on affiche le slot
        GameObject slot = leg_slots.transform.Find(type).gameObject;
        slot.SetActive(true);

        // on ajoute l'item
        leg_items.Add(type, item);

        // on met à jour le xbox_manager si on est show
        if (is_showed) { xbox_manager.updateWhileShowed(); }
    }

    private void delLeg(Item item)
    {
        string type = item.item_type;
        if (!leg_items.ContainsKey(type)) { return; }

        // on récupère le slot de l'item
        GameObject slot = leg_slots.transform.Find(type).gameObject;

        // on cache l'item
        slot.SetActive(false);

        // on supprime l'item
        leg_items.Remove(type);
    }

    private void replaceLeg(Item item, string slot)
    {
        // on supprime l'ancien item
        if (!leg_items.ContainsKey(slot)) { return; }

        // on met à jour l'ui
        setSpriteToLegSlot(slot, item);

        // on remplace l'item
        leg_items[slot] = item;
    }

    private void setSpriteToLegSlot(string slot,Item item)
    {
        // on récupère le slot de l'item
        GameObject item_obj = leg_slots.transform.Find(slot).Find("ui_leg_item").gameObject;

        // on maj le sprite
        item_obj.transform.Find("item").GetComponent<Image>().sprite = bank.getSprite(item.item_name);

        // on maj l'ui_item
        item_obj.GetComponent<UI_Item>().setItem(item);
        item_obj.GetComponent<UI_Item>().setUIInventory(gameObject);
    }


    // ITEMS
    public void grabItem(Item item)
    {
        // on vérifie si on a déjà un item de ce type
        string slot = item.item_type;
        if (!item_slots.ContainsKey(slot)) { return; }
        if (item_ui.ContainsKey(item)) { return; }

        // on récupère le slot
        GameObject slot_obj = item_slots[slot];

        // on instancie un ui_item
        GameObject ui_item = Instantiate(ui_item_prefab, slot_obj.transform);

        // on met à jour le sprite
        ui_item.transform.Find("item").GetComponent<Image>().sprite = bank.getSprite(item.item_name);

        // on met à jour ui_item
        ui_item.GetComponent<UI_Item>().setItem(item);
        ui_item.GetComponent<UI_Item>().setUIInventory(gameObject);

        // on ajoute l'item
        item_ui.Add(item, ui_item);

        // on affiche le slot
        item_slots_showed[slot] = true;
        slot_obj.transform.parent.gameObject.SetActive(is_showed);

        // on met à jour le xbox_manager si on est show
        if (is_showed) { xbox_manager.updateWhileShowed(); }
    }

    public void dropItem(Item item)
    {
        // on vérifie si on a déjà un item de ce type
        if (!item_ui.ContainsKey(item)) { return; }

        // on récupère le ui_item
        GameObject ui_item = item_ui[item];

        // on supprime l'item
        item_ui.Remove(item);

        // on vérifie le nombre d'items restants
        int nb_items = ui_item.transform.parent.childCount;

        // on cache le slot
        if (nb_items == 1)
        {
            // on cache le slot
            item_slots_showed[item.item_type] = false;
            ui_item.transform.parent.parent.gameObject.SetActive(false);
        }

        // on supprime le ui_item
        ui_item.transform.SetParent(null);
        Destroy(ui_item);
    }


    // HOVER
    private void resetAllSlotsHoover()
    {
        // on reset le hoover de tous les slots
        foreach (KeyValuePair<Item, GameObject> entry in item_ui)
        {
            entry.Value.GetComponent<UI_Item>().resetHoover();
        }

        // on reset le hoover des slots légendaires
        foreach (KeyValuePair<string, Item> entry in leg_items)
        {
            GameObject slot = leg_slots.transform.Find(entry.Key).Find("ui_leg_item").gameObject;
            slot.GetComponent<UI_Item>().resetHoover();
        }
    }

    // CLICKS
    public void clickOnItem(Item item)
    {
        perso.GetComponent<Perso>().inventory.dropItem(item);
    }

    // SLOTTABLE
    public List<GameObject> GetSlots(ref Vector2 base_position, ref float angle_threshold, ref float angle_multiplicator)
    {

        // on met à jour nos LayoutGroup
        foreach (KeyValuePair<string, GameObject> entry in item_slots)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(entry.Value.GetComponent<RectTransform>());
        }



        // on récupère les slots
        List<GameObject> slots = new List<GameObject>();

        // on récupère les slots des items légendaires
        foreach (KeyValuePair<string, Item> entry in leg_items)
        {
            GameObject slot = leg_slots.transform.Find(entry.Key).Find("ui_leg_item").gameObject;
            slots.Add(slot);
        }

        // on récupère les slots des items
        foreach (KeyValuePair<string, GameObject> entry in item_slots)
        {
            if (!item_slots_showed[entry.Key]) { continue; }
            foreach (Transform child in entry.Value.transform)
            {
                slots.Add(child.gameObject);
            }
        }

        // print("nb slots : " + slots.Count);

        // on met à jour les seuils
        angle_threshold = 45f;
        angle_multiplicator = 100f;

        // on met à jour la position de base
        base_position = GetComponent<RectTransform>().TransformPoint(GetComponent<RectTransform>().rect.center);

        return slots;
    }

}
