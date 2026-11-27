using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseAttackDirectionIndicator : MonoBehaviour
{
    public bool activated = false;
    public FeedbackSwitcher input_switcher;
    public Inventory inventory = null;
    private ItemPool WeaponPool => inventory?.GetItemPool("weapon");
    public Vector2 look_at;

    [Header("Logs")]
    [SerializeField] private bool log = false;
    [SerializeField] private bool log_callbacks = false;

    internal void OnCapableControlled(Capable controlled_capable)
    {
        if (inventory != null) { OnCapableUncontrolled(null); }
        inventory = controlled_capable.Inventory;
        if (inventory == null) { disable(); return; }

        inventory.OnItemGrabbed += on_inventory_changed;
        inventory.OnItemDropped += on_inventory_changed;
        if (log_callbacks) { Debug.Log($"(MADI) Setup callbacks for pool {inventory.name} : {inventory.GetInstanceID()}"); }

        if (WeaponPool?.GetItemsByType<Weapon>().FirstOrDefault() != null) { activate(); }
        else { disable(); }
    }
    internal void OnCapableUncontrolled(Capable controlled_capable)
    {
        if (inventory != null)
        {
            inventory.OnItemGrabbed -= on_inventory_changed;
            inventory.OnItemDropped -= on_inventory_changed;
            if (log_callbacks) { Debug.Log($"(MADI) Removed callbacks for pool {inventory.name} : {inventory.GetInstanceID()}"); }
        }
        inventory = null;
        disable();
    }

    // callbacks
    internal void on_inventory_changed(Item item)
    {
        if (WeaponPool?.GetItemsByType<Weapon>().FirstOrDefault() == null) { disable(); return; }
        activate();
    }

    // activate / disable
    private void activate()
    {
        activated = true;
        input_switcher.enabled = true;
        if (log) { Debug.Log("(MADI) activating madi"); }
        // here we don't input_switcher.keyboardFeedback.SetActive(true); because this depends on other parameters, if the input is on keyboard + potential setting is true
    }
    private void disable()
    {
        activated = false;
        input_switcher.enabled = false;
        input_switcher.keyboardFeedback.SetActive(false);
        if (log) { Debug.Log("(MADI) disabling madi"); }
    }

    private void Update()
    {
        if (!activated) { return; }

        // we gather the vector between the mouse and the controller
        Vector3 mousePos = Mouse.current.position.ReadValue();
        Vector2 world_mouse = Camera.main.ScreenToWorldPoint(mousePos);
        // Debug.Log($"(MADI) world_mouse is {world_mouse}");

        // we update our transform rotation angle based on the direction.normalized
        look_at = (world_mouse-(Vector2)transform.position).normalized;
        // Debug.Log($"(MADI) direction is {direction}");
        // transform.rotation = Quaternion.LookRotation(direction);
        // Draw a ray pointing at our target in
        // Debug.DrawRay(transform.position, look_at, Color.red);

        // we get the angle from the normalized vector (between 0 & 360f)
        float angle = Mathf.Atan2(look_at.y, look_at.x) * Mathf.Rad2Deg;
        // Debug.Log($"(MADI) new angle is {angle}");
        transform.rotation = Quaternion.Euler(0f,0f,angle);
    }
}