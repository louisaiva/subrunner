using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_Pool : MonoBehaviour
{
    [Header("Pool paramaters")]
    public string Reference = "pool";
    public bool Showed = false;
    public bool HasCancelAction = false; // if true, the UI_Manager will activate the cancel action when the pool is showed
    public bool UsePersoInputs = true; // if true, the UI_Manager will activate the inputs.perso when the pool is showed


    [Header("UI Elements")]
    [SerializeField] protected List<GameObject> ui_elements = new List<GameObject>();
    
    [Header("Inputs")]
    protected PlayerInputActions inputs;

    [Header("Debug")]
    [SerializeField] protected bool debug = false;

    // AWAKE
    protected virtual void Awake()
    {

        Hide();
    }

    // START
    private void Start()
    {
        // we get the inputs
        inputs = GameObject.Find("/utils/input_manager").GetComponent<InputManager>().inputs;
    }

    // SHOW / HIDE
    public virtual void Show()
    {
        // on affiche tous les éléments
        if (debug) { Debug.Log("(UI_Pool) showing pool : " + Reference); }
        foreach (GameObject ui in ui_elements)
        {
            ui.SetActive(true);
        }

        Showed = true;

        // s'il a une activate action, on désactive les inputs.perso
        if (UsePersoInputs)
        {
            inputs.perso.Enable();
            if (inputs.UI.navigate.bindings.Count > 1)
            {
                inputs.UI.navigate.ChangeBinding(1).Erase();
            }
        }
        else
        {
            inputs.perso.Disable();
            inputs.UI.navigate.AddBinding("<Gamepad>/leftStick");
        }
    }
    public virtual void Hide()
    {
        // on cache tous les éléments du pool
        if (debug) { Debug.Log("(UI_Pool) hiding pool : " + Reference); }
        foreach (GameObject ui in ui_elements)
        {
            ui.SetActive(false);
        }
        
        Showed = false;
    }

    // REGISTER ELEMENTS
    public void RegisterToPool(GameObject ui_element)
    {
        // if the gameobject is null, we return
        if (ui_element == null) { return; }

        // we check if the element is already in the pool
        else if (ui_elements.Contains(ui_element))
        {
            if (debug) { Debug.LogWarning("(UI_Manager) " + ui_element.name + " tried to register to a pool it's already in : " + Reference); }
            return;
        }

        if (debug) { Debug.Log("(UI_Manager) " + ui_element.name + " just registered to pool : " + Reference); }

        // on ajoute l'élément au pool
        ui_elements.Add(ui_element);

        // we show/hide the element if the pool is showed
        ui_element.SetActive(Showed);
    }
    public void QuitPool(GameObject ui_element)
    {
        // if the gameobject is null, we return
        if (ui_element == null) { return; }

        // we check if the element is already in the pool
        else if (!ui_elements.Contains(ui_element))
        {
            if (debug) { Debug.LogWarning("(UI_Manager) " + ui_element.name + " tried to quit a pool it's not in : " + Reference); }
            return;
        }

        if (debug) { Debug.Log("(UI_Manager) " + ui_element.name + " just quit pool : " + Reference); }

        // on enlève l'élément du pool
        ui_elements.Remove(ui_element);
    }
}