using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Numpad : MonoBehaviour, I_UI_Slot
{
    // hoover
    public bool is_hoovered { get; set; }
    public bool just_cliked = false;

    // ui_computer
    public GameObject ui_computer;
    
    // num
    public RectTransform num;
    public Vector3 base_num_pos;

    // slot sprite
    public Sprite base_sprite;
    public Sprite hoover_sprite;
    public Sprite clicked_sprite;

    // unity functions
    protected void Awake()
    {
        // on récupère les sprites
        Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheets/ui/numpad");

        // on vérifie si on est le zéro (height == 32)
        if (GetComponent<RectTransform>().rect.height == 32)
        {
            base_sprite = sprites[0];
            hoover_sprite = sprites[1];
            clicked_sprite = sprites[2];
        }
        else
        {
            base_sprite = sprites[3];
            hoover_sprite = sprites[4];
            clicked_sprite = sprites[5];
        }

        // on récupère l'ui_computer
        ui_computer = GameObject.Find("/ui/screen/ui_computer");

        // on récupère le num
        num = transform.Find("num").GetComponent<RectTransform>();
        base_num_pos = num.localPosition;
    }

    // getters
    public string getDescription()
    {
        /* string s = file.file_name + "\n\n" + file.file_description;
        return s; */
        return "";
    }

    public bool shouldDescriptionBeShown() { return false; }

    // interface functions
    public void OnPointerEnter(PointerEventData eventData)
    {
        // on change le sprite du slot
        if (!just_cliked) { GetComponent<Image>().sprite = hoover_sprite; }

        // on met à jour le fait qu'on est survolé
        is_hoovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // on change le sprite du slot
        if (!just_cliked) {GetComponent<Image>().sprite = base_sprite;}

        // on met à jour le fait qu'on est survolé
        is_hoovered = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (just_cliked) { return;}

        // Debug.Log("UI_Numpad : OnPointerClick : " + gameObject.name);

        // on change le sprite du slot
        GetComponent<Image>().sprite = clicked_sprite;

        // on descend le numéro
        // transform.Find("num").GetComponent<RectTransform>().localPosition += new Vector3(0, -1f, 0);
        num.localPosition = base_num_pos + new Vector3(0, -1f, 0);

        just_cliked = true;

        Invoke("resetSprite", 0.1f);

        
        ui_computer.GetComponent<UI_Computer>().clickOnNumpad(name);
        /*
        // on click
        {
            ui_computer.GetComponent<UI_computer>().clickOnFile(file);
        }
        else if (ui_computer.GetComponent<UI_ChestInventory>() != null)
        {
            ui_computer.GetComponent<UI_ChestInventory>().clickOnFile(file);
        }
        else
        {
            Debug.LogWarning("UI_File : no UI_computer or UI_ChestInventory found on " + ui_computer.name);
        } */
    }

    public void resetSprite()
    {
        num.localPosition = base_num_pos;

        // on change le sprite du slot
        GetComponent<Image>().sprite = is_hoovered ? hoover_sprite : base_sprite;

        just_cliked = false;
    }


    // reset hoover
    public void resetHoover()
    {
        if (!is_hoovered) return;

        OnPointerExit(null);
    }
}
