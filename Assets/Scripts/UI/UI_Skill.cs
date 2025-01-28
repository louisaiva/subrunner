using UnityEngine;
using UnityEngine.EventSystems;
// using UnityEngine.UIElements;
using UnityEngine.UI;

public class UI_Skill : MonoBehaviour, I_UI_Slot
{
    // hoover
    public bool is_hoovered { get; set; }
    public GameObject description_ui;

    // description
    public string skill_name = "max_life";
    public string human_readable_name = "max health";
    public string description = "your maximum health. makes you tanky as f";
    public string current_value = "/";
    public string next_value = "/";

    [Header("skill tree")]

    // skill tree
    public SkillTree skilltree;

    [Header("UI")]
    [SerializeField] private GameObject ui_bg;
    [SerializeField] private Color base_color = Color.white;
    [SerializeField] private Color hoover_color = Color.white;
    [SerializeField] private Color clicked_color = Color.white;


    // unity functions
    protected void Start()
    {
        // on récupère le skilltree
        skilltree = transform.parent.parent.gameObject.GetComponent<SkillTree>();

        // on récupère le description_ui
        description_ui = GameObject.Find("/ui/hoover_description");

        // on met à jour les valeurs
        current_value = skilltree.getSkillValue(skill_name).ToString();
        next_value = skilltree.getNextLevelSkillValue(skill_name).ToString();

        // on récupère le bg
        ui_bg = transform.Find("ui_bg").gameObject;

        // on met à jour les events du button
        // GetComponent<UnityEngine.UI.Button>().RegisterCallback<PointerEnterEvent>(OnPointerEnter);
        // GetComponent<UnityEngine.UI.Button>().RegisterCallback<PointerExitEvent>(OnPointerExit);
        // GetComponent<UnityEngine.UI.Button>().RegisterCallback<PointerClickEvent>(OnPointerClick);
    }
    // getters
    public string getDescription()
    {
        string s = "- " + human_readable_name + " -\n\n";
        s += description + "\n\n";
        s += "current: " + current_value + "\n";
        s += "next level: " + next_value + "\n";

        return s;
    }

    public bool shouldDescriptionBeShown()
    {
        // on regarde si le tree est actif
        return transform.parent.GetComponent<Canvas>().enabled;
    }

    // interface functions
    public void OnPointerEnter(PointerEventData eventData)
    {
        // on met à jour l'affichage
        description_ui.GetComponent<UI_HooverDescriptionHandler>().changeDescription(this);

        // on met à jour le fait qu'on est survolé
        is_hoovered = true;
        
        // on met à jour l'ui_bg
        ui_bg.GetComponent<Image>().color = hoover_color;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // on met à jour l'affichage
        description_ui.GetComponent<UI_HooverDescriptionHandler>().removeDescription(this);

        // on met à jour le fait qu'on est survolé
        is_hoovered = false;

        // on met à jour l'ui_bg
        ui_bg.GetComponent<Image>().color = base_color;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // on met à jour l'affichage
        description_ui.GetComponent<UI_HooverDescriptionHandler>().removeDescription(this);

        // on clique sur le button
        skilltree.levelUpSkill(skill_name);

        // on met à jour les valeurs
        current_value = skilltree.getSkillValue(skill_name).ToString();
        next_value = skilltree.getNextLevelSkillValue(skill_name).ToString();

        // on met à jour l'affichage
        description_ui.GetComponent<UI_HooverDescriptionHandler>().changeDescription(this);

        // on met à jour l'ui_bg
        ui_bg.GetComponent<Image>().color = base_color;
    }
}