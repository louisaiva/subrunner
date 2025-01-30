using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_BitHandler : MonoBehaviour
{

    // GAMEOBJECTS
    public GameObject perso;
    public GameObject bits_fill;
    public GameObject bit_prefab;

    // bit filling bar
    private float bit_fill_max_height = 104;
    private float bit_fill_width = 16;
    private Sprite[] bit_fill_sprites;

    // bit grid
    float off_x = 24f;
    float off_y = -16f;
    float ecart_x = 16f;
    float ecart_y = -16f;


    void Start()
    {
        // on récupère le perso
        perso = GameObject.Find("/perso");

        // on récupère le fill des bits
        bits_fill = transform.Find("bits_fill").gameObject;

        // on récupère le prefab des bits
        bit_prefab = Resources.Load("prefabs/ui/bit") as GameObject;

        // on desactive le fill des bits si le perso n'a pas encore obtenu d'bit
        if (perso.GetComponent<Perso>().max_bits == 0)
        {
            bits_fill.SetActive(false);
        }

        // on récupère les sprites du fill des bits
        Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheets/ui/ui_colors");
        bit_fill_sprites = new Sprite[4];
        bit_fill_sprites[0] = sprites[2];
        bit_fill_sprites[1] = sprites[3];
        bit_fill_sprites[2] = sprites[4];
        bit_fill_sprites[3] = sprites[5];
    }

    // Update is called once per frame
    void Update()
    {

        // ! on check si le perso est mort
        if (!perso || !perso.GetComponent<Perso>().Alive) { return; }

        // on met à jour le fill des bits
        update_bits_fill();

        // on met à jour les bits
        update_bits();

    }

    void update_bits_fill()
    {

        // on récupère les infos du perso
        int max_bits = perso.GetComponent<Perso>().max_bits;
        float bits = perso.GetComponent<Perso>().bits;

        // on met à jour la taille du fill
        float fill_percent = bits / max_bits;
        float fill_height = bit_fill_max_height * fill_percent;

        // on met à jour la taille du fill
        bits_fill.GetComponent<RectTransform>().sizeDelta = new Vector2(bit_fill_width, fill_height);

        // on met à jour le sprite du fill
        if (fill_percent > 0.5f)
        {
            bits_fill.GetComponent<Image>().sprite = bit_fill_sprites[0];
        }
        else if (fill_percent > 0.25f)
        {
            bits_fill.GetComponent<Image>().sprite = bit_fill_sprites[1];
        }
        else if (fill_percent > 0.125f)
        {
            bits_fill.GetComponent<Image>().sprite = bit_fill_sprites[2];
        }
        else
        {
            bits_fill.GetComponent<Image>().sprite = bit_fill_sprites[3];
        }
    }

    void update_bits()
    {

        // on récupère les infos du perso
        int bits = (int)Mathf.Floor(perso.GetComponent<Perso>().bits);

        // on récupère le nombre d'bits actuellement affichés
        int nb_bits_affiches = transform.childCount - 1;

        // on met à jour le nombre d'bits affichés
        if (bits > nb_bits_affiches)
        {

            // on ajoute des bits
            for (int i = nb_bits_affiches; i < bits; i++)
            {
                // on instancie un bit
                GameObject bit = Instantiate(bit_prefab, transform);

                // on le place
                bit.transform.localPosition = getbitPosition(i);

                // on le nomme
                bit.name = "bit_" + i.ToString();
            }
        }
        else if (bits < nb_bits_affiches)
        {

            // on supprime des bits
            for (int i = nb_bits_affiches - 1; i >= bits; i--)
            {
                // on supprime l'bit
                Destroy(transform.GetChild(i + 1).gameObject);
            }
        }

    }

    Vector3 getbitPosition(int i)
    {

        int c = i / 8; // colonne
        int l = i % 8; // ligne

        float x = off_x + c * ecart_x;
        float y = off_y + l * ecart_y;

        if (c == 0) { y -= off_y; }

        return new Vector3(x, y, 0f);
    }
}
