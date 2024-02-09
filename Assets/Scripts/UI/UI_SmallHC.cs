using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_SmallHC : MonoBehaviour
{

    [Header("Sprites")]
    private EnumHCI bank;
    [SerializeField] private Color hint_color = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color clicked_color = new Color(1f, 1f, 0f, 1f);

    [Header("Inputs")]
    private PlayerInputActions inputs;


    void Start()
    {
        // Set the bank
        bank = new EnumHCI();
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
    }

    void OnDisable()
    {
        if (inputs == null) { return; }

        inputs.TUTO.Disable();

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

    }

    // on met à jour les hints
    private void updateButton(string button_name, bool is_clicked)
    {

        // we get the category
        string category = getCategory(button_name);
        if (category == "") { return; }
        
        // we get the button
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