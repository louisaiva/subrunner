using System.Collections.Generic;
using UnityEngine;

public class UI_Pool : MonoBehaviour
{
    [Header("Pool paramaters")]
    public string Reference = "pool";
    public bool Showed = false;
    
    [Header("UI Elements")]
    [SerializeField] private List<GameObject> ui_elements = new List<GameObject>();

    /* [Header("Components")]
    [SerializeField] private UI_XboxNavigator navigator; */

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    // SHOW / HIDE
    public void Show()
    {
        if (Showed) { return; }

        // on affiche tous les éléments
        if (debug) { Debug.Log("(UI_Pool) showing pool : " + Reference); }
        foreach (GameObject ui in ui_elements)
        {
            ui.SetActive(true);
        }

        // on active le Xbox manager si c'est l'inventaire
        /* if (pool_name == "inventory")
        {
            UI_Inventory ui_inventory = transform.Find("inventory").GetComponent<UI_Inventory>();
            ui_inventory.Show();
            GetComponent<UI_XboxNavigator>()?.Enable(ui_inventory);
        } */

        Showed = true;
    }
    public void Hide(string pool_name)
    {
        if (!Showed) { return; }

        /* // on desactive le Xbox manager si c'est l'inventaire
        if (pool_name == "inventory")
        {
            UI_Inventory ui_inventory = transform.Find("inventory").GetComponent<UI_Inventory>();
            ui_inventory.Hide();
            GetComponent<UI_XboxNavigator>()?.Disable(ui_inventory);
        } */

        // on cache tous les éléments du pool
        if (debug) { Debug.Log("(UI_Pool) hiding pool : " + Reference); }
        foreach (GameObject ui in ui_elements)
        {
            ui.SetActive(false);
        }
        
        Showed = false;
    }

    public void TogglePool(string pool_name)
    {
        if (Showed)
        {
            Hide(pool_name);
        }
        else
        {
            Show();
        }
    }
}