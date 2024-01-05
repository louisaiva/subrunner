using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class UI_Minimap : MonoBehaviour {

    private Perso perso;
    private Minimap minimap;

    // sprites
    private string mini_map_sprites_path = "spritesheets/ui/ui_minimap";
    private Sprite[] sprites;

    // is map shown
    private bool is_map_shown = false;
    [SerializeField] private Vector2 OFFSET = new Vector2(0f, 0f);


    // 
    private GameObject ui_map;
    private GameObject ui_mask;

    private GameObject uis;

    // unity functions
    void Awake()
    {
        // on récupère le perso
        perso = GameObject.Find("/perso").GetComponent<Perso>();

        // on récupère la minimap
        minimap = GameObject.Find("/perso/minicam").GetComponent<Minimap>();

        // on recup les sprites
        sprites = Resources.LoadAll<Sprite>(mini_map_sprites_path);

        // on récupère la map
        ui_map = transform.Find("mask/map").gameObject;
        ui_mask = transform.Find("mask/map_mask").gameObject;
        uis = transform.Find("mask").gameObject;
    }

    void Update()
    {
    
        // on récupère l'area_name au niveau du perso
        // string area_name = minimap.getPersoAreaName();

        // on met à jour le texte de l'area
        // transform.Find("area_name").GetComponent<TextMeshProUGUI>().text = area_name;

        // on met à jour la local tile pos
        // transform.Find("tile_pos").GetComponent<TextMeshProUGUI>().text = minimap.getPersoAreaPos() + " " + minimap.getPersoTilePos();


        // on vérifie si on a la minimap
        if (!minimap.is_init)
        {
            // on cache la map
            disableMiniMap();
            return;
        }
        else if (ui_map.GetComponent<RawImage>().texture == null)
        {
            // on init la minimap
            init_minimap();
        }


        bool gyro = perso.hasCapacity("gyroscope");

        // on vérifie si on a le gyroscope
        if (gyro && !is_map_shown)
        {
            // on active la map
            enableMiniMap();
        }
        else if (!gyro && is_map_shown)
        {
            disableMiniMap();
        }

        // si on a pas de gyroscope, on anime l'image
        if (!gyro)
        {
            // on anime l'image
            GetComponent<Image>().sprite = sprites[(int)(Time.time * 10) % 2];
        }
        else
        {
            // on met à jour la position
            update_position();
        }


    }

    void update_position()
    {

        // update la position de la minimap
        Vector2 perso_tile_pos = minimap.getPersoPosFloat();

        Vector2 map_size = new Vector2(minimap.mapTexture.width, minimap.mapTexture.height);

        // on met à jour l'offset
        OFFSET = new Vector2(12,12);

        // on calcule la différence entre le milieu de la map et le perso
        Vector2 pos = perso_tile_pos * ui_map.GetComponent<RawImage>().rectTransform.localScale.x;

        // on met à jour la position
        ui_map.GetComponent<RectTransform>().anchoredPosition = -pos + OFFSET;
        ui_mask.GetComponent<RectTransform>().anchoredPosition = -pos + OFFSET;
    }

    // functions
    public void enableMiniMap()
    {
        // cancel invoke
        CancelInvoke("turnOffMap");

        // on active la map
        GetComponent<Image>().enabled = true;
        is_map_shown = true;

        // on met à jour le sprite
        GetComponent<Image>().sprite = sprites[2];

        // on met à jour la minimap
        // ui_map.GetComponent<RawImage>().enabled = true;
        // ui_mask.GetComponent<RawImage>().enabled = true;
        uis.SetActive(true);
    }

    public void disableMiniMap()
    {
        // on désactive la map
        is_map_shown = false;

        // on met à jour le sprite
        GetComponent<Image>().sprite = sprites[0];

        // on met à jour la minimap
        // ui_map.GetComponent<RawImage>().enabled = false;
        // ui_mask.GetComponent<RawImage>().enabled = false;
        uis.SetActive(false);

        // on désactive la map
        Invoke("turnOffMap", 0.5f);
    }

    private void turnOffMap()
    {
        if (!is_map_shown)
        {
            GetComponent<Image>().enabled = false;
        }
    }

    // init
    public void init_minimap()
    {
        // on récupère la texture
        ui_map.GetComponent<RawImage>().texture = minimap.mapTexture;
        // on met les bonnes dimensions
        ui_map.GetComponent<RawImage>().rectTransform.sizeDelta = new Vector2(minimap.mapTexture.width, minimap.mapTexture.height);

        // on récupère la texture
        ui_mask.GetComponent<RawImage>().texture = minimap.maskTexture;
        // on met les bonnes dimensions
        ui_mask.GetComponent<RawImage>().rectTransform.sizeDelta = new Vector2(minimap.maskTexture.width, minimap.maskTexture.height);

        print("textures récupérées");
    }
}