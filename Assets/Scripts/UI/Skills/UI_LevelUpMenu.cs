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

public class UI_LevelUpMenu : UI_Pool
{
    [Header("Level Up Menu Components")]
    [SerializeField] private TMPro.TextMeshProUGUI level_text;
    [SerializeField] private float delay_before_activating_buttons = 1f;

    [Header("Slottable")]
    [SerializeField] private Transform skills_parent;
    [SerializeField] private UI_Slottable slottable;


    // [Header("Components")]
    // public UI_Writer UI_ItemDescriptor;
    // public UI_Writer SkillNameDescriptor;
    

    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        // update the level text
        level_text.text = "LEVEL " + GameObject.Find("/perso").GetComponent<Perso>().level.ToString();

        yield return new WaitForSecondsRealtime(delay_before_activating_buttons);

        // on active le navigator
        slottable.Enable(ingame: false);
    }
    protected override IEnumerator disable_coroutine()
    {
        // on désactive le navigator
        slottable.Disable();
        yield break;
    }
}