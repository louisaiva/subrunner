using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Skill : UI_Slot, Descriptable
{
    // hover
    [Header("Hover")]
    public Color hover_color = new Color(1, 1, 0, 1);
    public Color down_color = new Color(1, 1, 1, 1);

    [Header("Skill")]
    private Image skill_bg;
    public string Reference = "stat:max_life";
    public string description = "your maximum health. makes you tanky as f";
    public string unit = "hp";

    [Header("Descriptable")]
    public string Name => Reference;
    public string Description => description;

    // AWAKE
    protected void Awake()
    {
        // on récupère le bg
        skill_bg = GetComponent<Image>();
    }

    // UI_SLOT
    public override void OnPointerEnter(PointerEventData eventData)
    {
        // on met à jour le fait qu'on est survolé
        skill_bg.color = hover_color;
        
        base.OnPointerEnter(eventData);
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        // on met à jour le fait qu'on est survolé
        skill_bg.color = new Color(1, 1, 1, 1);

        base.OnPointerExit(eventData);
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (log) Debug.Log("(UI_Skill) clicking on " + Reference);
        if (Controller.Perso == null) { return; }

        // reset the color
        skill_bg.color = new Color(1, 1, 1, 1);

        // Controller.Perso.skillManager.UpgradeSkill(Reference);

        // on reouvre le hud
        UI_Manager.Instance.SwitchToHUD(force: true);
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        skill_bg.color = down_color;

        if (log) { Debug.Log("(UI_Skill) downing " + Reference); }
    }
}