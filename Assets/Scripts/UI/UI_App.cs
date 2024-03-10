using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_App : MonoBehaviour, I_UI_Slot
{
    // hoover
    public bool is_hoovered { get; set; }

    // ui_computer
    public GameObject ui_computer;

    // slot sprite
    public Sprite base_sprite;
    public Sprite hoover_sprite;

    // unity functions
    protected void Awake()
    {
        // on récupère les sprites
        Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheets/ui/ui_applis");
        base_sprite = sprites[0];
        hoover_sprite = sprites[1];
    }

    public void setUIComputer(GameObject ui_computer)
    {
        this.ui_computer = ui_computer;
    }

    // getters
    public string getDescription() { return ""; }

    public bool shouldDescriptionBeShown() { return false; }

    // interface functions
    public void OnPointerEnter(PointerEventData eventData)
    {
        // on change le sprite du slot
        GetComponent<Image>().sprite = hoover_sprite;

        // on met à jour le fait qu'on est survolé
        is_hoovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // on change le sprite du slot
        GetComponent<Image>().sprite = base_sprite;

        // on met à jour le fait qu'on est survolé
        is_hoovered = false;
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        // on change le sprite du slot
        GetComponent<Image>().sprite = base_sprite;
    }

    // reset hoover
    public void resetHoover()
    {
        if (!is_hoovered) return;

        OnPointerExit(null);
    }
}