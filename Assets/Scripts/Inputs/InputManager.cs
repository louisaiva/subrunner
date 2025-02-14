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
    public HCBank bank;


    [Header("Debug")]
    public bool debug = false;
    public bool debug_input_maps_enabled = false;


    // unity functions
    void Awake()
    {
        // on launch la bank
        bank = new HCBank();

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
            print("(InputManager) switching to " + input_type);
        }
    }

    // getters
    public InputAction GetAction(InputActionReference reference)
    {
        // on récupère le nom de l'action
        string action_name = reference.ToString();

        // on découpe le nom de l'action avec : 
        string[] action_name_parts = action_name.Split(':');
        if (action_name_parts.Length > 1) { action_name = action_name_parts[1]; }

        // on découpe ensuite via / pour avoir l'inputMap
        action_name_parts = action_name.Split('/');
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
    public bool isUsingGamepad()
    {
        return current_input_type == "gamepad";
    }
    public string getCurrentInputType()
    {
        return current_input_type;
    }

}
public class HCBank
{
    public Dictionary<string, Sprite> empty_sprites = new Dictionary<string, Sprite>();
    public Dictionary<string, Sprite> hint_sprites = new Dictionary<string, Sprite>();


    public HCBank()
    {
        // on récupère les sprites
        Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheets/ui/ui_interactables");

        // on les ajoute aux dictionnaires
        empty_sprites.Add("y", sprites[0]);
        hint_sprites.Add("y", sprites[1]);
        empty_sprites.Add("b", sprites[2]);
        hint_sprites.Add("b", sprites[3]);
        empty_sprites.Add("a", sprites[4]);
        hint_sprites.Add("a", sprites[5]);
        empty_sprites.Add("x", sprites[6]);
        hint_sprites.Add("x", sprites[7]);

        // empty_sprites.Add("joy", sprites[8]);
        empty_sprites.Add("joy", sprites[9]);
        hint_sprites.Add("joyU", sprites[10]);
        hint_sprites.Add("joyUR", sprites[11]);
        hint_sprites.Add("joyR", sprites[12]);
        hint_sprites.Add("joyDR", sprites[13]);
        hint_sprites.Add("joyD", sprites[14]);
        hint_sprites.Add("joyDL", sprites[15]);
        hint_sprites.Add("joyL", sprites[16]);
        hint_sprites.Add("joyUL", sprites[17]);

        empty_sprites.Add("U", sprites[18]);
        hint_sprites.Add("U", sprites[19]);
        empty_sprites.Add("R", sprites[20]);
        hint_sprites.Add("R", sprites[21]);
        empty_sprites.Add("D", sprites[22]);
        hint_sprites.Add("D", sprites[23]);
        empty_sprites.Add("L", sprites[24]);
        hint_sprites.Add("L", sprites[25]);

        empty_sprites.Add("LT", sprites[26]);
        hint_sprites.Add("LT", sprites[27]);
        empty_sprites.Add("RT", sprites[28]);
        hint_sprites.Add("RT", sprites[29]);
        empty_sprites.Add("RB", sprites[30]);
        hint_sprites.Add("RB", sprites[31]);
        empty_sprites.Add("LB", sprites[32]);
        hint_sprites.Add("LB", sprites[33]);

        empty_sprites.Add("start", sprites[34]);
        hint_sprites.Add("start", sprites[35]);
        empty_sprites.Add("select", sprites[36]);
        hint_sprites.Add("select", sprites[37]);


        // on ajoute les sprites du keyboard
        hint_sprites.Add("kb_slot_hint", sprites[38]);
        hint_sprites.Add("kb_slot_clicked", sprites[39]);
        hint_sprites.Add("kb_space_hint", sprites[40]);
        hint_sprites.Add("kb_space_clicked", sprites[41]);
    }
}