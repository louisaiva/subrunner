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

    // LEGENDARY SPOTS
    private GameObject leg_items;
    protected Dictionary<string, Vector2> leg_item_positions = new Dictionary<string, Vector2>();
    protected Dictionary<string, Sprite> leg_item_slot_sprites = new Dictionary<string, Sprite>();

    // CURRENT ITEMS
    protected Dictionary<string, Item> items = new Dictionary<string, Item>();



    // unity functions
    void Start()
    {
        // on récupère le perso
        perso = GameObject.Find("/perso");

        // on récupère les items légendaires
        leg_items = transform.Find("leg_items").gameObject;

        // on récupère les positions des items légendaires
        leg_item_positions.Add("gyroscope", new Vector2(-34, 38));
        leg_item_positions.Add("noodle_os", new Vector2(-44, 0));
        leg_item_positions.Add("recorder", new Vector2(-34, -38));
        leg_item_positions.Add("glasses", new Vector2(34, 38));
        leg_item_positions.Add("weapon", new Vector2(44, 0));
        leg_item_positions.Add("shoes", new Vector2(34, -38));

        // on récupère les sprites des slots des items légendaires
        Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheets/ui/ui_inventory");
        leg_item_slot_sprites.Add("gyroscope", sprites[3]);
        leg_item_slot_sprites.Add("noodle_os", sprites[1]);
        leg_item_slot_sprites.Add("recorder", sprites[2]);
        leg_item_slot_sprites.Add("glasses", sprites[5]);
        leg_item_slot_sprites.Add("weapon", sprites[6]);
        leg_item_slot_sprites.Add("shoes", sprites[4]);
        
    }
    
    public void updateItems(List<Item> perso_items)
    {
        for (int i = 0; i < perso_items.Count; i++)
        {
            Item item = perso_items[i];
            if (item.legendary_item)
            {
                print("item : " + item.item_type + " is legendary");

                if (!items.ContainsKey(item.item_type))
                {
                    addLeg(item);
                }
            }
        }
    }

    // SHOWING
    private void show()
    {
        // on affiche l'inventaire
        is_showed = true;
        GetComponent<Image>().enabled = true;
        leg_items.SetActive(true);
    }

    private void hide()
    {
        // on cache l'inventaire
        is_showed = false;
        GetComponent<Image>().enabled = false;
        leg_items.SetActive(false);
    }

    public void rollShow()
    {
        // on affiche l'inventaire
        if (is_showed) { hide(); }
        else { show(); }
    }

    // LEGENDARY ITEMS
    private void addLeg(Item item)
    {

        string type = item.item_type;
        if (!leg_item_positions.ContainsKey(type)) { return; }

        // on récupère le slot de l'item
        GameObject slot = leg_items.transform.Find(type).gameObject;

        // on récupère le sprite du slot
        // Sprite slot_sprite = leg_item_slot_sprites[type];
        // slot.GetComponent<Image>().sprite = slot_sprite;

        // on récupère la position du slot
        // Vector2 item_pos = leg_item_positions[type];

        // on maj l'item
        // GameObject item_obj = slot.transform.Find("item").gameObject;
        // item_obj.GetComponent<RectTransform>().anchoredPosition = slot_pos;

        // on affiche l'item
        slot.SetActive(true);

        // on ajoute l'item
        items.Add(type, item);
    }

    private void delLeg(Item item)
    {
        string type = item.item_type;
        if (!items.ContainsKey(type)) { return; }

        // on récupère le slot de l'item
        GameObject slot = leg_items.transform.Find(type).gameObject;

        // on cache l'item
        slot.SetActive(false);

        // on supprime l'item
        items.Remove(type);
    }

}