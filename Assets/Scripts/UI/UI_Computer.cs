using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Computer : MonoBehaviour, I_UI_Slottable
{

    [Header("UI Layers")]
    [SerializeField] private bool is_showed = false;
    [SerializeField] private GameObject screen_bg;
    [SerializeField] private Transform files_parent;
    [SerializeField] private Transform details_parent;
    private UI_MainUI main_ui;


    [Header("Computer")]
    [SerializeField] private Computer computer;

    [Header("Files")]
    [SerializeField] private GameObject file_prefab;
    [SerializeField] private List<GameObject> files = new List<GameObject>();

    [Header("Slottable")]
    [SerializeField] private Vector2 base_position = new Vector2(0, 10000);
    [SerializeField] private float angle_threshold = 45f;
    [SerializeField] private float angle_multiplicator = 100f;


    [Header("Inputs")]
    [SerializeField] private UI_XboxNavigator xbox_manager;
    [SerializeField] private InputManager input_manager;

    // unity functions
    void Start()
    {
        // on récupère les ui
        screen_bg = transform.Find("bg").gameObject;
        files_parent = transform.Find("files");
        details_parent = transform.Find("details");

        // on récupère le main_ui
        main_ui = GameObject.Find("/ui").GetComponent<UI_MainUI>();

        // on récupère le prefab des files
        file_prefab = Resources.Load("prefabs/ui/ui_file") as GameObject;

        // on récupère le xbox_manager
        xbox_manager = GameObject.Find("/ui").GetComponent<UI_XboxNavigator>();

        // on récupère l'input_manager
        input_manager = GameObject.Find("/utils/input_manager").GetComponent<InputManager>();

        // on cache l'inventaire
        hide();
    }

    // SHOWING
    public void show()
    {

        // on vérifie qu'on a un computer
        if (computer == null) { return; }

        // on cache le main_ui
        main_ui.show();
        main_ui.showOnly(transform.parent.gameObject);

        // on affiche l'ui
        is_showed = true;

        // on affiche les ui
        screen_bg.SetActive(true);
        transform.Find("text").gameObject.SetActive(true);
        files_parent.gameObject.SetActive(true);
        // hc.SetActive(true);

        // on active le xbox_manager
        if (input_manager.isUsingGamepad()) { xbox_manager.enable(this); }
    }

    public void hide()
    {
        // on désactive le xbox_manager
        xbox_manager.disable();

        // on cache l'ui
        is_showed = false;

        // on cache les ui
        screen_bg.SetActive(false);
        transform.Find("text").gameObject.SetActive(false);
        files_parent.gameObject.SetActive(false);
        // hc.SetActive(false);

        // on affiche le main_ui
        main_ui.show();
    }

    public void rollShow()
    {
        // on affiche l'inventaire
        if (is_showed) { hide(); }
        else { show(); }
    }

    public bool isShowed()
    {
        return is_showed;
    }


    // MAIN FUNCTIONS
    public void setComputer(Computer computer)
    {

        // on met à jour le computer
        if (this.computer != computer)
        {
            Clear();

            // on créee les files
            List<File> files = computer.GetFiles();
            foreach (File file in files)
            {
                GameObject file_ui = Instantiate(file_prefab, files_parent);
                file_ui.GetComponent<UI_File>().setFile(file);
                file_ui.GetComponent<UI_File>().setUIComputer(gameObject);

                // on donne un nom
                file_ui.name = file.name + "." + file.extension + " (" + Random.Range(0, 1000) + ")";
                this.files.Add(file_ui);
            }
        }


        this.computer = computer;
    }

    public void Clear()
    {
        Debug.Log("UI_Computer : Clear");

        // on supprime les files
        foreach (Transform child in files_parent)
        {
            Destroy(child.gameObject);
        }
        files.Clear();

        // on supprime les details
        foreach (Transform child in details_parent)
        {
            Destroy(child.gameObject);
        }

        // on supprime le computer
        computer = null;
    }


    // SLOTTABLE
    public List<GameObject> GetSlots(ref Vector2 base_position, ref float angle_threshold, ref float angle_multiplicator)
    {

        // on met à jour les seuils
        angle_threshold = this.angle_threshold;
        angle_multiplicator = this.angle_multiplicator;

        // on met à jour la position de base
        base_position = this.base_position;

        if (files.Count > 0)
        {
            // on met à jour la position de base
            Debug.Log("UI_Computer : GetSlots : " + files.Count + " slots found, with first element : " + files[0].name);
        }

        return files;
    }

}