using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    
    [Header("INPUT MANAGER")]
    [SerializeField] private string current_input_type = "keyboard"; // keyboard or gamepad
    public PlayerInputActions inputs;


    [Header("HintControl (HC)")]
    [SerializeField] public float joystick_treshold_min = 0.1f;


    [Header("Debug")]
    public bool debug = false;
    public bool debug_input_maps_enabled = false;


    // unity functions
    void Awake()
    {
        // on crée les inputs
        inputs = new PlayerInputActions();

        // on active les inputs
        inputs.perso.Enable();
        inputs.UI.Enable();
        inputs.any.Enable();

        // on ajoute les listeners
        inputs.any.keyboard.performed += ctx => setInputType("keyboard");
        inputs.any.gamepad.performed += ctx => setInputType("gamepad");
    }

    void Update()
    {
        if (debug_input_maps_enabled)
        {
            string s = "inputs maps: \n\t";
            s += "- perso : " + inputs.perso.enabled + "\n\t";
            s += "- ui : " + inputs.UI.enabled + "\n\t";
            s += "- any : " + inputs.any.enabled + "\n\t";
            s += "- enhanced_perso : " + inputs.enhanced_perso.enabled + "\n\t";
            Debug.Log(s);
        }
    }

    // setters
    private void setInputType(string input_type)
    {
        if (current_input_type != input_type)
        {
            // on met à jour le type d'input
            current_input_type = input_type;
            if (debug) { Debug.Log("(InputManager) switching to " + input_type);}
        }
    }

    // getters
    public InputAction GetActionFromString(string action_name)
    {
        // on découpe via / pour avoir l'inputMap
        string[] action_name_parts = action_name.Split('/');
        string inputMap = action_name_parts[0];
        action_name = action_name_parts[1];

        // on instancie l'action
        InputAction action = null;

        // on récupère l'action
        if (inputMap == "perso") { action = inputs.perso.Get()[action_name]; }
        else if (inputMap == "UI") { action = inputs.UI.Get()[action_name]; }
        else if (inputMap == "any") { action = inputs.any.Get()[action_name]; }
        else if (inputMap == "enhanced_perso") { action = inputs.enhanced_perso.Get()[action_name]; }

        // on debug
        if (debug) { Debug.Log("(InputManager) getting action : " + action_name + " from " + inputMap + " returned " + action); }

        // on retourne l'action
        return action;
    }
    public InputAction GetAction(InputActionReference reference)
    {
        // on récupère le nom de l'action
        string action_name = reference.ToString();

        // on découpe le nom de l'action avec : 
        string[] action_name_parts = action_name.Split(':');
        if (action_name_parts.Length > 1) { action_name = action_name_parts[1]; }

        // on retourne l'action
        return GetActionFromString(action_name);
    }
    public bool isUsingGamepad()
    {
        return current_input_type == "gamepad";
    }
    public string getCurrentInputType()
    {
        return current_input_type;
    }

}