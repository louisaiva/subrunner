using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Inventory : MonoBehaviour
{

    // PERSO
    public GameObject perso;

    // AFFICHAGE
    [SerializeField] private bool is_showed = false;
    private GameObject ui_bg;

    // DESCRIPTION
    public UI_HooverDescriptionHandler description_ui;

    // ITEMS
    public EnumItem bank = new EnumItem();

    // LEGENDARY SPOTS
    private GameObject leg_slots;
    protected GameObject ui_leg_item_prefab;

    // CURRENT ITEMS
    protected Dictionary<string, Item> leg_items = new Dictionary<string, Item>();
    // protected Dictionary<string,List<Item>> items = new Dictionary<string, List<Item>>();

    // ITEMS SLOTS
    protected Dictionary<string, GameObject> item_slots = new Dictionary<string, GameObject>();
    protected Dictionary<string,bool> item_slots_showed = new Dictionary<string, bool>();

    protected Dictionary<Item, GameObject> item_ui = new Dictionary<Item, GameObject>();
    protected GameObject ui_item_prefab;


    // unity functions
    void Start()
    {
        // on récupère le perso
        perso = GameObject.Find("/perso");

        // on récupère le ui_bg
        ui_bg = transform.Find("bg").gameObject;

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

        // on cache l'inventaire
        hide();        
    }
    
    // SHOWING
    private void show()
    {
        // on affiche l'inventaire
        is_showed = true;
        GetComponent<Image>().enabled = true;

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
    }


    private void hide()
    {
        // on cache l'inventaire
        is_showed = false;
        GetComponent<Image>().enabled = false;

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
    }

    public void rollShow()
    {
        // on affiche l'inventaire
        if (is_showed) { hide(); }
        else { show(); }
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
        else if (leg_items[item.item_type].capacity_level < item.capacity_level)
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
                if (same_type_item.capacity_level > best_item.capacity_level)
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
        // if (!leg_item_positions.ContainsKey(type)) { return; }

        print("on ajoute un item légendaire : " + item.item_type + " -> " + item.item_name);

        // on met à jour le sprite de l'item
        setSpriteToLegSlot(type, item);

        // on affiche le slot
        GameObject slot = leg_slots.transform.Find(type).gameObject;
        slot.SetActive(true);

        // on ajoute l'item
        leg_items.Add(type, item);
    }

    private void delLeg(Item item)
    {
        string type = item.item_type;
        if (!leg_items.ContainsKey(type)) { return; }

        print("on supprime un item légendaire : " + item.item_type + " -> " + item.item_name);

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

        print("on remplace un item légendaire : " + item.item_type + " -> " + item.item_name);

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
        item_obj.GetComponent<Image>().sprite = bank.getSprite(item.item_name);

        // on maj l'ui_item
        item_obj.GetComponent<UI_Item>().setItem(item);
        item_obj.GetComponent<UI_Item>().setUIInventory(gameObject);
    }


    // ITEMS
    /* private void addItem(Item item)
    {
        string type = item.item_type;
        if (!items.ContainsKey(type))
        {
            // on ajoute le type
            items.Add(type, new List<Item>());
        }

        print("on ajoute un item : " + item.item_type + " -> " + item.item_name);

        // on ajoute l'item
        items[type].Add(item);
    }

    private void delItem(Item item)
    {
        string type = item.item_type;
        if (!items.ContainsKey(type)) { return; }

        print("on supprime un item : " + item.item_type + " -> " + item.item_name);

        // on supprime l'item
        items[type].Remove(item);
    } */

    public void grabItem(Item item)
    {
        print("on essaie de grab un item : " + item.item_type + " -> " + item.item_name);
        // on vérifie si on a déjà un item de ce type
        string slot = item.item_type;
        if (!item_slots.ContainsKey(slot)) { return; }

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

        // on supprime le ui_item
        Destroy(ui_item);

        // on cache le slot
        if (nb_items == 1)
        {
            // on cache le slot
            item_slots_showed[item.item_type] = false;
            ui_item.transform.parent.parent.gameObject.SetActive(false);
        }
    }


    // CLICKS
    public void clickOnItem(Item item)
    {
        perso.GetComponent<Perso>().inventory.dropItem(item);
    }

}

public class EnumItem
{

