using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UI_Minimap : MonoBehaviour {

    private Perso perso;

    // sprites
    private string mini_map_sprites_path = "spritesheets/ui/ui_minimap";
    private Sprite[] sprites;

    // is map shown
    private bool is_map_shown = false;

    // unity functions
    void Awake()
    {
        // on récupère le perso
        perso = GameObject.Find("/perso").GetComponent<Perso>();

        // on recup les sprites
        sprites = Resources.LoadAll<Sprite>(mini_map_sprites_path);
    }

    void Update()
    {
        // on vérifie si on a le gyroscope
        if (perso.has_gyroscope && !is_map_shown)
        {
            // on active la map
            enableMiniMap();
        }
        else if (!perso.has_gyroscope && is_map_shown)
        {
            disableMiniMap();
        }

        // si on a pas de gyroscope, on anime l'image
        if (!perso.has_gyroscope)
        {
            // on anime l'image
            GetComponent<Image>().sprite = sprites[(int)(Time.time * 10) % 2];
        }
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
        transform.Find("map").GetComponent<RawImage>().enabled = true;
    }

    public void disableMiniMap()
    {
        // on désactive la map
        // GetComponent<Image>().enabled = false;
        is_map_shown = false;

        // on met à jour le sprite
        GetComponent<Image>().sprite = sprites[0];

        // on met à jour la minimap
        transform.Find("map").GetComponent<RawImage>().enabled = false;

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

}