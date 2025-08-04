using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [Header("Special items slots")]
    [SerializeField] private UI_LaptopItemSlot laptop_slot;
    [SerializeField] private UI_Item weapon_slot;
    [SerializeField] private UI_Item cons1_slot;
    [SerializeField] private UI_Item cons2_slot;
    [SerializeField] private UI_Item cons3_slot;
    [SerializeField] private UI_Item cons4_slot;

    [Header("HUD Renderers")]
    [SerializeField] private RectTransform weapon_renderer;
    // [SerializeField] private RectTransform laptop_renderer;
    [SerializeField] private RectTransform cons1_renderer;
    [SerializeField] private RectTransform cons2_renderer;
    [SerializeField] private RectTransform cons3_renderer;
    [SerializeField] private RectTransform cons4_renderer;

    [Header("Log")]
    [SerializeField] private bool log = false;

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

        // we update the HUD renderers
        update_hud_renderers();
    }

    // ITEM GETTERS
    public Laptop GetLaptop()
    {
        if (laptop_slot == null || !laptop_slot.HasLaptop)
        {
            if (log) { Debug.LogWarning("(ItemManager) No laptop found in the laptop slot."); }
            return null;
        }
        return laptop_slot.Laptop;
    }
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


    // UPDATE HUDs
    private void update_hud_renderers(List<Item> items = null)
    {
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

        /* // update CONS 1
            if (cons1_slot.Item != null)
            {
                enable_renderer(cons1_renderer, x);
                // cons1_renderer.anchoredPosition = new Vector2(x, cons1_renderer.anchoredPosition.y);
                x += cons_size;
            }
            else { disable_renderer(cons1_renderer); }

        // update CONS 2
        if (cons2_slot.Item != null)
        {
            enable_renderer(cons2_renderer, x);
            // cons2_renderer.anchoredPosition = new Vector2(x, cons2_renderer.anchoredPosition.y);
            x += cons_size;
        }
        else { disable_renderer(cons2_renderer); }

        // update CONS 3
        if (cons3_slot.Item != null)
        {
            enable_renderer(cons3_renderer, x);
            // cons3_renderer.anchoredPosition = new Vector2(x, cons3_renderer.anchoredPosition.y);
            x += cons_size;
        }
        else { disable_renderer(cons3_renderer); }

        // update CONS 4
        if (cons4_slot.Item != null)
        {
            enable_renderer(cons4_renderer, x);
            // cons4_renderer.anchoredPosition = new Vector2(x, cons4_renderer.anchoredPosition.y);
            x += cons_size;
        }
        else { disable_renderer(cons4_renderer); } */
    }

    private void disable_renderer(RectTransform renderer)
    {
        // renderer.gameObject.SetActive(false);
        Transitioner transitioner = renderer.GetComponent<Transitioner>();
        transitioner.Hide();        
    }
    private void enable_renderer(RectTransform renderer, float x)
    {
        renderer.anchoredPosition = new Vector2(x, renderer.anchoredPosition.y);
        // renderer.gameObject.SetActive(true);
        Transitioner transitioner = renderer.GetComponent<Transitioner>();
        transitioner.Show();
    }
}