using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    
    [Header("INPUT MANAGER")]
    [SerializeField] private string current_input_type = "keyboard"; // keyboard or gamepad


    [Header("KEYBOARD")]

    [Header("GAMEPAD")]
    private PlayerInputActions gamepad_inputs;

    // unity functions
    void Start()
    {
        // on récupère le type d'input
        // current_input_type = PlayerPrefs.GetString("current_input_type", "keyboard");
        gamepad_inputs = GameObject.Find("/perso").GetComponent<Perso>().playerInputs;
        // gamepad_inputs.performed += ctx => setInputType("gamepad");

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