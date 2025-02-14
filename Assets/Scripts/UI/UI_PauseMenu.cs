using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class UI_PauseMenu : MonoBehaviour, I_UI_Slottable
{

    private UI_Manager manager;
    
    [Header("UI elements")]
    [SerializeField] private Transform text_slots;
    [SerializeField] private Transform black;
    [SerializeField] private Transform joy_f;
    [SerializeField] private Transform pad_f;
    [SerializeField] private bool is_showed = false;

    [Header("Slottable")]
    [SerializeField] private Vector2 base_position = new Vector2(0, 10000);
    [SerializeField] private float angle_threshold = 45f;
    [SerializeField] private float angle_multiplicator = 100f;

    [Header("Inputs")]
    [SerializeField] private InputActionReference input;
    private InputAction roll_action;
    [SerializeField] private UI_XboxNavigator xbox_manager;
    [SerializeField] private InputManager input_manager;
    // callback
    private System.Action<InputAction.CallbackContext> cancel_callback;
    public System.Action<InputAction.CallbackContext> CancelCallback
    {
        get
        {
            return cancel_callback;
        }
    }

    [Header("Debug")]
    public bool debug = false;

    // unity functions
    void Start()
    {
        // on récupère les slots
        text_slots = transform.Find("texts");
        black = transform.Find("black");
        joy_f = transform.Find("joystick_feedback");
        pad_f = transform.Find("pad_feedback");

        // on récupère le xbox_manager
        xbox_manager = GameObject.Find("/ui").GetComponent<UI_XboxNavigator>();

        // on récupère le main_ui
        // main_ui = GameObject.Find("/ui").GetComponent<UI_MainUI>();
        manager = GameObject.Find("/ui").GetComponent<UI_Manager>();
        manager.RegisterToPool("pause", gameObject);

        // on récupère l'input_manager
        input_manager = GameObject.Find("/utils/input_manager").GetComponent<InputManager>();

        // on ajoute les listeners
        define_callback();

        // on cache l'inventaire
        hide();
    }

    // CALLBACK
    public void define_callback()
    {
        // we define the hide action callback
        // pause_hide_callback = ctx => hide();

        // we get the action from input_manager
        roll_action = input_manager.GetAction(input);

        if (debug)
        {
            InputAction manager_pause = GameObject.Find("/utils/input_manager").GetComponent<InputManager>().inputs.UI.pause;
            string s = "(UI_Pause) set callback to rollShow() : " + roll_action;
            s += "\n\t action == manager_pause action ?" + (manager_pause == roll_action);
            s += "\n\t action.actionMap == manager_pause action.actionMap ?" + (manager_pause.actionMap == roll_action.actionMap);
            Debug.Log(s);
        }

        // we set the show callback
        roll_action.performed += ctx => rollShow();

        // we set the cancel callback
        cancel_callback = ctx => hide();
    }

    // SHOWING
    public void show()
    {
        if (debug) { Debug.Log("(UI_Pause) showing"); }

        // on cache le main_ui
        // main_ui.hide();
        manager.ShowPool("pause");

        // on affiche l'inventaire
        is_showed = true;

        // on affiche les éléments d'ui
        text_slots.gameObject.SetActive(true);
        black.gameObject.SetActive(true);
        joy_f.gameObject.SetActive(true);
        pad_f.gameObject.SetActive(true);

        // on set le cancel callback
        input_manager.inputs.UI.cancel.performed += cancel_callback;

        // on active le xbox_manager
        if (input_manager.isUsingGamepad()) { xbox_manager.enable(this); }

        // on arrête le temps
        Time.timeScale = 0;

    }

    public void hide()
    {
        if (debug) { Debug.Log("(UI_Pause) hiding"); }

        // on cache l'inventaire
        is_showed = false;

        // on cache les éléments d'ui
        text_slots.gameObject.SetActive(false);
        black.gameObject.SetActive(false);
        joy_f.gameObject.SetActive(false);
        pad_f.gameObject.SetActive(false);

        // on deset le cancel callback
        input_manager.inputs.UI.cancel.performed -= cancel_callback;

        // on désactive le xbox_manager
        xbox_manager.disable();

        // on affiche le main_ui
        // main_ui.show();
        manager.HidePool("pause");

        // on remet le temps
        Time.timeScale = 1;
    }

    public void rollShow()
    {
        // on affiche l'inventaire
        if (is_showed) { hide(); }
        else { show(); }
    }

    public bool isShowed()
    {
        return is_showed;
    }

    // SLOTTABLE
    public List<GameObject> GetSlots(ref Vector2 base_position, ref float angle_threshold, ref float angle_multiplicator)
    {

        // on récupère les slots
        List<GameObject> slots = new List<GameObject>();

        // on récupère les slots des texts
        foreach (Transform child in text_slots)
        {
            if (child.gameObject.GetComponent<UI_Text>() != null && child.gameObject.activeSelf)
            {
                slots.Add(child.gameObject);
            }
        }

        // on met à jour les seuils
        angle_threshold = this.angle_threshold;
        angle_multiplicator = this.angle_multiplicator;

        // on met à jour la position de base
        base_position = this.base_position;

        return slots;
    }

}