using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions;

public class UI_CinematicInputsController : InputController, UI_Controller
{
    public EndlessInput<T> GetEndlessInput<T>(string name) where T : struct { return get_endless_input<T>(name); }
    [SerializeField] private UIActions ui_inputs;
    private Action<InputAction.CallbackContext> ui_activateCallback = delegate { };

    [Header("FastForward")]
    [SerializeField] private float cinematic_speed = 1f;
    [SerializeField] private float cinematic_speed_max = 32f;

    [Header("Logs")]
    [SerializeField] private bool log_states = true;
    [SerializeField] private bool log_fastforward = true;


    // START
    protected void Start()
    {
        // on récupère les inputs et crée le callback
        ui_inputs = InputManager.Instance.inputs.UI;
        ui_activateCallback = ctx => handle_UI_activate_input(ctx);

        // on crée un endless input pour le fastforward
        EndlessInput<float> fastforward_input = add_endless_input(new EndlessInput<float>("fastforward", ui_inputs.activate,
                threshold: InputManager.Instance.BUTTON_ENDLESSLY_XTREM_LONG_THRESHOLD,
                repeat: InputManager.Instance.BUTTON_ENDLESSLY_XTREM_LONG_DELAY,
                unscaled_time: true));
        fastforward_input.OnEndless += _ => OnFastForwardSpeedUp();
        fastforward_input.OnResetted += () => OnFastForwardReset();

        if (log_states) { Debug.Log("(UI_CinematicInputsController) UCIC started"); }
    }

    // ENABLE / DISABLE
    public void EnableInputs(bool ingame)
    {
        ui_inputs.activate.performed += ui_activateCallback;
        if (log_states) { Debug.Log("(UI_CinematicInputsController) UCIC enabled"); }
    }
    public void DisableInputs()
    {
        ui_inputs.activate.performed -= ui_activateCallback;
        if (log_states) { Debug.Log("(UI_CinematicInputsController) UCIC disabled"); }

        // we cancel all the endless inputs we have
        cancel_all_inputs();
    }

    /// <summary>
    /// This method transmit the input to the endless input class
    /// </summary>
    /// <param name="context"></param>
    private void handle_UI_activate_input(InputAction.CallbackContext context)
    {
        // checks magnitue to know if we downed or released
        float activate_value = context.ReadValue<float>();
        if (activate_value >= 0.5f)
        {
            get_endless_input<float>("fastforward").OnInput(context);
            return;
        }

        // ! no need for manual cancelling, when calling the OnInput method it will register to cancelling automatically
    }

    /// <summary>
    /// These methods gather calls from the endless input class when we need them
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private void OnFastForwardReset()
    {
        if (log_fastforward) { Debug.Log("(UI_CIC) Fast forwarding is resetted to 0"); }
        cinematic_speed = 1f;
        
        Time.timeScale = 1f;
    }
    private void OnFastForwardSpeedUp()
    {
        if (log_fastforward) { Debug.Log("(UI_CIC) Fast forwarding cinematic !"); }
        cinematic_speed *= 2f;
        cinematic_speed = Mathf.Min(cinematic_speed, cinematic_speed_max);
        
        Time.timeScale = cinematic_speed;
    }


}