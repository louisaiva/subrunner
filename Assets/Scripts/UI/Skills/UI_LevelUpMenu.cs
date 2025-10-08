using System;
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
    [SerializeField] private float delay_before_activating_buttons = 1f;

    [Header("Slottable")]
    [SerializeField] private Transform skills_parent;
    [SerializeField] private Vector2 base_position = new Vector2(0, 10000);


    [Header("Components")]
    public Description Descriptor;
    public Description SkillNameDescriptor;
    // private UI_XboxNavigator navigator;
    private TMPro.TextMeshProUGUI level_text;

    // AWAKE
    protected void Awake()
    {
        // navigator = GameObject.Find("/ui").GetComponent<UI_XboxNavigator>();
        level_text = transform.Find("text").GetComponent<TMPro.TextMeshProUGUI>();
    }

    // POOL
    public override async Awaitable Show(List<GameObject> dont_show = null)
    {
        await base.Show(dont_show);

        // update the level text
        level_text.text = "LEVEL " + GameObject.Find("/perso").GetComponent<Perso>().level.ToString();

        // sets base position to center of the screen
        base_position = new Vector2(Screen.width / 2f, Screen.height / 2f);

        await System.Threading.Tasks.Task.Delay((int)(delay_before_activating_buttons * 1000));

        // on active le navigator
        UI_XboxNavigator.Instance.Enable(this);
    }
    public override async Awaitable Hide(List<GameObject> dont_hide = null)
    {
        // on désactive le navigator
        UI_XboxNavigator.Instance.Disable(this);

        await base.Hide(dont_hide);
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
        angle_threshold = base.angle_threshold;
        angle_multiplicator = base.angle_multiplicator;

        // on met à jour la position de base
        base_position = this.base_position;

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
    public Vector2 SavedPosition { get; private set; } = Vector2.zero;
}