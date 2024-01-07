using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Item : MonoBehaviour, I_Descriptable, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

 
    // hoover
    public bool is_hoovered { get; set; }
    public GameObject description_ui;

    // item
    public Item item;

    // ui_inventory
    public GameObject ui_inventory;


    // unity functions
    protected void Start()
    {
        // on récupère le description_ui
        description_ui = GameObject.Find("/ui/hoover_description");
    }

    // main functions
    public void setItem(Item item)
    {
        this.item = item;
    }

    public void setUIInventory(GameObject ui_inventory)
    {
        this.ui_inventory = ui_inventory;
    }

    // getters
    public string getDescription()
    {
        return item.getDescription();
    }

    public bool shouldDescriptionBeShown() {return true;}

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

        // on click
        ui_inventory.GetComponent<UI_Inventory>().clickOnItem(item);
    }
}