using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UI_Minimap : MonoBehaviour {

    private Perso perso;
    private Minimap minimap;

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

        // on récupère la minimap
        minimap = GameObject.Find("/perso/minicam").GetComponent<Minimap>();

        // on recup les sprites
        sprites = Resources.LoadAll<Sprite>(mini_map_sprites_path);
    }

    void Update()
    {
        if (!minimap.is_init) return;
        else if (transform.Find("map").GetComponent<RawImage>().texture == null)
        {
            // on récupère la texture
            transform.Find("map").GetComponent<RawImage>().texture = minimap.mapTexture;

            // on parcourt la texture pour voir ce qu'il y a
            string str = "";
            for (int y = 0; y < minimap.mapTexture.height; y++)
            {
                for (int x = 0; x < minimap.mapTexture.width; x++)
                {
                    // on récupère la couleur
                    Color color = minimap.mapTexture.GetPixel(x, y);

                    // on ajoute le type
                    str += x.ToString() + " " + y.ToString() + " " + color + "\n";
                }
                str += "\n\n";
            }

            print("texture récupérée\n\n" + str);
        }

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