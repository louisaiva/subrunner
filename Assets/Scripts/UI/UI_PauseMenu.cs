using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PauseMenu : UI_Pool, I_UI_Slottable
{
    [Header("Slottable")]
    [SerializeField] private Transform slots_parent;
    [SerializeField] private Vector2 base_position = new Vector2(0, 10000);

    [Header("Components")]
    [SerializeField] private UI_XboxNavigator xbox_manager;

    // AWAKE
    protected override void Awake()
    {
        base.Awake();
        // on récupère le xbox_manager
        xbox_manager = GameObject.Find("/ui").GetComponent<UI_XboxNavigator>();
    }


    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        // on active le navigator
        UI_XboxNavigator.Instance.Enable(this);
        yield break;
    }
    protected override IEnumerator disable_coroutine()
    {
        // on désactive le navigator
        UI_XboxNavigator.Instance.Disable(this);
        yield break;
    }

    // SLOTTABLE
    public List<GameObject> GetSlots(ref Vector2 base_position, ref float angle_threshold, ref float angle_multiplicator)
    {
        // on récupère les slots
        List<GameObject> slots = new List<GameObject>();

        // on récupère les slots des texts
        foreach (Transform slot in slots_parent)
        {
            if (slot.gameObject.GetComponent<UI_Text>() != null && slot.gameObject.activeSelf)
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
    public Vector2 SavedPosition { get => base_position; }
}