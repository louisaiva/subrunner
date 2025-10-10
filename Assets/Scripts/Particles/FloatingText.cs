using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;


public class FloatingText : MonoBehaviour {
    
    [Header("Parameters")]
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private float fade_out_speed = 0.5f;
    [SerializeField] private float ttl = 3f;
    [SerializeField] private float size = 30f;
    [SerializeField] private float scale = 0.1f;

    [Header("Resources")]
    private string materials_path = "materials/text/";
    private string font_name = "pixelrunner";


    [Header("Colors")]
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
    [SerializeField] private string base_color = "white";


    [Header("Components")]
    [SerializeField] private TMP_Text text_mesh;

    [Header("Logs")]
    [SerializeField] private bool initialised_read_only = false;
    [SerializeField] private bool log = false;

    private void Start()
    {
        if (text_mesh == null)
        {
            Debug.LogError("FloatingText: no text mesh found");
            return;
        }

        if (text_mesh.text != "") 
        {
            init(text_mesh.text, base_color);
        }
    }

    // INIT
    public void init(string text, string color, float size = -1f, float speed = -1f, float fade_out_speed = -1f, float ttl = -1f)
    {
        if (initialised_read_only) { if (log) { Debug.LogWarning("FloatingText: already initialised"); } return; }

        initialised_read_only = true;
        base_color = color;

        if (text_mesh == null)
        {
            text_mesh = GetComponent<TMP_Text>();
            if (text_mesh == null) { Debug.LogError("FloatingText: no text mesh found"); }
        }

        // on initialise le texte
        text_mesh.text = text;
        text_mesh.color = colors[color];
        text_mesh.fontSize = 1;

        // on ajuste le material
        ajustMaterial();

        // ajustement de la taille
        if (size != -1f) { this.size = size; }
        transform.localScale = new Vector3(scale, scale, scale) * this.size;

        // on initialise les variables
        if (speed != -1f) { this.speed = speed; }
        if (fade_out_speed != -1f) { this.fade_out_speed = fade_out_speed; }
        if (ttl != -1f) { this.ttl = ttl; }
    }


    // UPDATING
    void Update()
    {
        // on fait disparaitre le texte
        ttl -= Time.deltaTime;
        if (ttl < 0) { Destroy(gameObject); }

        // on fait monter le texte
        transform.position += new Vector3(0, speed * Time.deltaTime, 0);

        // on fait disparaitre le texte
        Color color = text_mesh.color;
        color.a -= fade_out_speed * Time.deltaTime;
        text_mesh.color = color;
    }
    public void ajustMaterial()
    {
        // on récupère le material
        string material_name = font_name + "_mat_" + base_color;
        Material material = Resources.Load<Material>(materials_path + material_name);

        // on le met
        text_mesh.fontMaterial = material;
    }
    public void setTTL(float ttl)
    {
        this.ttl = ttl;
    }

}