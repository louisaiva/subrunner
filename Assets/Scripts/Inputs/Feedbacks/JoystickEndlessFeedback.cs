using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This class is used to give feedback to the player when they are pressing a Joystick
/// </summary>
public class JoystickEndlessFeedback : JoystickFeedback
{
    [Header("Joystick Endless Feedback")]
    [SerializeField] protected string endless_input_name = "";
    EndlessInput<Vector2> endless_input;
    [SerializeField] protected Color endless_color = new Color(1f, 1f, 0f, 1f);
    private Color saved_clicked_color;
    protected System.Action<Vector2> endless_callback;

    protected override void Start()
    {
        base.Start();

        // we save the clicked color
        saved_clicked_color = clicked_color;
    }

    // ONENABLE/DISABLE
    protected override void OnEnable()
    {
        // we get the endless input
        if (endless_input_name == "") { return; }
        if (endless_input == null) { endless_input = Controller.Instance.GetEndlessInput<Vector2>(endless_input_name); }
        if (endless_input == null)
        {
            if (log) { Debug.LogError("(JEF) endless input " + endless_input_name + " not found!"); }
            return;
        }

        // we get the bank
        if (bank == null)
        {
            bank = GameObject.Find("/utils/bank").GetComponent<SpriteBank>();
            if (log) { Debug.Log("(JEF) SpriteBank loaded : SpriteBank == " + bank); }
        }

        // we add listeners
        endless_input.OnStarted += input_callback;
        endless_input.OnHold += endless_callback;
        endless_input.OnEndless += endless_callback;
        endless_input.OnResetted += reset_callback;

        // we reset the IF
        OnReset();
    }
    protected override void OnDisable()
    {
        // we remove listeners
        endless_input.OnStarted -= input_callback;
        endless_input.OnHold -= endless_callback;
        endless_input.OnEndless -= endless_callback;
        endless_input.OnResetted -= reset_callback;
    }

    // CALLBACKS
    protected override void defineCallbacks()
    {
        // we define the callback
        input_callback =
            ctx =>
            {
                joystick_direction = ctx.ReadValue<Vector2>();
                clicked_color = saved_clicked_color;
                OnInput();
            };
        endless_callback = dir =>
            {
                joystick_direction = dir;
                clicked_color = endless_color;
                OnInput();
            };
        reset_callback = ctx =>
            {
                joystick_direction = Vector2.zero;
                OnReset();
            };
    }
}