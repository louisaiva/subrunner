using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Skill : MonoBehaviour, I_Descriptable, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{


    // hoover
    public bool is_hoovered { get; set; }
    public GameObject description_ui;

    // description
    public string skill_name = "max_vie";
    public string human_readable_name = "max health";
    public string description = "your maximum health. makes you tanky as f";
    public string current_value = "/";
    public string next_value = "/";

    // skill tree
    public SkillTree skilltree;


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
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // on met à jour l'affichage
        description_ui.GetComponent<UI_HooverDescriptionHandler>().removeDescription(this);

        // on met à jour le fait qu'on est survolé
        is_hoovered = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // on met à jour l'affichage
        description_ui.GetComponent<UI_HooverDescriptionHandler>().removeDescription(this);

        // on met à jour les valeurs
        current_value = skilltree.getSkillValue(skill_name).ToString();
        next_value = skilltree.getNextLevelSkillValue(skill_name).ToString();

        // on met à jour l'affichage
        description_ui.GetComponent<UI_HooverDescriptionHandler>().changeDescription(this);
    }
}