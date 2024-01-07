using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class UI_HooverDescriptionHandler : MonoBehaviour {
    

    // affichage
    private bool is_showed = false;
    private GameObject ui_bg;
    public Vector3 offset_vector = new Vector3(0, 0, 0);

    // perso
    private GameObject perso;

    // descriptable
    private I_Descriptable current_descriptable;

    // unity functions
    void Start()
    {
        // on récupère le ui_bg
        ui_bg = transform.Find("ui_bg").gameObject;

        // on récupère le perso
        perso = GameObject.Find("/perso");
    }

    void Update()
    {
        // on regarde si on est en train de survoler un objet
        if (current_descriptable != null && current_descriptable.shouldDescriptionBeShown())
        {
            if (!is_showed)
            {
                // on affiche la description
                is_showed = true;
                ui_bg.SetActive(true);
                GetComponent<TextMeshProUGUI>().enabled = true;
            }
        }
        else if (is_showed)
        {
            // on cache la description
            is_showed = false;
            ui_bg.SetActive(false);
            GetComponent<TextMeshProUGUI>().enabled = false;
        }

        if (is_showed)
        {
            // on met à jour la position à la position de la souris
            transform.position = Input.mousePosition + offset_vector;
        }
    }

    public void changeDescription(I_Descriptable item)
    {
        // on récupère le texte
        string text = item.getDescription();

        // on met à jour le texte
        GetComponent<TextMeshProUGUI>().text = text;

        // on met à jour la taille du bg
        float w = 200;
        float h = (text.Length / 10 + 1) * 30;
        GetComponent<RectTransform>().sizeDelta = new Vector2(w,h);

        current_descriptable = item;
    }

    public void removeDescription(I_Descriptable item)
    {
        // on regarde si on est en train de survoler l'objet
        if (current_descriptable == item)
        {
            // on cache la description
            current_descriptable = null;
        }
    }

    public void removeAllDescriptions()
    {
        // on cache la description
        current_descriptable = null;
    }
}