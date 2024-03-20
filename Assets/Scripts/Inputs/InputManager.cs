using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    
    [Header("INPUT MANAGER")]
    [SerializeField] private string current_input_type = "keyboard"; // keyboard or gamepad
    [SerializeField] private GameObject perso;
    public PlayerInputActions inputs;

    // unity functions
    void Awake()
    {
        // on récupère le perso
        perso = GameObject.Find("/perso");

        // on récupère les inputs
        inputs = new PlayerInputActions();

        // on ajoute les listeners
        inputs.any.Enable();
        inputs.any.keyboard.performed += ctx => setInputType("keyboard");
        inputs.any.gamepad.performed += ctx => setInputType("gamepad");

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
    }

    // getters
    public bool isUsingGamepad()
    {
        return current_input_type == "gamepad";
    }

    public string getCurrentInputType()
    {
        return current_input_type;
    }

}