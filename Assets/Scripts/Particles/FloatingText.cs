using UnityEngine;
using TMPro;
using System.Collections.Generic;


[RequireComponent(typeof(TMP_Text))]
public class FloatingText : MonoBehaviour {
    
    [Header("Parameters")]
    public float speed = 0.5f;
    public float fade_out_speed = 0.5f;
    public float ttl = 3f;
    [SerializeField] private float size = 30f;
    [SerializeField] private float scale = 0.1f;

    [Header("Resources")]
    private string materials_path = "materials/text/";
    private string font_name = "pixelrunner";


    [Header("Colors")]
    private static readonly Dictionary<string,Color> colors = new Dictionary<string, Color>
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
    [SerializeField] private TMP_Text text_mesh = null;
    public TMP_Text TextMesh
    {
        get
        {
            if (text_mesh == null) { text_mesh = GetComponent<TMP_Text>(); }
            return text_mesh;
        }}

    // INIT
    public void Init(string text, string color, float size = -1f, float speed = -1f, float fade_out_speed = -1f, float ttl = -1f)
    {
        // on initialise le texte & couleur et font size
        TextMesh.text = text;
        base_color = color;
        TextMesh.color = colors[color];
        TextMesh.fontSize = 1;

        // on ajuste le material
        AdjustMaterial();

        // ajustement de la taille
        this.size = (size == -1f) ? 50f : size;
        transform.localScale = new Vector3(scale, scale, scale) * this.size;

        // on initialise les variables
        this.speed = (speed == -1f) ? 0.5f : speed;
        this.fade_out_speed = (fade_out_speed == -1f) ? 0.5f : fade_out_speed;
        this.ttl = (ttl == -1f) ? 3f : ttl;
    }


    // UPDATING
    public void AdjustMaterial()
    {
        // on récupère le material
        string material_name = font_name + "_mat_" + base_color;
        Material material = Resources.Load<Material>(materials_path + material_name);

        // on le met
        TextMesh.fontMaterial = material;
    }
    public void SetTTL(float ttl)
    {
        this.ttl = ttl;
    }

}