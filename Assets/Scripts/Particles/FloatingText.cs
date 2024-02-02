using UnityEngine;
using TMPro;
using System.Collections.Generic;


public class FloatingText : MonoBehaviour {
    
    // mouvement
    [SerializeField] private float speed = 0.5f;

    // disparition
    [SerializeField] private float fade_out_speed = 0.5f;

    // time to live
    [SerializeField] private float ttl = 3f;

    // size
    [SerializeField] private float size = 30f;

    // materials path
    private string materials_path = "materials/text/";
    private string font_name = "pixelrunner";


    // colors
    private Dictionary<string,Color> colors = new Dictionary<string, Color>
    {
        {"red", Color.red},
        {"green", Color.green},
        {"blue", Color.blue},
        {"yellow", Color.yellow},
        {"white", Color.white},
        {"black", Color.black},
        {"cyan", Color.cyan},
        {"magenta", Color.magenta},
        {"grey", Color.grey},
        {"brown", new Color(0.5f, 0.25f, 0)},
        {"pink", new Color(1, 0.5f, 0.5f)},
        {"orange", new Color(1, 0.5f, 0)},
        {"purple", new Color(0.5f, 0, 1)}
    };

    private string base_color = "white";

    // unity functions

    public void init(string text, string color, float size=-1f, float speed = -1f, float fade_out_speed = -1f, float ttl = -1f)
    {
        base_color = color;

        // on initialise le texte
        GetComponent<TextMeshPro>().text = text;
        GetComponent<TextMeshPro>().color = colors[color];
        GetComponent<TextMeshPro>().fontSize = 1;

        // on ajuste le material
        ajustMaterial();

        // ajustement de la taille
        if (size != -1f) { this.size = size; }
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f) * this.size;

        // on initialise les variables
        if (speed != -1f) { this.speed = speed; }
        if (fade_out_speed != -1f) { this.fade_out_speed = fade_out_speed; }
        if (ttl != -1f) { this.ttl = ttl; }
        /* this.speed = speed;
        this.fade_out_speed = fade_out_speed;
        this.ttl = ttl; */
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

        string material_name = font_name + "_mat_" + base_color;

        /* if (color == Color.red)
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
        else if (color == Color.black)
        {
            material_name += "black";
        }
        else if (color == Color.cyan)
        {
            material_name += "cyan";
        }
        else if (color == Color.magenta)
        {
            material_name += "magenta";
        }
        else if (color == Color.grey)
        {
            material_name += "grey";
        }
        else if (color == 
        {
            material_name += "brown";
        }
        else if (color == new Color(1, 0.5f, 0.5f))
        {
            material_name += "pink";
        }
        else if (color == new Color(1, 0.5f, 0))
        {
            material_name += "orange";
        }
        else if (color == new Color(0.5f, 0, 1))
        {
            material_name += "purple";
        }
        else
        {
            material_name += "white";
        } */


        // on récupère le material
        Material material = Resources.Load<Material>(materials_path + material_name);

        // on l'ajuste
        GetComponent<TextMeshPro>().fontMaterial = material;
    }


    public void setTTL(float ttl)
    {
        this.ttl = ttl;
    }

}