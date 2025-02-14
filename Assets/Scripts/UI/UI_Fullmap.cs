using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class UI_Fullmap : MonoBehaviour {

    private Perso perso;
    private Minimap minimap;
    // is map shown
    private bool is_map_shown = false;

    private GameObject ui_map;
    private GameObject ui_mask;
    private GameObject hc;
    private GameObject uis;
    private UI_MainUI main_ui;

    [Header("Inputs")]
    [SerializeField] private PlayerInputActions inputs;

    [Header("Map Settings")]
    [SerializeField] private float move_speed = 0.1f;
    [SerializeField] private float zoom_speed = 0.1f;
    [SerializeField] private Vector2 zoom_limits = new Vector2(0.5f, 2f);

    // unity functions
    void Awake()
    {
        // on récupère le perso
        perso = GameObject.Find("/perso").GetComponent<Perso>();

        // on récupère la minimap
        minimap = GameObject.Find("/perso/minimap").GetComponent<Minimap>();

        // on récupère le main_ui
        main_ui = GameObject.Find("/ui").GetComponent<UI_MainUI>();

        // on récupère la map
        ui_map = transform.Find("mask/map").gameObject;
        ui_mask = transform.Find("mask/map_mask").gameObject;
        uis = transform.Find("mask").gameObject;
        hc = transform.Find("hc").gameObject;

        // on cache la map
        hide();
    }

    protected void Start()
    {
        // on récupère les inputs
        inputs = GameObject.Find("/utils/input_manager").GetComponent<InputManager>().inputs;
    }

    void Update()
    {
        if (minimap.is_init && ui_map.GetComponent<RawImage>().texture == null)
        {
            // on init la minimap
            init_fullmap();
        }

        // on vérifie qu'on a une texture sinon on return
        if (ui_map.GetComponent<RawImage>().texture == null) return;

        if (inputs != null && is_map_shown)
        {
            if (inputs.UI.navigate.ReadValue<Vector2>() != Vector2.zero)
            {
                move(inputs.UI.navigate.ReadValue<Vector2>());
            }

            if (inputs.UI.scroll.ReadValue<float>() != 0)
            {
                zoom(inputs.UI.scroll.ReadValue<float>());
            }
        }
    }

    public void init_fullmap()
    {
        // on récupère la texture
        ui_map.GetComponent<RawImage>().texture = minimap.mapTexture;
        // on met les bonnes dimensions
        ui_map.GetComponent<RawImage>().rectTransform.sizeDelta = new Vector2(minimap.mapTexture.width, minimap.mapTexture.height);

        // on récupère la texture
        ui_mask.GetComponent<RawImage>().texture = minimap.maskTexture;
        // on met les bonnes dimensions
        ui_mask.GetComponent<RawImage>().rectTransform.sizeDelta = new Vector2(minimap.maskTexture.width, minimap.maskTexture.height);

        print("(UI_Fullmap) textures récupérées");
    }


    // show/hide
    public void show()
    {
        // on cache le main_ui
        main_ui.show();
        main_ui.showOnly(gameObject);

        // on affiche la map
        is_map_shown = true;
        uis.SetActive(true);
        transform.Find("black").gameObject.SetActive(true);
        transform.Find("text").gameObject.SetActive(true);
        hc.SetActive(true);

        // on arrête le temps
        Time.timeScale = 0;

        // on lance les inputs
        // inputs.UI.Enable();
        // hc.GetComponent<HC>().OnEnable();
        /* inputs.UI.navigate.performed += ctx => move();
        inputs.UI.scroll.performed += ctx => zoom(); */
    }

    public void hide()
    {
        // on arrête les inputs
        if (inputs != null)
        {
            // inputs.UI.Disable();
            /* inputs.UI.navigate.performed -= ctx => move();
            inputs.UI.scroll.performed -= ctx => zoom(); */
        }


        // on cache la map
        is_map_shown = false;
        uis.SetActive(false);
        transform.Find("black").gameObject.SetActive(false);
        transform.Find("text").gameObject.SetActive(false);
        hc.SetActive(false);

        // on remet le temps
        Time.timeScale = 1;

        // on affiche le main_ui
        main_ui.show();
    }

    public void rollShow()
    {
        (is_map_shown ? (System.Action)hide : show)();
    }


    // move zoom
    private void move(Vector2 dir)
    {

        // on effectue le déplacement
        Vector2 movement = new Vector2(dir.x, dir.y);
        movement *= move_speed;
        // uis.transform.position -= movement;
        uis.GetComponent<RectTransform>().anchoredPosition -= movement;


        // on décale le pivot à la position du joueur (pour que le zoom soit centré)
        // float pivot_move_x = movement.x / (uis.transform.localScale.x*uis.GetComponent<RectTransform>().rect.width);
        // float pivot_move_y = movement.y / (uis.transform.localScale.y*uis.GetComponent<RectTransform>().rect.height);
        // uis.GetComponent<RectTransform>().pivot -= new Vector2(pivot_move_x, pivot_move_y);

    }

    private void zoom(float dir)
    {
        dir *= zoom_speed;
        uis.transform.localScale += new Vector3(dir, dir, 0);
        if (uis.transform.localScale.x < zoom_limits.x)
        {
            uis.transform.localScale = new Vector3(zoom_limits.x, zoom_limits.x, 0);
        }
        else if (uis.transform.localScale.x > zoom_limits.y)
        {
            uis.transform.localScale = new Vector3(zoom_limits.y, zoom_limits.y, 0);
        }
    }


    // getters

    public bool isShowed()
    {
        return is_map_shown;
    }
}