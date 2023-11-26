using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour {
    
    // mouvement
    private float speed = 0.5f;

    // disparition
    private float fade_out_speed = 0.5f;

    // time to live
    private float ttl = 3f;

    // unity functions
    
    void Update()
    {
        // on fait disparaitre le texte
        ttl -= Time.deltaTime;
        if (ttl < 0) { Destroy(gameObject); }

        // on fait monter le texte
        transform.position += new Vector3(0, speed * Time.deltaTime, 0);

        // on fait disparaitre le texte
        Color color = GetComponent<TextMeshPro>().color;
        color.a -= fade_out_speed * Time.deltaTime;
        GetComponent<TextMeshPro>().color = color;
    }

}