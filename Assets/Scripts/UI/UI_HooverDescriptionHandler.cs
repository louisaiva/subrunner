using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class UI_HooverDescriptionHandler : MonoBehaviour {
    

    // affichage
    private bool is_showed = false;
    private GameObject ui_bg;
    public Vector3 offset_vector = new Vector3(0, 0, 0);
    [SerializeField] private float keyboard_offset = 50f;
    [SerializeField] private float gamepad_offset = 100f;

    // perso
    private GameObject perso;

    // descriptable
    private I_Descriptable current_descriptable;

    // inputs
    private InputManager input_manager;
    private UI_XboxNavigator xbox_manager;

    // unity functions
    void Start()
    {
        // on récupère le ui_bg
        ui_bg = transform.Find("ui_bg").gameObject;

        // on récupère le perso
        perso = GameObject.Find("/perso");

        // on récupère l'input manager
        input_manager = GameObject.Find("/utils/input_manager").GetComponent<InputManager>();
        // on récupère le xbox_manager
        xbox_manager = GameObject.Find("/ui").GetComponent<UI_XboxNavigator>();
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
            Vector3 next_pos = new Vector3(0, 0, 0);

            float w = GetComponent<RectTransform>().sizeDelta.x - ui_bg.GetComponent<RectTransform>().offsetMax.x - ui_bg.GetComponent<RectTransform>().offsetMin.x;
            float h = GetComponent<RectTransform>().sizeDelta.y - ui_bg.GetComponent<RectTransform>().offsetMax.y - ui_bg.GetComponent<RectTransform>().offsetMin.y;

            // on met à jour la position
            if (input_manager.isUsingGamepad())
            {
                offset_vector = new Vector3(gamepad_offset, -h/2, 0);
                next_pos = (Vector3) xbox_manager.getCursorPosition() + offset_vector;
            }
            else
            {
                offset_vector = new Vector3(keyboard_offset, 0, 0);
                next_pos = Input.mousePosition + offset_vector;
            }

            // on vérifie qu'on ne sort pas de l'écran
            if (next_pos.x + w > Screen.width)
            {
                next_pos.x -= (w + offset_vector.x*2);
            }
            else if (next_pos.x < 0)
            {
                next_pos.x = 0;
            }
            if (next_pos.y + h> Screen.height)
            {
                next_pos.y = Screen.height - h;
            }
            else if (next_pos.y < 0)
            {
                next_pos.y = 0;
            }
            transform.position = next_pos;
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