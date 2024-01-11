using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HackUI : MonoBehaviour
{

    [Header("COLORS")]
    public Color color_hackable;
    public Color color_hacked;
    public Color color_unhackable;
    
    // unity functions
    void Start()
    {
        // get the colors
        color_hackable = new Color(0f, 0.5f, 0f, 0.8f);
        color_hacked = new Color(0f, 1f, 0f, 1f);
        color_unhackable = new Color(0.5f, 0f, 0f, 0.8f);

        hide();
    }

    // main functions
    public void 

    // showin
    public void hide()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    public void show()
    {
        GetComponent<SpriteRenderer>().enabled = true;
    }
}