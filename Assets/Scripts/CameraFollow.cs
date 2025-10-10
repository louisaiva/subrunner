using UnityEngine;


public class CameraFollow : Singleton<CameraFollow>
{

    [SerializeField] private Capable target;
    [SerializeField] private Rigidbody2D capable_rb;
    private Capable capable => Controller.Instance.Capable;

    public float timeOffset;
    private Vector3 velocity;


    [Header("Dynamic camrea")]
    [SerializeField] private bool dynamic_cam = false;
    public float Y_OFF;
    [SerializeField] private float min_velocity = 0.5f;
    [SerializeField] private float Y_OFF_MAX = 1.5f;
    [SerializeField] private float Y_OFF_SPEED = 0.5f;

    public void RefreshTarget(Capable new_target)
    {
        if (new_target == null) { return; }
        target = new_target;
        capable_rb = target.GetComponent<Rigidbody2D>();
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    // UPDATE
    void Update()
    {
        // if (Perso.Instance == null) { capable_rb = null; return; }
        if (capable == null || target == null || Controller.Instance.PIC.InputsDisabled) { capable_rb = null; target = null; return; }


        // calcule le mouvement de la cam en X
        float final_x = target.transform.position.x;
        float x_movement = final_x - transform.position.x;


        // on ajuste l'offset en fonction de la vitesse du joueur en Y
        if (dynamic_cam && capable_rb != null)
        {
            Y_OFF = 0;
            if (Mathf.Abs(capable_rb.linearVelocity.y) > min_velocity)
            {
                Y_OFF = (capable_rb.linearVelocity.y - Mathf.Sign(capable_rb.linearVelocity.y)*min_velocity) * Y_OFF_SPEED;
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
}