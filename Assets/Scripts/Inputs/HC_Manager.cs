using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public class HC_Manager : MonoBehaviour
{
    

    [Header("HC MANAGER")]
    [SerializeField] private string current_input_type = "keyboard"; // keyboard or gamepad
    // [SerializeField] private GameObject perso;
    [SerializeField] private InputManager inputs;
    [SerializeField] public HCBank bank;
    
    [Header("HC")]
    [SerializeField] private List<HC> hcs = new List<HC>();


    [Header("Listeners")]
    [SerializeField] private Dictionary<InputAction,List<Action<InputAction.CallbackContext>>> listeners = new Dictionary<InputAction,List<Action<InputAction.CallbackContext>>>();

    // unity functions
    void Start()
    {
        // on récupère les inputs
        inputs = GetComponent<InputManager>();

        // on launch la bank
        // bank = new HCBank();

        // on récupère les HC
        // hcs = new List<HC>();   
    }

    void Update()
    {
        // we check if we are using the keyboard
        if (current_input_type != inputs.getCurrentInputType())
        {
            current_input_type = inputs.getCurrentInputType();
            if (current_input_type == "keyboard")
            {
                // we switch all the HC to keyboard
                foreach (HC hc in hcs)
                {
                    hc.switchToKeyboard();
                }
            }
            else
            {
                // we switch all the HC to gamepad
                foreach (HC hc in hcs)
                {
                    hc.switchToGamepad();
                }
            }
        }
    }

    // HC GESTION
    public void addHC(HC hc)
    {
        if (hcs.Contains(hc)) { return; }

        hcs.Add(hc);

        // we switch the HC to the current input type
        if (current_input_type == "keyboard")
        {
            hc.switchToKeyboard();
        }
        else
        {
            hc.switchToGamepad();
        }

        // on ajoute les listeners
        foreach (InputAction action in hc.getActions())
        {
            if (listeners.Keys.Contains(action)) { continue; }

            List<Action<InputAction.CallbackContext>> actions = new List<Action<InputAction.CallbackContext>>();
            actions.Add(ctx => activateHC(action, true));
            actions.Add(ctx => activateHC(action, false));

            action.performed += actions[0];
            action.canceled += actions[1];
            listeners.Add(action,actions);
        }
    }

    public void delHC(HC hc)
    {
        hcs.Remove(hc);

        // on supprime les listeners
        foreach (InputAction action in hc.getActions())
        {
            // on vérifie si on a ce listener
            if (!listeners.Keys.Contains(action)) { continue; }

            // on vérifie si on a pas encore besoin de ce listener
            List<HC> hc_with_action = hcs.Where(hc => hc.hasAction(action)).ToList();
            if (hc_with_action.Count > 0) { continue; }

            // on récupère les actions
            List<Action<InputAction.CallbackContext>> actions = listeners[action];
            action.performed -= actions[0];
            action.canceled -= actions[1];

            listeners.Remove(action);
        }
    }

    public void activateHC(InputAction action, bool is_clicked)
    {
        // on trie les HC en fonction de si ils ont cette action ou pas
        List<HC> hc_with_action = hcs.Where(hc => hc.hasAction(action)).ToList();

        for (int i = 0; i < hc_with_action.Count; i++)
        {
            hc_with_action[i].activate(action,is_clicked);
        }
    }

}