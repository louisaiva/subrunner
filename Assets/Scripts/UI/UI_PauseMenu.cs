using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class UI_PauseMenu : MonoBehaviour, I_UI_Slottable
{
    private Transform text_slots;
    private Transform black;
    private Transform hc;
    private bool is_showed = false;

    [Header("Slottable")]
    [SerializeField] private Vector2 base_position = new Vector2(0, 10000);
    [SerializeField] private float angle_threshold = 45f;
    [SerializeField] private float angle_multiplicator = 100f;

    [Header("Inputs")]
    [SerializeField] private InputActionReference roll_input;
    // [SerializeField] private InputActionReference hide_input;
    // private event System.Action<InputAction.CallbackContext> pause_hide_callback;
    [SerializeField] private UI_XboxNavigator xbox_manager;
    [SerializeField] private InputManager input_manager;
    private UI_MainUI main_ui;

    [Header("Debug")]
    public bool debug = false;

    // unity functions
    void Start()
    {
        // on récupère les slots
        text_slots = transform.Find("texts");
        black = transform.Find("black");
        hc = transform.Find("hc");

        // on récupère le xbox_manager
        xbox_manager = GameObject.Find("/ui").GetComponent<UI_XboxNavigator>();

        // on récupère le main_ui
        main_ui = GameObject.Find("/ui").GetComponent<UI_MainUI>();

        // on récupère l'input_manager
        input_manager = GameObject.Find("/utils/input_manager").GetComponent<InputManager>();

        // on ajoute les listeners
        defineCallback();

        // on cache l'inventaire
        hide();
    }

    // CALLBACK
    private void defineCallback()
    {
        // we define the hide action callback
        // pause_hide_callback = ctx => hide();

        // we set the show callback
        roll_input.action.performed += ctx => rollShow();

        if (debug) { Debug.Log("(UI_Pause) set callback to rollShow()"); }
    }

    // SHOWING
    public void show()
    {
        if (debug) { Debug.Log("(UI_Pause) showing"); }

        // on disable le show input
        // show_input.action.Disable();
        // we set the hide callback
        // hide_input.action.performed += pause_hide_callback;

        // on cache le main_ui
        main_ui.show();
        main_ui.hide();

        // on affiche l'inventaire
        is_showed = true;

        // on affiche les éléments d'ui
        text_slots.gameObject.SetActive(true);
        black.gameObject.SetActive(true);
        hc.gameObject.SetActive(true);

        // on active le xbox_manager
        if (input_manager.isUsingGamepad()) { xbox_manager.enable(this); }

        // on arrête le temps
        Time.timeScale = 0;
    }

    public void hide()
    {
        if (debug) { Debug.Log("(UI_Pause) hiding"); }

        // we unset the hide callback
        // hide_input.action.performed -= pause_hide_callback;
        // on reenable le perso input
        // show_input.action.Enable();

        // on cache l'inventaire
        is_showed = false;

        // on cache les éléments d'ui
        text_slots.gameObject.SetActive(false);
        black.gameObject.SetActive(false);
        hc.gameObject.SetActive(false);


        // on désactive le xbox_manager
        xbox_manager.disable();

        // on affiche le main_ui
        main_ui.show();

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