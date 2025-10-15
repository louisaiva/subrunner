#pragma warning disable 4014
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [Header("Special items slots")]
    [SerializeField] private UI_LaptopItemSlot laptop_slot;
    [SerializeField] private UI_Item weapon_slot;
    [SerializeField] private UI_ItemPool shoes_pool;
    [SerializeField] private UI_Item cons1_slot;
    [SerializeField] private UI_Item cons2_slot;
    [SerializeField] private UI_Item cons3_slot;
    [SerializeField] private UI_Item cons4_slot;

    [Header("HUD Renderers")]
    [SerializeField] private RectTransform laptop_renderer;
    [SerializeField] private RectTransform hack_renderer;
    [SerializeField] private RectTransform weapon_renderer;
    [SerializeField] private UI_ItemRenderer shoes_renderer;
    [SerializeField] private RectTransform cons1_renderer;
    [SerializeField] private RectTransform cons2_renderer;
    [SerializeField] private RectTransform cons3_renderer;
    [SerializeField] private RectTransform cons4_renderer;

    [Header("Log")]
    [SerializeField] private bool log = false;
    [SerializeField] private bool log_shoes = false;

    // START
    private void Start()
    {
        // checks if we have all variables set
        if (laptop_slot == null || weapon_slot == null || cons1_slot == null || cons2_slot == null || cons3_slot == null || cons4_slot == null)
        {
            Debug.LogError("(ItemManager) One or more item slots are not set.");
            return;
        }
        else if (weapon_renderer == null || cons1_renderer == null || cons2_renderer == null || cons3_renderer == null || cons4_renderer == null)
        {
            Debug.LogError("(ItemManager) One or more HUD renderers are not set.");
            return;
        }

        // we subscribe to events
        laptop_slot.OnItemChanged += update_hud_renderers;
        weapon_slot.OnItemChanged += update_hud_renderers;
        cons1_slot.OnItemChanged += update_hud_renderers;
        cons2_slot.OnItemChanged += update_hud_renderers;
        cons3_slot.OnItemChanged += update_hud_renderers;
        cons4_slot.OnItemChanged += update_hud_renderers;

        // shoes events
        shoes_pool.OnPoolChanged += (ui_item) =>
        {
            if (log_shoes) { Debug.Log("(ItemManager) Shoes pool changed : " + ui_item.name + " ui_item has " + ui_item.GetItems().Count + " items."); }
            update_shoes();
            update_hud_renderers();
        };

        // we update the HUD renderers
        update_hud_renderers();
    }

    // ITEM GETTERS
    /* public Laptop GetLaptop()
    {
        if (laptop_slot == null || !laptop_slot.HasLaptop)
        {
            if (log) { Debug.LogWarning("(ItemManager) No laptop found in the laptop slot."); }
            return null;
        }
        return laptop_slot.Laptop;
    } */
    public Weapon GetWeapon()
    {
        if (weapon_slot == null || weapon_slot.Item == null || !(weapon_slot.Item is Weapon))
        {
            if (log) { Debug.LogWarning("(ItemManager) No weapon found in the weapon slot."); }
            return null;
        }
        return weapon_slot.Item as Weapon;
    }
    public Usable GetConsumable(int index)
    {
        UI_Item slot = null;
        switch (index)
        {
            case 1: slot = cons1_slot; break;
            case 2: slot = cons2_slot; break;
            case 3: slot = cons3_slot; break;
            case 4: slot = cons4_slot; break;
            default:
                if (log) { Debug.LogWarning("(ItemManager) Invalid consumable index: " + index); }
                return null;
        }

        if (slot == null || slot.Item == null)
        {
            if (log) { Debug.LogWarning("(ItemManager) No consumable found in the consumable slot " + index); }
            return null;
        }
        if (slot.Item is not Usable usable)
        {
            if (log) { Debug.LogWarning("(ItemManager) Item in consumable slot " + index + " is not usable."); }
            return null;
        }
        return usable;
    }


    // SHOES GETTERS
    public Shoes GetShoes()
    {
        if (shoes_pool == null) { return null; }

        List<Item> items = shoes_pool.GetAllItems();
        foreach (var item in items)
        {
            if (item != null && item is Shoes) { return item as Shoes; }
        }
        return null;
    }
    public UI_Item GetShoesUI_Item()
    {
        if (shoes_pool == null) { return null; }

        foreach (Transform child in shoes_pool.transform)
        {
            // we check if the ui_slot is enabled
            if (!child.gameObject.activeSelf) { continue; }

            // we check if the slot is a UI_Item
            UI_Item ui_item = child.GetComponent<UI_Item>();
            if (ui_item == null) { continue; }

            if (ui_item.Item != null && ui_item.Item is Shoes) { return ui_item; }
        }
        return null;
    }


    // UPDATE HUDs
    private void update_shoes()
    {
        shoes_renderer.SetTarget(GetShoesUI_Item());
        if (log_shoes) { Debug.Log("(ItemManager) Shoes renderer target set to " + (shoes_renderer.Target != null ? shoes_renderer.Target.name : "null")); }
    }
    private void update_hud_renderers(List<Item> items = null)
    {
        // update LAPTOP
        if (laptop_slot.Item != null)
        {
            enable_renderer(laptop_renderer);
            enable_renderer(hack_renderer);
        }
        else
        {
            disable_renderer(laptop_renderer);
            disable_renderer(hack_renderer);
        }

        // initialize position
        float x = 0f;
        float cons_size = 37.5f; // size of the consumable icons

        // update WEAPON
        if (weapon_slot.Item != null)
        {
            enable_renderer(weapon_renderer, x);
            x += 50f;
        }
        else { disable_renderer(weapon_renderer); }

        // update SHOES
        if (shoes_renderer != null && shoes_renderer.Target != null && shoes_renderer.Target.Item != null)
        {
            enable_renderer(shoes_renderer.GetComponent<RectTransform>(), x);
            x += 50f;
        }
        else { disable_renderer(shoes_renderer.GetComponent<RectTransform>()); }

        // update CONS
        for (int i = 1; i <= 4; i++)
        {
            UI_Item slot = null;
            RectTransform renderer = null;
            switch (i)
            {
                case 1:
                    slot = cons1_slot;
                    renderer = cons1_renderer;
                    break;
                case 2:
                    slot = cons2_slot;
                    renderer = cons2_renderer;
                    break;
                case 3:
                    slot = cons3_slot;
                    renderer = cons3_renderer;
                    break;
                case 4:
                    slot = cons4_slot;
                    renderer = cons4_renderer;
                    break;
            }
            if (slot.Item != null)
            {
                enable_renderer(renderer, x);
                // renderer.anchoredPosition = new Vector2(x, renderer.anchoredPosition.y);
                x += cons_size;
            }
            else { disable_renderer(renderer); }
        }

    }

    // RENDERER HELPERS
    private void disable_renderer(RectTransform renderer)
    {
        // renderer.gameObject.SetActive(false);
        Transitioner transitioner = renderer.GetComponent<Transitioner>();
        transitioner.Hide();
    }
    private void enable_renderer(RectTransform renderer, float x = float.NaN)
    {
        if (!float.IsNaN(x)) { renderer.anchoredPosition = new Vector2(x, renderer.anchoredPosition.y); }
        Transitioner transitioner = renderer.GetComponent<Transitioner>();
        transitioner.Show();
    }
}