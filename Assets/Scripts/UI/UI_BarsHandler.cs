
using UnityEngine;
using UnityEngine.UI;

public class UI_BarsHandler : MonoBehaviour {

    private Perso perso;

    // sprites
    private string bars_sprites_path = "spritesheets/ui/ui_bars";
    private Sprite[] sprites;

    // bars_affichee
    private int nb_bits_affiches = 0;
    private int nb_octets_max = 0;

    // UI_BitsHandler
    public GameObject ui_bits_handler;

    // unity functions
    void Awake()
    {
        // on récupère le perso
        perso = GameObject.Find("/perso").GetComponent<Perso>();

        // on met à jour le nb d'octets max
        nb_octets_max = 13;

        // on récupère le UI_BitsHandler
        ui_bits_handler = transform.parent.transform.Find("bits_handler").gameObject;

        // on recup les sprites
        sprites = Resources.LoadAll<Sprite>(bars_sprites_path);
    }

    void Update()
    {
        // print(sprites);

        if (perso.hasCapacity("hoover_hack"))
        {
            // on active le UI_BitsHandler
            ui_bits_handler.SetActive(true);

            // on met à jour les barres
            updateBars();
        }
        else
        {
            // on désactive le UI_BitsHandler
            ui_bits_handler.SetActive(false);

            // on met à jour avec le sprite 0
            GetComponent<Image>().sprite = sprites[0];

            // on met à jour le nombre de bits affichés
            nb_bits_affiches = 0;
        }
    }

    // functions
    private void updateBars()
    {
        // on verifie si on a besoin de mettre à jour les barres
        if (nb_bits_affiches == perso.max_bits || nb_octets_max <= nb_bits_affiches/8) { return; }

        // on met à jour le nb max de bits affichés
        // nb_octets_max = sprites.Length - 1;

        // on met à jour les barres
        // max_bits == 0 => sprites[0]
        // max_bits == 8 => sprites[1]
        // max_bits == 16 => sprites[2]
        // ...

        int nb_octets = perso.max_bits / 8;
        if (perso.max_bits % 8 != 0) { nb_octets++; }

        // on vérifie qu'on a assez de sprites
        if (nb_octets > sprites.Length)
        {
            // on choisit le dernier sprite
            nb_octets = sprites.Length - 1;
            return;
        }

        // on met à jour l'image
        GetComponent<Image>().sprite = sprites[nb_octets];

        // on met à jour le nombre de bits affichés
        nb_bits_affiches = perso.max_bits;
    }

}