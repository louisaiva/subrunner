using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ChestInventory : MonoBehaviour, I_UI_Slottable
{

    // PERSO
    public GameObject perso;

    // AFFICHAGE
    [SerializeField] private bool is_showed = false;
    public UI_HooverDescriptionHandler description_ui;

    // ITEMS
    [SerializeField] private int max_items = 4;
    protected GameObject slot;
    protected Dictionary<Item, GameObject> item_ui = new Dictionary<Item, GameObject>();
    protected GameObject ui_item_prefab;
    public EnumItem bank = new EnumItem();


    [Header("Slottable")]
    private UI_XboxNavigator xbox_manager;
    private InputManager input_manager;
    // [SerializeField] private Vector2 base_position;
    [SerializeField] private float angle_threshold = 45f;
    [SerializeField] private float angle_multiplicator = 0f;


    // unity functions
    void Awake()
    {
        // on récupère le perso
        perso = GameObject.Find("/perso");

        // on récupère le description_ui
        description_ui = GameObject.Find("/ui/hoover_description").GetComponent<UI_HooverDescriptionHandler>();

        // on initialise les items
        bank.init(Resources.LoadAll<Sprite>("spritesheets/items"));

        // on récupère les slots des items
        slot = transform.Find("slot").gameObject;

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
        // on affiche l'inventaire
        is_showed = true;

        // on affiche les slots des items
        slot.SetActive(true);

        // on active le xbox_manager
        if (input_manager.isUsingGamepad()) { xbox_manager.enable(this); }
    }

    public void hide()
    {
        // on cache l'inventaire
        is_showed = false;

        // on reset le hoover de tous les slots
        resetAllSlotsHoover();

        // on cache les slots des items
        slot.SetActive(false);

        // on cache la description
        description_ui.removeAllDescriptions();

        // on désactive le xbox_manager
        xbox_manager.disable();

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


    // ITEMS
    public bool grabItem(Item item)
    {
        // on vérifie si on a déjà un item de ce type
        if (item_ui.ContainsKey(item)) { return false; }

        // on vérifie si on a de la place
        if (!canGrab()) { return false; }

        Debug.Log("ui_prefab : " + ui_item_prefab);
        Debug.Log("slot : " + slot);

        // on instancie un ui_item
        GameObject ui_item = Instantiate(ui_item_prefab, slot.transform);

        // on met à jour le sprite
        ui_item.transform.Find("item").GetComponent<Image>().sprite = bank.getSprite(item.item_name);

        // on met à jour ui_item
        ui_item.GetComponent<UI_Item>().setItem(item);
        ui_item.GetComponent<UI_Item>().setUIInventory(gameObject);

        // on ajoute l'item
        item_ui.Add(item, ui_item);

        // on met à jour le xbox_manager si on est show
        // if (is_showed) { xbox_manager.updateWhileShowed(); }

        return true;
    }

    public void dropItem(Item item)
    {
        // on vérifie si on a déjà un item de ce type
        if (!item_ui.ContainsKey(item)) { return; }

        // si on est un coffre, on drop l'item dans le perso
        perso.GetComponent<Perso>().grab(item);

        // on récupère le ui_item
        GameObject ui_item = item_ui[item];

        // on supprime l'item
        item_ui.Remove(item);
        ui_item.transform.SetParent(null);
        Destroy(ui_item);
    }


    public void randomize(string cat = "all")
    {
        // on ajoute des items random
        for (int i = 0; i < max_items; i++)
        {
            // on récupère un prefab random
            Item item = bank.getRandomItem(cat);

            Debug.Log("item : " + item.item_name);

            // on ajoute l'item
            grabItem(item);
        }
    }


    // HOOVER
    private void resetAllSlotsHoover()
    {
        // on reset le hoover de tous les slots
        foreach (KeyValuePair<Item, GameObject> entry in item_ui)
        {
            entry.Value.GetComponent<UI_Item>().resetHoover();
        }
    }


    // SLOTTABLE
    public List<GameObject> GetSlots(ref Vector2 base_position, ref float angle_threshold, ref float angle_multiplicator)
    {

        // on met à jour nos LayoutGroup
        LayoutRebuilder.ForceRebuildLayoutImmediate(slot.GetComponent<RectTransform>());



        // on récupère les slots
        List<GameObject> slots = new List<GameObject>();

        // on récupère les slots des items
        foreach (Transform child in slot.transform)
        {
            slots.Add(child.gameObject);
        }

        // print("nb slots : " + slots.Count);

        // on met à jour les seuils
        angle_threshold = this.angle_threshold;
        angle_multiplicator = this.angle_multiplicator;

        // on met à jour la position de base
        base_position = GetComponent<RectTransform>().TransformPoint(GetComponent<RectTransform>().rect.center);

        return slots;
    }
    public void clickOnItem(Item item)
    {
        dropItem(item);
    }



    // GETTERS
    public List<Item> getItems()
    {
        // on récupère les items
        return item_ui.Keys.ToList();
    }

    public bool canGrab()
    {
        // on vérifie si on a de la place
        return item_ui.Count < max_items;
    }


}