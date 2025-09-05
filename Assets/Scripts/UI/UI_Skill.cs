using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// using UnityEngine.UIElements;
using UnityEngine.UI;

public class UI_Skill : MonoBehaviour, I_UI_Slot
{
    // hover
    [Header("Hover")]
    public Color hover_color = new Color(1, 1, 0, 1);
    public Color down_color = new Color(1, 1, 1, 1);
    public bool is_hovered { get; set; }


    [Header("Skill")]
    private Image skill_bg;
    public string Reference = "stat:max_life";
    public string description = "your maximum health. makes you tanky as f";
    public string unit = "hp";

    [Header("Components")]
    private UI_LevelUpMenu menu;

    [Header("Logs")]
    public bool log = false;

    // AWAKE
    protected void Awake()
    {
        // on récupère les components
        menu = transform.parent.parent.GetComponent<UI_LevelUpMenu>();

        // on récupère le bg
        skill_bg = GetComponent<Image>();
    }

    private void update_description()
    {
        menu.SkillNameDescriptor.SetDescription(Reference);

        if (Perso.Instance == null) { menu.Descriptor.SetDescription("looks like there is no player anymore"); return; }

        string desc = "";
        desc += "current : " + Perso.Instance.skillManager.GetSkillValue(Reference).ToString() + " " + unit;
        desc += "\nnext : " + Perso.Instance.skillManager.GetNextLevelSkillValue(Reference).ToString() + " " + unit;
        desc += "\n\n" + description;

        // on met à jour la description
        menu.Descriptor.SetDescription(desc);
    }

    // I_UI_SLOT
    public void OnPointerEnter(PointerEventData eventData)
    {
        // on met à jour la description
        update_description();

        // on met à jour le fait qu'on est survolé
        skill_bg.color = hover_color;
        is_hovered = true;

        if (log) Debug.Log("(UI_Skill) hovering " + Reference);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        // on met à jour le fait qu'on est survolé
        skill_bg.color = new Color(1, 1, 1, 1);
        is_hovered = false;

        if (log) Debug.Log("(UI_Skill) unhovering " + Reference);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (log) Debug.Log("(UI_Skill) clicking on " + Reference);
        if (Perso.Instance == null) { return; }

        // reset the color
        skill_bg.color = new Color(1, 1, 1, 1);

        Perso.Instance.skillManager.UpgradeSkill(Reference);

        // on reouvre le hud
        GameObject.Find("/ui").GetComponent<UI_Manager>().SwitchTo("hud");
    }
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        skill_bg.color = down_color;

        if (log) { Debug.Log("(UI_Skill) downing " + Reference); }
    }
}