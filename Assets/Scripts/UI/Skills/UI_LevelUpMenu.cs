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

public class UI_LevelUpMenu : UI_Pool/* , Slottable */
{
    [Header("Level Up Menu Components")]
    [SerializeField] private TMPro.TextMeshProUGUI level_text;
    [SerializeField] private float delay_before_activating_buttons = 1f;

    [Header("Slottable")]
    [SerializeField] private Transform skills_parent;
    [SerializeField] private UI_Slottable slottable;
    // [SerializeField] private Vector2 base_position = new Vector2(0, 10000);


    [Header("Components")]
    public Description Descriptor;
    public Description SkillNameDescriptor;
    

    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        // update the level text
        level_text.text = "LEVEL " + GameObject.Find("/perso").GetComponent<Perso>().level.ToString();

        yield return new WaitForSecondsRealtime(delay_before_activating_buttons);

        // on active le navigator
        // UI_Navigator.Instance.Enable(this);
        slottable.Enable(ingame: false, starting_slot: true);
    }
    protected override IEnumerator disable_coroutine()
    {
        // on désactive le navigator
        // UI_Navigator.Instance.Disable(this);
        slottable.Disable();
        yield break;
    }

    // SLOTTABLE
    /* public List<UI_Slot> GetSlots()
    {
        // on récupère les slots
        List<UI_Slot> slots = new List<UI_Slot>();

        // on récupère les slots des texts
        for (int i = 0; i < skills_parent.childCount; i++)
        {
            Transform slot = skills_parent.GetChild(i);
            if (!slot.gameObject.activeSelf) { continue; }
            UI_Skill skill = slot.gameObject.GetComponent<UI_Skill>();
            if (skill == null) { continue; }
            slots.Add(skill);
        }

        return slots;
    }
    public bool IsYourSlot(UI_Slot slot)
    {
        // on regarde si le slot est dans les slots
        return slot.transform.IsChildOf(skills_parent);
    } */
    // public Vector2 SavedPosition { get; private set; } = new Vector2(Screen.width / 2f, Screen.height / 2f);
}