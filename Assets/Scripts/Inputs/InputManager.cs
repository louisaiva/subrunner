using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    
    [Header("INPUT MANAGER")]
    [SerializeField] private string current_input_type = "keyboard"; // keyboard or gamepad


    [Header("KEYBOARD")]

    [Header("GAMEPAD")]
    public PlayerInputActions inputs;
    private InputControlScheme keyboard_scheme;
    private InputControlScheme gamepad_scheme;

    // unity functions
    void Awake()
    {
        inputs = new PlayerInputActions();

        // on récupère le type d'input
        gamepad_scheme = inputs.asset.controlSchemes[0];
        keyboard_scheme = inputs.asset.controlSchemes[1];

        print("(InputManager) gamepad scheme: " + gamepad_scheme.name);
        print("(InputManager) keyboard scheme: " + keyboard_scheme.name);

    }

    // methods
    private void setInputType(string input_type)
    {
        if (current_input_type != input_type)
        {
            // on met à jour le type d'input
            current_input_type = input_type;
            print("(InputManager) switching to " + input_type);
        }
        else
        {
            print("(InputManager) " + input_type + " input detected");
        }


        current_input_type = input_type;
    }


    // getters
    public bool isUsingGamepad()
    {
        return current_input_type == "gamepad";
    }

}