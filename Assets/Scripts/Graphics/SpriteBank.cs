using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This class is used to store specific sprites for the game.
/// useful for the damage collider, UI, etc.
/// </summary>
public class SpriteBank : MonoBehaviour
{
    // START
    private void Awake()
    {
        init_input_feedback_sprites();
    }


    [Header("Damage Sprites")]
    public Sprite[] damage_sprites;

    // HAS DAMAGE COLLIDER
    public bool HasDamageCollider(Sprite sprite)
    {
        // we check if the sprite has a damage collider
        foreach (Sprite s in damage_sprites)
        {
            if (s == sprite) { return true; }
        }
        return false;
    }

    
    [Header("Input Feedback Sprites")]
    public Dictionary<string, Sprite> IF_base_sprites = new Dictionary<string, Sprite>();
    public Dictionary<string, Sprite> IF_clicked_sprites = new Dictionary<string, Sprite>();

    // GET INPUT FEEDBACK SPRITE
    private void init_input_feedback_sprites()
    {
        // on récupère les sprites
        Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheets/ui/ui_interactables");

        // on les ajoute aux dictionnaires
        IF_base_sprites.Add("y", sprites[0]);
        IF_clicked_sprites.Add("y", sprites[1]);
        IF_base_sprites.Add("b", sprites[2]);
        IF_clicked_sprites.Add("b", sprites[3]);
        IF_base_sprites.Add("a", sprites[4]);
        IF_clicked_sprites.Add("a", sprites[5]);
        IF_base_sprites.Add("x", sprites[6]);
        IF_clicked_sprites.Add("x", sprites[7]);

        // IF_base_sprites.Add("joy", sprites[8]);
        IF_base_sprites.Add("joy", sprites[9]);
        IF_clicked_sprites.Add("joyU", sprites[10]);
        IF_clicked_sprites.Add("joyUR", sprites[11]);
        IF_clicked_sprites.Add("joyR", sprites[12]);
        IF_clicked_sprites.Add("joyDR", sprites[13]);
        IF_clicked_sprites.Add("joyD", sprites[14]);
        IF_clicked_sprites.Add("joyDL", sprites[15]);
        IF_clicked_sprites.Add("joyL", sprites[16]);
        IF_clicked_sprites.Add("joyUL", sprites[17]);

        IF_base_sprites.Add("U", sprites[18]);
        IF_clicked_sprites.Add("U", sprites[19]);
        IF_base_sprites.Add("R", sprites[20]);
        IF_clicked_sprites.Add("R", sprites[21]);
        IF_base_sprites.Add("D", sprites[22]);
        IF_clicked_sprites.Add("D", sprites[23]);
        IF_base_sprites.Add("L", sprites[24]);
        IF_clicked_sprites.Add("L", sprites[25]);

        IF_base_sprites.Add("LT", sprites[26]);
        IF_clicked_sprites.Add("LT", sprites[27]);
        IF_base_sprites.Add("RT", sprites[28]);
        IF_clicked_sprites.Add("RT", sprites[29]);
        IF_base_sprites.Add("RB", sprites[30]);
        IF_clicked_sprites.Add("RB", sprites[31]);
        IF_base_sprites.Add("LB", sprites[32]);
        IF_clicked_sprites.Add("LB", sprites[33]);

        IF_base_sprites.Add("start", sprites[34]);
        IF_clicked_sprites.Add("start", sprites[35]);
        IF_base_sprites.Add("select", sprites[36]);
        IF_clicked_sprites.Add("select", sprites[37]);


        // on ajoute les sprites du keyboard
        IF_base_sprites.Add("keyboard", sprites[38]);
        IF_clicked_sprites.Add("keyboard", sprites[39]);
        IF_base_sprites.Add("space", sprites[40]);
        IF_clicked_sprites.Add("space", sprites[41]);
    }

    public Sprite GetInputFeedbackSprite(string key, bool empty=true)
    {
        if (empty) { return IF_base_sprites[key]; }
        else { return IF_clicked_sprites[key]; }
    }

}