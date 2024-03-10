using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_File : MonoBehaviour, I_UI_Slot
{

 
    // hoover
    public bool is_hoovered { get; set; }

    // file
    public File file;
    public Transform ui;

    // ui_computer
    public GameObject ui_computer;

    // slot sprite
    public Sprite base_sprite;
    public Sprite hoover_sprite;

    // file sprite
    public Sprite txt_sprite;
    public Sprite png_sprite;
    public Sprite exe_sprite;
    public Sprite mp3_sprite;


    // unity functions
    protected void Awake()
    {

        // on récupère l'ui
        ui = transform.Find("file");

        // on récupère les sprites
        Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheets/ui/ui_file_slots");
        base_sprite = sprites[0];
        hoover_sprite = sprites[1];

        // on récupère les sprites des files
        Sprite[] file_sprites = Resources.LoadAll<Sprite>("spritesheets/ui/ui_files");
        txt_sprite = file_sprites[0];
        mp3_sprite = file_sprites[1];
        exe_sprite = file_sprites[2];
        png_sprite = file_sprites[3];
    }

    // main functions
    public void setFile(File file)
    {
        if (ui == null) { Awake(); }

        this.file = file;

        // on change le sprite
        if (file.extension == "txt")
        {
            ui.GetComponent<Image>().sprite = txt_sprite;
        }
        else if (file.extension == "mp3")
        {
            ui.GetComponent<Image>().sprite = mp3_sprite;
        }
        else if (file.extension == "exe")
        {
            ui.GetComponent<Image>().sprite = exe_sprite;
        }
        else if (file.extension == "png")
        {
            ui.GetComponent<Image>().sprite = png_sprite;
        }
        else
        {
            ui.GetComponent<Image>().sprite = txt_sprite;
        }
    }

    public void setUIComputer(GameObject ui_computer)
    {
        this.ui_computer = ui_computer;
    }

    // getters
    public string getDescription()
    {
        /* string s = file.file_name + "\n\n" + file.file_description;
        return s; */
        return "";
    }

    public bool shouldDescriptionBeShown() {return false;}

    // interface functions
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("UI_File : OnPointerEnter : " + gameObject.name);

        // on change le sprite du slot
        GetComponent<Image>().sprite = hoover_sprite;

        // on met à jour le fait qu'on est survolé
        is_hoovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // on change le sprite du slot
        GetComponent<Image>().sprite = base_sprite;

        // on met à jour le fait qu'on est survolé
        is_hoovered = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // on change le sprite du slot
        GetComponent<Image>().sprite = base_sprite;

        // on click
        /* if (ui_computer.GetComponent<UI_computer>() != null)
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

    // reset hoover
    public void resetHoover()
    {
        if (!is_hoovered) return;

        OnPointerExit(null);
    }
}