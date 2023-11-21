using UnityEngine;


public class CameraFollow : MonoBehaviour
{

    public GameObject player;

    public float timeOffset;

    private Vector3 velocity;
    public float Y_OFF;


    // unity functions

    void Update()
    {

        // X

        float final_x = player.transform.position.x;

        float x_movement = final_x - transform.position.x;

        // Y

        float final_y = player.transform.position.y + Y_OFF;
        float y_movement = final_y - transform.position.y;


        // on calcule la position finale de la cam, puis on la déplace
        Vector3 final_position = new Vector3(final_x, final_y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, final_position, ref velocity, timeOffset);

    }

    // useful functions

}