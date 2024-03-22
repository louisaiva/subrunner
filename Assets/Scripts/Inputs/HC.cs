using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class HC : MonoBehaviour
{

    // HC means Hint Control -> shows controls to the player

    [Header("HC")]
    private InputManager inputs;
    private HC_Manager manager;
    private HCBank bank;

    [Header("Colors")]
    [SerializeField] private Color hint_color = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color clicked_color = new Color(1f, 1f, 0f, 1f);


    [Header("actions")]
    [SerializeField] private List<InputActionReference> actions = new List<InputActionReference>(); // store the slots with their associate type
    [SerializeField] private List<Transform> slots = new List<Transform>(); // store the slots with their associate type
    [SerializeField] private List<string> slots_types = new List<string>(); // store the slots with their associate type


    [Header("keyboard")]
    [SerializeField] private Vector3 text_position_base;
    [SerializeField] private Vector3 text_position_offset = new Vector3(0, -1f / 8f, 0);


    void Start()
    {
        // Set the inputs
        inputs = GameObject.Find("/utils/input_manager").GetComponent<InputManager>();
        manager = inputs.GetComponent<HC_Manager>();

        // Set the bank
        bank = inputs.bank;
        
        // we try to find if we have some slots or not
        if (actions.Count == slots.Count && slots.Count == slots_types.Count && slots.Count > 0)
        {
            // we warn the manager
            manager.addHC(this);
        }
    }

    void OnEnable()
    {
        
        if (manager != null)
        {
            reset();
            manager.addHC(this);
        }
    }

    void OnDisable()
    {
        if (manager != null) { manager.delHC(this); }
    }

    // ACTIONS SETTING
    public void addAction(InputActionReference action, string type, GameObject slot)
    {
        // we add the action
        actions.Add(action);
        slots_types.Add(type);

        // we add the slot to the children
        slot.transform.SetParent(transform);
        slots.Add(slot.transform);
    }


    // ACTIONS CALLBACKS
    private void reset()
    {
        for (int i=0; i<actions.Count; i++)
        {
            if (slots_types[i] == "keyboard" || slots_types[i].Contains("2Daxis"))
            {
                updateKeyboard(slots[i], false);
            }
            else if (slots_types[i] == "pad")
            {
                updatePad(slots[i], false);
            }
            else if (slots_types[i] == "joystick")
            {
                updateJoystick(slots[i], new Vector2(0, 0));
            }
        }
    }

    public void switchToKeyboard()
    {
        for (int i=0; i<actions.Count; i++)
        {
            slots[i].transform.parent.gameObject.SetActive( slots_types[i] == "keyboard" || slots_types[i].Contains("2Daxis"));
        }
    }

    public void switchToGamepad()
    {
        for (int i=0; i<actions.Count; i++)
        {
            slots[i].transform.parent.gameObject.SetActive(!(slots_types[i] == "keyboard" || slots_types[i].Contains("2Daxis")));
        }
    }

    public void activate(InputAction action, bool is_clicked)
    {
        // we get the index
        List<int> indexes = getIndexesOfAction(action);

        /* string s= "(HC - " + transform.parent.parent.gameObject.name + ") activate " + action + " " + is_clicked + " " + indexes.Count + " indexes : ";
        foreach (int index in indexes)
        {
            s += index + " ";
        }
        Debug.Log(s); */

        foreach (int index in indexes)
        {
            // we check the type
            string type = slots_types[index];

            if (type == "pad")
            {
                updatePad(slots[index], is_clicked);
            }
            else if (type == "joystick")
            {
                updateJoystick(slots[index], is_clicked ? new Vector2(action.ReadValue<Vector2>().x, action.ReadValue<Vector2>().y) : new Vector2(0, 0));
            }
            else if (type == "keyboard")
            {
                updateKeyboard(slots[index], is_clicked);
            }
            else if (type.Contains("2Daxis"))
            {
                // we get the axis
                string axis_name = type.Split('_')[1];
                Vector2 axis2D = new Vector2(0, 0);
                if (axis_name == "+x")
                {
                    axis2D = new Vector2(1, 0);
                }
                else if (axis_name == "-x")
                {
                    axis2D = new Vector2(-1, 0);
                }
                else if (axis_name == "+y")
                {
                    axis2D = new Vector2(0, 1);
                }
                else if (axis_name == "-y")
                {
                    axis2D = new Vector2(0, -1);
                }

                // we get the direction
                Vector2 direction = new Vector2(action.ReadValue<Vector2>().x, action.ReadValue<Vector2>().y);

                update2DAxis(slots[index], axis2D, direction);
            }

        }

    }


    // on met à jour les hints
    private void updatePad(Transform pad, bool is_clicked)
    {
        // Debug.Log("(HC - " + transform.parent.parent.gameObject.name + ") updatePad " + pad.gameObject.name + " is pressed ?" + is_clicked);

        // we set the color
        if (is_clicked)
        {
            pad.GetComponent<Image>().color = clicked_color;
        }
        else
        {
            pad.GetComponent<Image>().color = hint_color;
        }
    }

    private void updateJoystick(Transform joystick, Vector2 direction)
    {
        /* // we get the joystick
        Transform joystick = transform.Find(joystick_name);
        if (joystick == null) { return; } */
        // joystick = joystick.Find("joy");
        // if (joystick == null) { return; }

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

    private void updateKeyboard(Transform slot, bool is_clicked)
    {
        // we get the key
        TextMeshPro key = slot.Find("key").GetComponent<TextMeshPro>();

        // si on a le meme texte qu'affiché alors on clique sur le bouton
        if (is_clicked)
        {
            // we change the color
            slot.GetComponent<Image>().color = clicked_color;
            key.color = clicked_color;

            // we change the sprite
            if (key.text.ToUpper() == "SPACE")
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
            if (key.text.ToUpper() == "SPACE")
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

    private void update2DAxis(Transform slot, Vector2 axis2D, Vector2 direction)
    {
        // we check if it's a release
        if (direction.x == 0 && direction.y == 0)
        {
            updateKeyboard(slot, false);
            return;
        }

        // we get the direction
        float x = direction.x;
        float y = direction.y;

        // we check if it is on the axis ?
        if (axis2D.x == 1 && x > 0.1f)
        {
            updateKeyboard(slot, true);
        }
        else if (axis2D.x == -1 && x < -0.1f)
        {
            updateKeyboard(slot, true);
        }
        else if (axis2D.y == 1 && y > 0.1f)
        {
            updateKeyboard(slot, true);
        }
        else if (axis2D.y == -1 && y < -0.1f)
        {
            updateKeyboard(slot, true);
        }
    }


    // getters & setters
    public bool hasAction(InputAction action)
    {
        // we check if the action is in the list
        for (int i = 0; i < actions.Count; i++)
        {
            if (actions[i].action == action && slots[i].gameObject.activeSelf && slots[i].transform.parent.gameObject.activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    public List<int> getIndexesOfAction(InputAction action)
    {
        // we check if the action is in the list
        List<int> indexes = new List<int>();
        for (int i = 0; i < actions.Count; i++)
        {
            if (actions[i].action == action && slots[i].gameObject.activeSelf && slots[i].transform.parent.gameObject.activeSelf)
            {
                // Debug.Log("found action " + action + " at index " + i + " in " + transform.parent.gameObject.name + " with type " + slots_types[i]);
                indexes.Add(i);
            }
        }
        return indexes;
    }

    public List<InputAction> getActions()
    {
        return actions.ConvertAll(action => action.action);
    }

    public List<InputAction> getKeyboardActions()
    {
        

        List<InputAction> kb_actions = new List<InputAction>();
        for (int i=0; i<actions.Count; i++)
        {
            if (slots_types[i] == "keyboard" || slots_types[i].Contains("2Daxis"))
            {
                kb_actions.Add(actions[i].action);
            }
        }
        /* string s="(HC - " + transform.parent.parent.gameObject.name + ") getKeyboardActions : ";
        foreach (InputAction action in kb_actions)
        {
            s += action.name + " ";
        }
        Debug.Log(s); */

        return kb_actions;
    }

    public List<InputAction> getGamepadActions()
    {
        List<InputAction> gp_actions = new List<InputAction>();
        for (int i = 0; i < actions.Count; i++)
        {
            if (slots_types[i] == "pad" || slots_types[i] == "joystick")
            {
                gp_actions.Add(actions[i].action);
            }
        }
        return gp_actions;
    }

}