using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// UI_LevelUpMenu is the menu thats shows when you level up (lol)
/// shows the pool of 3 UI_Skill and allows you to upgrade one of them
/// </summary>

public class UI_LevelUpMenu : UI_Pool, I_UI_Slottable
{
    [Header("Level Up Menu Components")]
    [SerializeField] private TMPro.TextMeshProUGUI level_text;
    [SerializeField] private float delay_before_activating_buttons = 1f;

    [Header("Slottable")]
    [SerializeField] private Transform skills_parent;
    // [SerializeField] private Vector2 base_position = new Vector2(0, 10000);


    [Header("Components")]
    public Description Descriptor;
    public Description SkillNameDescriptor;
    

    // POOL
    protected override IEnumerator show_coroutine(List<GameObject> dont_show = null)
    {
        yield return base.show_coroutine(dont_show);

        // update the level text
        level_text.text = "LEVEL " + GameObject.Find("/perso").GetComponent<Perso>().level.ToString();

        yield return new WaitForSecondsRealtime(delay_before_activating_buttons);

        // on active le navigator
        UI_XboxNavigator.Instance.Enable(this);
    }
    protected override IEnumerator hide_coroutine(List<GameObject> dont_hide = null)
    {
        // on désactive le navigator
        UI_XboxNavigator.Instance.Disable(this);

        yield return base.hide_coroutine(dont_hide);
    }

    // SLOTTABLE
    public List<GameObject> GetSlots(ref Vector2 base_position, ref float angle_threshold, ref float angle_multiplicator)
    {
        // on récupère les slots
        List<GameObject> slots = new List<GameObject>();

        // on récupère les slots des texts
        foreach (Transform slot in skills_parent)
        {
            if (slot.gameObject.GetComponent<UI_Skill>() != null && slot.gameObject.activeSelf)
            {
                slots.Add(slot.gameObject);
            }
        }

        // on met à jour les seuils
        // angle_threshold = base.angle_threshold;
        // angle_multiplicator = base.angle_multiplicator;

        // on met à jour la position de base
        // base_position = this.base_position;

        return slots;
    }
    public bool IsYourSlot(GameObject slot)
    {
        // on regarde si le slot est dans les slots
        if (slot.transform.IsChildOf(skills_parent))
        {
            return true;
        }
        return false;
    }
    public Vector2 SavedPosition { get; private set; } = new Vector2(Screen.width / 2f, Screen.height / 2f);
}