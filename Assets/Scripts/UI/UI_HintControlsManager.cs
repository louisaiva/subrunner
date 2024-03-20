using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class UI_HintControlsManager : MonoBehaviour
{

    [Header("HintsControls")]
    [SerializeField] private bool show_full_hints = false;
    
    [Header("Joystick")]
    [SerializeField] private GameObject xyab;
    [SerializeField] private GameObject joyL;
    [SerializeField] private GameObject joyR;
    [SerializeField] private GameObject arrows;
    [SerializeField] private GameObject triggers;
    [SerializeField] private GameObject start_select;

    [Header("Sprites")]
    // [SerializeField] private Dictionary<string, Sprite> empty_sprites = new Dictionary<string, Sprite>();
    // [SerializeField] private Dictionary<string, Sprite> hint_sprites = new Dictionary<string, Sprite>();

    private EnumHCI bank;
    [SerializeField] private Color empty_color = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private Color hint_color = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color clicked_color = new Color(1f, 1f, 0f, 1f);

    [Header("Inputs")]
    private PlayerInputActions player_input_actions;

    [Header("Hints")]
    private Dictionary<string,string> hints = new Dictionary<string, string>();

    // unity functions
    void Start()
    {
        bank = new EnumHCI();

        // on récupère les inputs
        player_input_actions = GameObject.Find("/utils/input_manager").GetComponent<InputManager>().inputs;
        player_input_actions.TUTO.Enable();

        // on ajoute les listeners
        player_input_actions.TUTO.x.performed += ctx => updateButton("x", true);
        player_input_actions.TUTO.x.canceled += ctx => updateButton("x", false);
        player_input_actions.TUTO.y.performed += ctx => updateButton("y", true);
        player_input_actions.TUTO.y.canceled += ctx => updateButton("y", false);
        player_input_actions.TUTO.a.performed += ctx => updateButton("a", true);
        player_input_actions.TUTO.a.canceled += ctx => updateButton("a", false);
        player_input_actions.TUTO.b.performed += ctx => updateButton("b", true);
        player_input_actions.TUTO.b.canceled += ctx => updateButton("b", false);
        
        player_input_actions.TUTO.U.performed += ctx => updateButton("U", true);
        player_input_actions.TUTO.U.canceled += ctx => updateButton("U", false);
        player_input_actions.TUTO.R.performed += ctx => updateButton("R", true);
        player_input_actions.TUTO.R.canceled += ctx => updateButton("R", false);
        player_input_actions.TUTO.D.performed += ctx => updateButton("D", true);
        player_input_actions.TUTO.D.canceled += ctx => updateButton("D", false);
        player_input_actions.TUTO.L.performed += ctx => updateButton("L", true);
        player_input_actions.TUTO.L.canceled += ctx => updateButton("L", false);

        player_input_actions.TUTO.LT.performed += ctx => updateButton("LT", true);
        player_input_actions.TUTO.LT.canceled += ctx => updateButton("LT", false);
        player_input_actions.TUTO.RT.performed += ctx => updateButton("RT", true);
        player_input_actions.TUTO.RT.canceled += ctx => updateButton("RT", false);
        player_input_actions.TUTO.RB.performed += ctx => updateButton("RB", true);
        player_input_actions.TUTO.RB.canceled += ctx => updateButton("RB", false);
        player_input_actions.TUTO.LB.performed += ctx => updateButton("LB", true);
        player_input_actions.TUTO.LB.canceled += ctx => updateButton("LB", false);

        player_input_actions.TUTO.start.performed += ctx => updateButton("start", true);
        player_input_actions.TUTO.start.canceled += ctx => updateButton("start", false);
        player_input_actions.TUTO.select.performed += ctx => updateButton("select", true);
        player_input_actions.TUTO.select.canceled += ctx => updateButton("select", false);

        player_input_actions.TUTO.joyL.performed += ctx => updateJoystick("joyL", ctx.ReadValue<Vector2>());
        player_input_actions.TUTO.joyL.canceled += ctx => updateJoystick("joyL", Vector2.zero);
        player_input_actions.TUTO.joyR.performed += ctx => updateJoystick("joyR", ctx.ReadValue<Vector2>());
        player_input_actions.TUTO.joyR.canceled += ctx => updateJoystick("joyR", Vector2.zero);

        // on désactive tout
        hide();

        init();
    }

    void init()
    {
        if (!show_full_hints) { return; }

        // on affiche les boutons
        showCat(joyL);
        showCat(joyR);
        showCat(arrows);
        showCat(triggers);
        showCat(start_select);
        showCat(xyab);
    }

    /* void Update()
    {
        // on print les hints
        string s = "";
        foreach (KeyValuePair<string, string> hint in hints)
        {
            s += hint.Key + " " + hint.Value + "\n";
        }
        print(s);
    } */

    // show & hide
    public void hide()
    {
        xyab.SetActive(false);
        joyL.SetActive(false);
        joyR.SetActive(false);
        arrows.SetActive(false);
        triggers.SetActive(false);
        start_select.SetActive(false);
    }

    private void showCat(GameObject category)
    {
        // resetCategory(category);

        // on reset tous les boutons de la catégorie
        if (category == joyL || category == joyR)
        {
            updateJoystick(category.name, Vector2.zero);
        }
        else
        {
            foreach (Transform child in category.transform)
            {
                updateButton(child.name, false);
            }
        }

        category.SetActive(true);

        // print("showing category " + category.name);
    }

    public void hideCat(string category_name)
    {
        GameObject category = getCatObject(category_name);
        if (category == null) { return; }

        category.SetActive(false);
    }

    public void addHint(string button_name)
    {
        if (hints.Keys.Contains(button_name)) { return; }

        // récupère la catégorie
        string category_name = getCategory(button_name);
        GameObject category = getCatObject(category_name);
        if (category == null) { return; }

        // on affiche la catégorie
        if (!category.activeSelf) { showCat(category); }

        // print("showing hint for " + button_name);

        // on regarde si c'est pas un joystick
        if (category == joyL || category == joyR)
        {
            if (category == joyL)
            {
                // on met le joystick
                hints.Add("joyL", "joyL");
                updateJoystick("joyL", player_input_actions.TUTO.joyL.ReadValue<Vector2>());

                print("setHintJoystick joyL");
            }
            else if (category == joyR)
            {
                // on met le joystick
                hints.Add("joyR", "joyR");
                updateJoystick("joyR", player_input_actions.TUTO.joyR.ReadValue<Vector2>());
            }
            return;
        }

        // on récupère le bouton
        // GameObject button = category.transform.Find(button_name).gameObject;

        // on ajoute le bouton aux hints
        hints.Add(button_name, category_name);

        // on met la couleur
        updateButton(button_name, false);
    }

    public void removeHint(string button_name)
    {
        if (!hints.Keys.Contains(button_name)) { return; }

        // print("hiding hint for " + button_name);

        // on regarde si c'est pas un joystick
        if (button_name == "joyL" || button_name == "joyR")
        {
            hints.Remove(button_name);
            hideCat(button_name);
            return;
        }

        // on enlève le bouton des hints
        string cat = hints[button_name];
        hints.Remove(button_name);

        // on reset le bouton
        updateButton(button_name, false);

        // on regarde si on doit cacher la catégorie
        if (!hints.Values.Contains(cat))
        {
            hideCat(cat);
        }
    }


    // on met à jour les hints
    private void updateButton(string button_name, bool is_clicked)
    {
        GameObject category = getCatObject(button_name);

        if (category == null) { return; }
        if (category == joyL || category == joyR) { return; }

        // we get the button
        GameObject button = category.transform.Find(button_name).gameObject;

        // we set the color
        if (is_clicked)
        {
            // button.GetComponent<SpriteRenderer>().sprite = hint_sprites[button_name];
            button.GetComponent<SpriteRenderer>().color = clicked_color;
        }
        else
        {
            // button.GetComponent<SpriteRenderer>().sprite = empty_sprites[button_name];
            if (hints.Keys.Contains(button_name))
            {
                button.GetComponent<SpriteRenderer>().color = hint_color;
                button.GetComponent<SpriteRenderer>().sprite = bank.hint_sprites[button_name];
            }
            else
            {
                button.GetComponent<SpriteRenderer>().color = empty_color;
                button.GetComponent<SpriteRenderer>().sprite = bank.empty_sprites[button_name];
            }
        }
    }

    private void updateJoystick(string joystick_name, Vector2 direction)
    {
        // we get the joystick
        GameObject joystick = getCatObject(joystick_name);
        if (joystick == null) { return; }

        // we get the button
        GameObject button = joystick.transform.Find("moving").gameObject;

        // we get the direction
        float x = direction.x;
        float y = direction.y;

        // we get the angle
        float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        // we get the distance
        float distance = Mathf.Sqrt(x * x + y * y);

        // we get the sprite
        SpriteRenderer sprite_renderer = button.GetComponent<SpriteRenderer>();

        // we set the sprite
        if (distance > 0.5f)
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
            sprite_renderer.sprite = bank.hint_sprites[sprite_name];

            // we set the color
            sprite_renderer.color = clicked_color;

        }
        else
        {
            // we set the sprite
            sprite_renderer.sprite = bank.empty_sprites["joy"];

            // we set the color
            if (hints.Keys.Contains(joystick_name))
            {
                sprite_renderer.color = hint_color;
            }
            else
            {
                sprite_renderer.color = empty_color;
            }
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

    private GameObject getCatObject(string name)
    {
        switch (name)
        {
            case "xyab":
                return xyab;
            case "joyL":
                return joyL;
            case "joyR":
                return joyR;
            case "arrows":
                return arrows;
            case "triggers":
                return triggers;
            case "start_select":
                return start_select;
            default:
                break;
        }
        
        string category_name = getCategory(name);
        if (category_name == "")
        {
            Debug.LogError("getCatObject : category name is empty for button " + name);
            return null;
        }
        return getCatObject(category_name);
    }
}

class EnumHCI
{
    public Dictionary<string, Sprite> empty_sprites = new Dictionary<string, Sprite>();
    public Dictionary<string, Sprite> hint_sprites = new Dictionary<string, Sprite>();

    private static EnumHCI instance = null;


    public EnumHCI()
    {
        if (instance != null) { return; }
        else { instance = this; }

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