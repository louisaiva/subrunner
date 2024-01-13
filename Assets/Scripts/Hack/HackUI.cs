using UnityEngine;
using System.Linq;

[RequireComponent(typeof(SpriteRenderer))]
public class HackUI : MonoBehaviour
{

    [SerializeField] private string mode = "unhackable"; // "hackable", "hacked", "unhackable"

    [Header("HACKING COLORS & SPRITES")]
    [SerializeField] private Sprite sprite_hackable;
    [SerializeField] private Sprite sprite_unhackable;
    [SerializeField] private Color color_base;
    [SerializeField] private Color color_hacked;

    private float auto_hide_delay = 0.5f;
    
    // unity functions
    void Awake()
    {
        if (sprite_hackable == null) {
            sprite_hackable = Resources.LoadAll<Sprite>("spritesheets/ui/ui_hackables")[0];
        }
        if (sprite_unhackable == null) {
            sprite_unhackable = Resources.LoadAll<Sprite>("spritesheets/ui/ui_unhackables")[0];
        }
        
        // get the colors
        if (color_base == null) { color_base = new Color(1f, 1f, 1f, 1f); }
        if (color_hacked == null) { color_hacked = new Color(0f, 1f, 0f, 1f); }

        hide();

        setMode("unhackable");
    }

    // main functions
    public void setMode(string newMode, bool force = false)
    {
        // check if the mode is valid
        if (!(new string[] {"hackable", "hacked", "unhackable"}).Contains(newMode))
        {
            Debug.LogError("HackUI: setMode: mode " + newMode + " is not valid");
            return;
        }

        // verify if the mode is different
        if (newMode == mode && !force) { return; }

        // set the mode
        mode = newMode;

        // update the color
        if (mode == "hackable")
        {
            GetComponent<SpriteRenderer>().sprite = sprite_hackable;
            GetComponent<SpriteRenderer>().color = color_base;
        }
        else if (mode == "hacked")
        {
            GetComponent<SpriteRenderer>().sprite = sprite_hackable;
            GetComponent<SpriteRenderer>().color = color_hacked;
        }
        else if (mode == "unhackable")
        {
            GetComponent<SpriteRenderer>().sprite = sprite_unhackable;
            GetComponent<SpriteRenderer>().color = color_base;
        }
    }

    // showin
    public void hide()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    public void show()
    {
        CancelInvoke("hide");
        GetComponent<SpriteRenderer>().enabled = true;

        // on cache après un certain temps
        Invoke("hide", auto_hide_delay);
    }
}