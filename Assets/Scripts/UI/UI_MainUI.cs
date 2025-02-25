using UnityEngine;
using System.Collections.Generic;

public class UI_MainUI : MonoBehaviour
{
    // [SerializeField] private bool is_showed = true;
    [SerializeField] private List<GameObject> ui_elements = new List<GameObject>();
    [SerializeField] private Dictionary<GameObject, bool> ui_elements_states = new Dictionary<GameObject, bool>();
    
    // inputs
    private PlayerInputActions inputs;

    [Header("Debug")]
    public bool debug = false;


    // unity functions
    void Start()
    {
        // on active les inputs
        inputs = GameObject.Find("/utils/input_manager").GetComponent<InputManager>().inputs;

        if (debug) {Debug.Log("(UI_MainUI) UI inputs enabled");}

        // on récupère tous les ui_elements
        foreach (Transform child in transform)
        {
            if (child.gameObject.name == "hoover_description") { continue; }
            if (child.gameObject.name == "pause_menu") { continue; }
            ui_elements.Add(child.gameObject);
        }

        // on sauvegarde les states
        saveStates();
    }

    // SHOWING
    public void show()
    {
        // on remet les states
        // is_showed = true;
        foreach (GameObject ui_element in ui_elements)
        {
            if (ui_elements_states[ui_element])
            {
                ui_element.SetActive(true);
            }
            else
            {
                ui_element.SetActive(false);
            }
        }
    }

    public void hide()
    {

        // on sauvegarde les states
        saveStates();

        // on cache tout
        // is_showed = false;
        foreach (GameObject ui_element in ui_elements)
        {
            ui_element.SetActive(false);
        }
    }

    public void showOnly(GameObject ui_element)
    {
        // on sauvegarde les states
        saveStates();

        // on cache tout
        foreach (GameObject ui_element_ in ui_elements)
        {
            ui_element_.SetActive(false);
        }

        // on affiche l'ui_element
        ui_element.SetActive(true);
    }

    // STATES
    private void saveStates()
    {
        ui_elements_states.Clear();
        foreach (GameObject ui_element in ui_elements)
        {
            ui_elements_states.Add(ui_element, ui_element.activeSelf);
        }
    }

}