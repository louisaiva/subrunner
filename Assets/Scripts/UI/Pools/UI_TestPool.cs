using System.Collections.Generic;
using UnityEngine;

public class UI_TestPool : UI_Pool, I_UI_Slottable
{
    [Header("Slottable")]
    [SerializeField] private Transform slots_parent;
    [SerializeField] private Vector2 base_position = new Vector2(0, 10000);

    [Header("Components")]
    [SerializeField] private UI_XboxNavigator xbox_manager;

    // unity functions
    protected void Awake()
    {
        // on récupère le xbox_manager
        xbox_manager = GameObject.Find("/ui").GetComponent<UI_XboxNavigator>();
    }

    // SHOWING
    public override async Awaitable Show(float duration)
    {
        await base.Show(duration);

        // on active le xbox_manager
        xbox_manager.Enable(this);
    }
    public override async Awaitable Hide(float duration)
    {

        // on désactive le xbox_manager
        xbox_manager.Disable(this);

        await base.Hide(duration);
    }

    // SLOTTABLE
    public List<GameObject> GetSlots(ref Vector2 base_position, ref float angle_threshold, ref float angle_multiplicator)
    {
        // on récupère les slots
        List<GameObject> slots = new List<GameObject>();

        // on récupère les slots
        foreach (Transform slot in slots_parent)
        {
            if (slot.gameObject.GetComponent<UI_Slot>() != null && slot.gameObject.activeSelf)
            {
                slots.Add(slot.gameObject);
            }
        }

        // on met à jour les seuils
        angle_threshold = base.angle_threshold;
        angle_multiplicator = base.angle_multiplicator;

        // on met à jour la position de base
        base_position = this.base_position;

        return slots;
    }
    public bool IsYourSlot(GameObject slot)
    {
        // on regarde si le slot est dans les slots
        if (slot.transform.IsChildOf(slots_parent))
        {
            return true;
        }
        return false;
    }
}