    public Dictionary<string, Sprite> item_sprites = new Dictionary<string, Sprite>();
    public Dictionary<string, string> item_prefabs = new Dictionary<string, string>();

    // constructor
    public EnumItem()
    {

        // on ajoute les prefabs des items
        item_prefabs.Add("holy_water", "prefabs/items/holy_water");
        item_prefabs.Add("heal_potion", "prefabs/items/heal_potion");
        item_prefabs.Add("magic_water", "prefabs/items/magic_water");
        item_prefabs.Add("usb_key", "prefabs/items/usb_key");
        item_prefabs.Add("gv_glasses", "prefabs/items/legendary/gv_glasses");
        item_prefabs.Add("speed_glasses", "prefabs/items/legendary/speed_glasses");
        item_prefabs.Add("3d_glasses", "prefabs/items/legendary/3d_glasses");
        item_prefabs.Add("dynamite", "prefabs/items/dynamite");
        item_prefabs.Add("tape_recorder", "prefabs/items/legendary/tape_recorder");
        item_prefabs.Add("helium_shoes", "prefabs/items/legendary/helium_shoes");
        item_prefabs.Add("carbon_shoes", "prefabs/items/legendary/carbon_shoes");
        item_prefabs.Add("orangina", "prefabs/items/orangina");
        item_prefabs.Add("noodle_os", "prefabs/items/legendary/noodle_os");
        item_prefabs.Add("gyroscope", "prefabs/items/legendary/gyroscope");
        item_prefabs.Add("door_hack", "prefabs/items/door_hack");
        item_prefabs.Add("computer_hack", "prefabs/items/computer_hack");
        item_prefabs.Add("subway_hack", "prefabs/items/subway_hack");
        item_prefabs.Add("firewall_hack", "prefabs/items/firewall_hack");
        item_prefabs.Add("zombo_damage", "prefabs/items/zombo_damage");
        item_prefabs.Add("zombo_explosion", "prefabs/items/zombo_explosion");
        item_prefabs.Add("zombo_electrochoc", "prefabs/items/zombo_electrochoc");
        item_prefabs.Add("zombo_control", "prefabs/items/zombo_control");
        item_prefabs.Add("light_hack", "prefabs/items/light_hack");
        item_prefabs.Add("tv_hack", "prefabs/items/tv_hack");
        item_prefabs.Add("katana", "prefabs/items/legendary/katana");

    }

    public void init(Sprite[] sprites)
    {
        // on ajoute les sprites des items
        item_sprites.Add("holy_water", sprites[0]);
        item_sprites.Add("heal_potion", sprites[1]);
        item_sprites.Add("magic_water", sprites[2]);
        item_sprites.Add("usb_key", sprites[3]);
        item_sprites.Add("gv_glasses", sprites[4]);
        item_sprites.Add("speed_glasses", sprites[5]);
        item_sprites.Add("3d_glasses", sprites[6]);
        item_sprites.Add("dynamite", sprites[7]);
        item_sprites.Add("tape_recorder", sprites[8]);
        item_sprites.Add("helium_shoes", sprites[9]);
        item_sprites.Add("carbon_shoes", sprites[10]);
        item_sprites.Add("orangina", sprites[11]);
        item_sprites.Add("noodle_os", sprites[12]);
        item_sprites.Add("gyroscope", sprites[13]);
        item_sprites.Add("door_hack", sprites[14]);
        item_sprites.Add("computer_hack", sprites[15]);
        item_sprites.Add("subway_hack", sprites[16]);
        item_sprites.Add("firewall_hack", sprites[17]);
        item_sprites.Add("zombo_damage", sprites[18]);
        item_sprites.Add("zombo_explosion", sprites[19]);
        item_sprites.Add("zombo_electrochoc", sprites[20]);
        item_sprites.Add("zombo_control", sprites[21]);
        item_sprites.Add("light_hack", sprites[22]);
        item_sprites.Add("tv_hack", sprites[23]);
        item_sprites.Add("katana", sprites[24]);
    }


    // GETTERS
    public Sprite getSprite(string item_name)
    {
        if (!item_sprites.ContainsKey(item_name))
        {
            Debug.LogWarning("(EnumItem) cannot find sprite " + item_name);
            return null;
        }

        return item_sprites[item_name];
    }
}