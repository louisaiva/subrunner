using UnityEngine;

/// <summary>
/// this script is the main script for handling camera movement.
/// It can work with any capable, or with a SimpleCameraController
/// </summary>
public class CameraFollow : Singleton<CameraFollow>
{
    [Header("Simple Camera Controller Target")]
    private SimpleCameraController simple_target;
    [SerializeField] private bool using_simple_target = false;

    [Header("Capable Target")]
    [SerializeField] private Capable target;
    [SerializeField] private Rigidbody2D capable_rb;

    public float timeOffset;
    private Vector3 velocity;


    [Header("Dynamic camrea")]
    [SerializeField] private bool dynamic_cam = false;
    public float Y_OFF;
    [SerializeField] private float min_velocity = 0.5f;
    [SerializeField] private float Y_OFF_MAX = 1.5f;
    [SerializeField] private float Y_OFF_SPEED = 0.5f;

    [Header("Size")]
    [SerializeField] private float default_size = 3f;
    private float target_size = 3f;

    private void Start()
    {
        simple_target = FindFirstObjectByType<SimpleCameraController>(FindObjectsInactive.Include);
    }

    // SIMPLE TARGET ENABLER
    public void EnableSimpleController() { using_simple_target = true; }
    public void DisableSimpleController() { using_simple_target = false; }
    public void ResetSimpleControllerToCenter()
    {
        if (simple_target == null) { return; }
        simple_target.transform.position = new Vector3(0, 0, simple_target.transform.position.z);        
    }

    // CAPABLE TARGET SETTER
    public void ChangeCapableTarget(Capable new_target, bool tp = false)
    {
        if (new_target == null) { return; }
        target = new_target;
        capable_rb = target.GetComponent<Rigidbody2D>();
        if (tp) { transform.position = new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z); }
        // if not tp we will smoothly lerp to the new target in the update loop
    }

    // UPDATE
    private void Update()
    {
        // applique le zoom
        if (Camera.main.orthographicSize != target_size)
        {
            float next_size = Mathf.Lerp(Camera.main.orthographicSize, target_size, Time.deltaTime * 5f);
            if (Mathf.Abs(next_size - target_size) < 0.01f) { next_size = target_size; }
            Camera.main.orthographicSize = next_size;
        }

        if (using_simple_target) { lerp_to_simple_target(); }
        else if (Controller.Capable != null) { lerp_to_capable(); }
    }
    private void lerp_to_simple_target()
    {
        if (simple_target == null) { return; }
        Vector3 final_position = new Vector3(simple_target.transform.position.x, simple_target.transform.position.y + Y_OFF, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, final_position, ref velocity, timeOffset);
    }
    private void lerp_to_capable()
    {
        if (Controller.Capable == null || target == null || Controller.LazyInstance.PIC.InputsDisabled) { capable_rb = null; target = null; return; }

        // calcule le mouvement de la cam en X
        float final_x = target.transform.position.x;
        float x_movement = final_x - transform.position.x;


        // on ajuste l'offset en fonction de la vitesse du joueur en Y
        if (dynamic_cam && capable_rb != null)
        {
            Y_OFF = 0;
            if (Mathf.Abs(capable_rb.linearVelocity.y) > min_velocity)
            {
                Y_OFF = (capable_rb.linearVelocity.y - Mathf.Sign(capable_rb.linearVelocity.y) * min_velocity) * Y_OFF_SPEED;
                Y_OFF = Mathf.Clamp(Y_OFF, -Y_OFF_MAX, Y_OFF_MAX);
            }
        }


        // calcule le mouvement de la cam en Y
        float final_y = target.transform.position.y + Y_OFF;
        float y_movement = final_y - transform.position.y;


        // on calcule la position finale de la cam, puis on la déplace
        Vector3 final_position = new Vector3(final_x, final_y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, final_position, ref velocity, timeOffset);
    }

    // SIZE SETTER
    public void SetSize(float size) { target_size = size;}
    public void ResetSize() { SetSize(default_size); }
    public float GetSize() { return target_size; }
    public float GetSizeRelativeToDefault() { return target_size / default_size; }
}