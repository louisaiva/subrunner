using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour {
    
    // mouvement
    private float speed = 0.5f;

    // disparition
    private float fade_out_speed = 0.5f;

    // time to live
    private float ttl = 3f;

    // materials path
    private string materials_path = "materials/text/";
    private string font_name = "pixelrunner";

    // unity functions

    public void init(string text, Color color, float size, float speed = 0.5f, float fade_out_speed = 0.5f, float ttl = 3f)
    {
        // on initialise le texte
        GetComponent<TextMeshPro>().text = text;
        GetComponent<TextMeshPro>().color = color;
        GetComponent<TextMeshPro>().fontSize = 1;

        // on ajuste le material
        ajustMaterial();

        // ajustement de la taille
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f) * size;

        // on initialise les variables
        this.speed = speed;
        this.fade_out_speed = fade_out_speed;
        this.ttl = ttl;
    }

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

    public void ajustMaterial()
    {
        // on récupère la couleur actuelle
        Color color = GetComponent<TextMeshPro>().color;

        string material_name = font_name + "_mat_";

        if (color == Color.red)
        {
            material_name += "red";
        }
        else if (color == Color.green)
        {
            material_name += "green";
        }
        else if (color == Color.blue)
        {
            material_name += "blue";
        }
        else if (color == Color.yellow)
        {
            material_name += "yellow";
        }
        else if (color == Color.white)
        {
            material_name += "white";
        }
        else
        {
            return;
        }

        // on récupère le material
        Material material = Resources.Load<Material>(materials_path + material_name);

        // on l'ajuste
        GetComponent<TextMeshPro>().fontMaterial = material;
    }

}