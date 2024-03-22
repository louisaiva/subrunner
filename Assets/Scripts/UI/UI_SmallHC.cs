using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_SmallHC : MonoBehaviour
{

    [Header("Sprites & Colors")]
    private HCBank bank;
    [SerializeField] private Color hint_color = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color clicked_color = new Color(1f, 1f, 0f, 1f);

    [Header("Inputs")]
    private PlayerInputActions inputs;
    private InputManager manager;
    private string current_input_type = "gamepad"; // keyboard or gamepad

    [Header("Gamepad")]
    [SerializeField] private Transform gamepad_slot;

    [Header("Keyboard")]
    [SerializeField] private Transform kb;
    [SerializeField] private Transform slot;
    private string key_name;
    private Vector3 text_position_base;
    private Vector3 text_position_offset = new Vector3(0, -1f/8f, 0);



    void Start()
    {
        // Set the inputs
        manager = GameObject.Find("/utils/input_manager").GetComponent<InputManager>();

        // Set the bank
        bank = manager.bank;

        // Set the gamepad
        if (gamepad_slot == null)
        {
            gamepad_slot = transform.Find("xyab");
        }

        // we get the keyboard
        kb = transform.Find("kb");
        if (kb == null) { return; }
        if (kb.Find("space").gameObject.activeSelf)
        {
            slot = kb.Find("space");
            key_name = "space";
            text_position_base = slot.Find("key").localPosition;
        }
        else
        {
            slot = kb.Find("slot");
            key_name = slot.Find("key").GetComponent<TextMeshPro>().text;
            text_position_base = slot.Find("key").localPosition;
        }

        // we set the current input type
        current_input_type = "gamepad"; // "keyboard
        kb.gameObject.SetActive(false);
        gamepad_slot.gameObject.SetActive(true);
    }

    void OnEnable()
    {
        // on récupère les inputs
        inputs = GameObject.Find("/utils/input_manager").GetComponent<InputManager>().inputs;
        if (inputs == null)
        {
            // Debug.LogWarning("(UI_SmallHC) inputs are null");
            return;
        }

        inputs.TUTO.Enable();

        // on ajoute les listeners
        inputs.TUTO.x.performed += ctx => updateButton("x", true);
        inputs.TUTO.x.canceled += ctx => updateButton("x", false);
        inputs.TUTO.y.performed += ctx => updateButton("y", true);
        inputs.TUTO.y.canceled += ctx => updateButton("y", false);
        inputs.TUTO.a.performed += ctx => updateButton("a", true);
        inputs.TUTO.a.canceled += ctx => updateButton("a", false);
        inputs.TUTO.b.performed += ctx => updateButton("b", true);
        inputs.TUTO.b.canceled += ctx => updateButton("b", false);

        inputs.TUTO.U.performed += ctx => updateButton("U", true);
        inputs.TUTO.U.canceled += ctx => updateButton("U", false);
        inputs.TUTO.R.performed += ctx => updateButton("R", true);
        inputs.TUTO.R.canceled += ctx => updateButton("R", false);
        inputs.TUTO.D.performed += ctx => updateButton("D", true);
        inputs.TUTO.D.canceled += ctx => updateButton("D", false);
        inputs.TUTO.L.performed += ctx => updateButton("L", true);
        inputs.TUTO.L.canceled += ctx => updateButton("L", false);

        inputs.TUTO.LT.performed += ctx => updateButton("LT", true);
        inputs.TUTO.LT.canceled += ctx => updateButton("LT", false);
        inputs.TUTO.RT.performed += ctx => updateButton("RT", true);
        inputs.TUTO.RT.canceled += ctx => updateButton("RT", false);
        inputs.TUTO.RB.performed += ctx => updateButton("RB", true);
        inputs.TUTO.RB.canceled += ctx => updateButton("RB", false);
        inputs.TUTO.LB.performed += ctx => updateButton("LB", true);
        inputs.TUTO.LB.canceled += ctx => updateButton("LB", false);

        inputs.TUTO.start.performed += ctx => updateButton("start", true);
        inputs.TUTO.start.canceled += ctx => updateButton("start", false);
        inputs.TUTO.select.performed += ctx => updateButton("select", true);
        inputs.TUTO.select.canceled += ctx => updateButton("select", false);

        inputs.TUTO.joyL.performed += ctx => updateJoystick("joyL", ctx.ReadValue<Vector2>());
        inputs.TUTO.joyL.canceled += ctx => updateJoystick("joyL", Vector2.zero);
        inputs.TUTO.joyR.performed += ctx => updateJoystick("joyR", ctx.ReadValue<Vector2>());
        inputs.TUTO.joyR.canceled += ctx => updateJoystick("joyR", Vector2.zero);

        // if (kb == null) { return; }

        inputs.TUTO.kb.performed += ctx => updateKeyboard(ctx.control.name, true);
        inputs.TUTO.kb.canceled += ctx => updateKeyboard(ctx.control.name, false);
    }

    void OnDisable()
    {
        if (inputs == null) { return; }


        // inputs.TUTO.Disable();

        // on enlève les listeners
        inputs.TUTO.x.performed -= ctx => updateButton("x", true);
        inputs.TUTO.x.canceled -= ctx => updateButton("x", false);
        inputs.TUTO.y.performed -= ctx => updateButton("y", true);
        inputs.TUTO.y.canceled -= ctx => updateButton("y", false);
        inputs.TUTO.a.performed -= ctx => updateButton("a", true);
        inputs.TUTO.a.canceled -= ctx => updateButton("a", false);
        inputs.TUTO.b.performed -= ctx => updateButton("b", true);
        inputs.TUTO.b.canceled -= ctx => updateButton("b", false);

        inputs.TUTO.U.performed -= ctx => updateButton("U", true);
        inputs.TUTO.U.canceled -= ctx => updateButton("U", false);
        inputs.TUTO.R.performed -= ctx => updateButton("R", true);
        inputs.TUTO.R.canceled -= ctx => updateButton("R", false);
        inputs.TUTO.D.performed -= ctx => updateButton("D", true);
        inputs.TUTO.D.canceled -= ctx => updateButton("D", false);
        inputs.TUTO.L.performed -= ctx => updateButton("L", true);
        inputs.TUTO.L.canceled -= ctx => updateButton("L", false);

        inputs.TUTO.LT.performed -= ctx => updateButton("LT", true);
        inputs.TUTO.LT.canceled -= ctx => updateButton("LT", false);
        inputs.TUTO.RT.performed -= ctx => updateButton("RT", true);
        inputs.TUTO.RT.canceled -= ctx => updateButton("RT", false);
        inputs.TUTO.RB.performed -= ctx => updateButton("RB", true);
        inputs.TUTO.RB.canceled -= ctx => updateButton("RB", false);
        inputs.TUTO.LB.performed -= ctx => updateButton("LB", true);
        inputs.TUTO.LB.canceled -= ctx => updateButton("LB", false);

        inputs.TUTO.start.performed -= ctx => updateButton("start", true);
        inputs.TUTO.start.canceled -= ctx => updateButton("start", false);
        inputs.TUTO.select.performed -= ctx => updateButton("select", true);
        inputs.TUTO.select.canceled -= ctx => updateButton("select", false);

        inputs.TUTO.joyL.performed -= ctx => updateJoystick("joyL", ctx.ReadValue<Vector2>());
        inputs.TUTO.joyL.canceled -= ctx => updateJoystick("joyL", Vector2.zero);
        inputs.TUTO.joyR.performed -= ctx => updateJoystick("joyR", ctx.ReadValue<Vector2>());
        inputs.TUTO.joyR.canceled -= ctx => updateJoystick("joyR", Vector2.zero);

        inputs.TUTO.kb.performed -= ctx => updateKeyboard(ctx.control.name, true);
        inputs.TUTO.kb.canceled -= ctx => updateKeyboard(ctx.control.name, false);

        Debug.LogWarning("(UI_SmallHC) OnDisable called for " + transform.parent.parent.gameObject.name );//+ "/" + transform.parent.gameObject.name);
    }

    void Update()
    {
        if (kb == null || slot == null) { return; }

        // we check if we are using the keyboard
        if (current_input_type != manager.getCurrentInputType())
        {
            current_input_type = manager.getCurrentInputType();
            if (current_input_type == "keyboard")
            {
                gamepad_slot.gameObject.SetActive(false);
                kb.gameObject.SetActive(true);
            }
            else
            {
                gamepad_slot.gameObject.SetActive(true);
                kb.gameObject.SetActive(false);
            }
        }
    }

    // on met à jour les hints
    private void updateButton(string button_name, bool is_clicked)
    {
        print("WOW " + button_name + " " + transform.parent.parent.gameObject.name);

        // we get the category
        string category = getCategory(button_name);
        if (category == "") { return; }
        
        // we get the button
        try
        {
            Transform butto2n = transform.Find(category);
        }
        catch
        {
            Debug.LogWarning("button not found on object " + transform.parent.gameObject.name);
            return;
        }



        Transform button = transform.Find(category);
        if (button == null) { return; }
        button = button.Find(button_name);
        if (button == null) { return; }

        // we set the color
        if (is_clicked)
        {
            button.GetComponent<Image>().color = clicked_color;
        }
        else
        {
            button.GetComponent<Image>().color = hint_color;
        }
    }

    private void updateJoystick(string joystick_name, Vector2 direction)
    {
        // we get the joystick
        Transform joystick = transform.Find(joystick_name);
        if (joystick == null) { return; }
        joystick = joystick.Find("moving");
        if (joystick == null) { return; }

        // we get the direction
        float x = direction.x;
        float y = direction.y;

        // we get the angle
        float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        // we get the distance
        float distance = Mathf.Sqrt(x * x + y * y);

        // we get the sprite
        Image img = joystick.GetComponent<Image>();

        // we set the sprite
        if (distance > 0f)
        {
            // we find the right sprite
            string sprite_name = "joy";
            if (angle > 22.5f && angle <= 67.5f)
            {
                sprite_name += "UR";
            }
            else if (angle > 67.5f && angle <= 112.5f)
            {
                sprite_name += "U";
            }
            else if (angle > 112.5f && angle <= 157.5f)
            {
                sprite_name += "UL";
            }
            else if (angle > 157.5f || angle <= -157.5f)
            {
                sprite_name += "L";
            }
            else if (angle > -157.5f && angle <= -112.5f)
            {
                sprite_name += "DL";
            }
            else if (angle > -112.5f && angle <= -67.5f)
            {
                sprite_name += "D";
            }
            else if (angle > -67.5f && angle <= -22.5f)
            {
                sprite_name += "DR";
            }
            else
            {
                sprite_name += "R";
            }

            // we print the bank
            /* string s = "sprite bank : " + bank.hint_sprites.Count + " \n";
            foreach (KeyValuePair<string, Sprite> entry in bank.hint_sprites)
            {
                s += entry.Key + " " + entry.Value + " \n";
            }
            Debug.Log(s); */



            // we set the sprite
            img.sprite = bank.hint_sprites[sprite_name];

            // we set the color
            img.color = clicked_color;

        }
        else
        {
            // we set the sprite
            img.sprite = bank.empty_sprites["joy"];

            // we set the color
            img.color = hint_color;
        }
    }

    private void updateKeyboard(string key_name, bool is_clicked)
    {
        if (this.key_name == null) { return; }

        // Debug.Log("updateKeyboard " + key_name.ToUpper() + " " + is_clicked + " " + this.key_name.ToUpper());
        if (key_name.ToUpper() != this.key_name.ToUpper()) { return; }

        // we get the button
        if (kb == null || !kb.gameObject.activeSelf) { return; }
        if (slot == null || !slot.gameObject.activeSelf) { return; }

        // we get the key
        TextMeshPro key = slot.Find("key").GetComponent<TextMeshPro>();

        // si on a le meme texte qu'affiché alors on clique sur le bouton
        if (is_clicked)
        {
            // we change the color
            slot.GetComponent<Image>().color = clicked_color;
            key.color = clicked_color;

            // we change the sprite
            if (key_name == "space")
            {
                slot.GetComponent<Image>().sprite = bank.hint_sprites["kb_space_clicked"];
            }
            else
            {
                slot.GetComponent<Image>().sprite = bank.hint_sprites["kb_slot_clicked"];
            }
            // we translate the text
            key.transform.localPosition = text_position_base + text_position_offset;
        }
        else
        {
            slot.GetComponent<Image>().color = hint_color;
            key.color = hint_color;

            // we change the sprite
            if (key_name == "space")
            {
                slot.GetComponent<Image>().sprite = bank.hint_sprites["kb_space_hint"];
            }
            else
            {
                slot.GetComponent<Image>().sprite = bank.hint_sprites["kb_slot_hint"];
            }

            // we reset the text
            key.transform.localPosition = text_position_base;
        }
    }


    // getters & setters
    private string getCategory(string button_name)
    {
        switch (button_name)
        {
            case "y":
            case "b":
            case "a":
            case "x":
                return "xyab";
            case "U":
            case "R":
            case "D":
            case "L":
                return "arrows";
            case "LT":
            case "RT":
            case "RB":
            case "LB":
                return "triggers";
            case "select":
            case "start":
                return "start_select";
            case "joyL":
                return "joyL";
            case "joyR":
                return "joyR";
            default:
                return "";
        }
    }

}