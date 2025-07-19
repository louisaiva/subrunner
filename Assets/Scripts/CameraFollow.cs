using UnityEngine;


public class CameraFollow : MonoBehaviour
{

    private Rigidbody2D perso_rb;

    public float timeOffset;
    private Vector3 velocity;


    [Header("Dynamic camrea")]
    [SerializeField] private bool dynamic_cam = false;
    public float Y_OFF;
    [SerializeField] private float min_velocity = 0.5f;
    [SerializeField] private float Y_OFF_MAX = 1.5f;
    [SerializeField] private float Y_OFF_SPEED = 0.5f;

    // UPDATE
    void Update()
    {
        if (Perso.Instance == null) { perso_rb = null; return; }
        if (perso_rb == null) { perso_rb = Perso.Instance.GetComponent<Rigidbody2D>(); }


        // calcule le mouvement de la cam en X
        float final_x = perso_rb.transform.position.x;
        float x_movement = final_x - transform.position.x;


        // on ajuste l'offset en fonction de la vitesse du joueur en Y
        if (dynamic_cam)
        {
            Y_OFF = 0;
            if (Mathf.Abs(perso_rb.linearVelocity.y) > min_velocity)
            {
                Y_OFF = (perso_rb.linearVelocity.y - Mathf.Sign(perso_rb.linearVelocity.y)*min_velocity) * Y_OFF_SPEED;
                Y_OFF = Mathf.Clamp(Y_OFF, -Y_OFF_MAX, Y_OFF_MAX);
            }
        }
            

        // calcule le mouvement de la cam en Y
        float final_y = perso_rb.transform.position.y + Y_OFF;
        float y_movement = final_y - transform.position.y;


        // on calcule la position finale de la cam, puis on la déplace
        Vector3 final_position = new Vector3(final_x, final_y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, final_position, ref velocity, timeOffset);

    }
}