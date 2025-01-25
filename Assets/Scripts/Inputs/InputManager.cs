using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    
    [Header("INPUT MANAGER")]
    [SerializeField] private string current_input_type = "keyboard"; // keyboard or gamepad
    [SerializeField] private GameObject perso;
    [SerializeField] public float joystick_treshold_min = 0.1f;
    public PlayerInputActions inputs;
    public HCBank bank;

    // unity functions
    void Awake()
    {
        // on récupère le perso
        perso = GameObject.Find("/perso");

        // on récupère les inputs
        inputs = new PlayerInputActions();

        // on launch la bank
        bank = new HCBank();


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