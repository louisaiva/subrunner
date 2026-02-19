using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// This is the new version of UI_Item, which is obsolete now.
/// UI_ItemStack is the visual representation of ItemStack, and so is linked to an ItemStack from the construction of ItemStack to its death.
/// UI_ItemStack is created, handled and destroyed by UI_ItemPool which is the visual rep. of ItemPool (lol)
/// </summary>
public class UI_ItemStack : UI_ImageSlot, Droppable, Descriptable
{

    public Sprite drag_sprite;
    public Sprite drag_hover_sprite;
    protected Sprite current_item_sprite;

    [Header("Item Stack Data")]
    public ItemStack Stack;

    [Header("Descriptable")]
    public string Name => Stack.Name;
    public string Description => Stack.Description;

    [Header("Components")]
    protected TextMeshProUGUI quantity_text;
    protected Image item_image;



    // UI_ItemPool
    private UI_ItemPool _item_pool;
    public UI_ItemPool UI_ItemPool
    {
        get
        {
            if (_item_pool != null) { return _item_pool; }
            _item_pool = GetComponentInParent<UI_ItemPool>(includeInactive: true);
            return _item_pool;
        }
    }




    // AWAKE
    public virtual void Init(ItemStack stack)
    {
        // get some components
        item_image = transform.Find("item").GetComponent<Image>();
        quantity_text = transform.Find("qty").GetComponent<TextMeshProUGUI>();

        // attach the UI_ItemStack to its stack (register to its callbacks)
        this.Stack = stack;
        stack.OnUpdated += SyncUIWithStack;

        // sync the ui
        SyncUIWithStack();
    }
    private void OnDestroy()
    {
        if (Stack == null) { return; }
        Stack.OnUpdated -= SyncUIWithStack; // we unregister from the stack callbacks
    }


    // UPDATE UI
    public void SyncUIWithStack()
    {
        if (Stack == null) { return; }

        update_ui_item();
        update_ui_qty();
    }
    protected virtual void update_ui_item()
    {
        if (Stack.Quantity == 0) { clear_ui_item(); return; }

        // on charge le sprite de l'image
        current_item_sprite = ItemBank.Instance.GetSprite(Stack.Item);
        set_ui_item(current_item_sprite);

        // on change le nom du prefab
        name = "ui_" + Stack.Item.Reference;

        // on enable le slot
        Enable();
    }
    protected void update_ui_qty()
    {
        // we update the text
        quantity_text.text = Stack.Quantity.ToString();

        // we show or hide the text
        quantity_text.gameObject.SetActive(Stack.Quantity > 1);
    }
    protected virtual void set_ui_item(Sprite sprite)
    {
        item_image.sprite = sprite;
        item_image.color
                = sprite != null
                ? new Color(1, 1, 1, 1)
                : new Color(0, 0, 0, 0);
        if (sprite == null) { return; }

        // on calcule la taille de l'image
        RectTransform rt = item_image.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
    }
    protected void clear_ui_item()
    {
        // on change le sprite de l'image
        set_ui_item(null);
        current_item_sprite = null;

        // on change le nom du prefab
        name = "ui_empty";

        // on disable le slot vu qu'on a plus rien dedans
        if (UI_ItemPool != null && UI_ItemPool.DoNotDisableEmptySlots) { return; }
        Disable();
    }

    // DROPPABLE
    public void OnPointerDropped(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }


    // MOVE ITEM
    public virtual void OnPointerDragEnter(UI_Item moving_ui_item) { }

}