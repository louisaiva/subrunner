using System;
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
    [SerializeField] private string IF_gamepad_path = "spritesheets/ui/inputs/ui_interactables";
    [SerializeField] private string IF_kb_path = "spritesheets/ui/inputs/input_kb_feedback";
    public Dictionary<string, Sprite> IF_base_sprites = new Dictionary<string, Sprite>();
    public Dictionary<string, Sprite> IF_clicked_sprites = new Dictionary<string, Sprite>();

    // GET INPUT FEEDBACK SPRITE
    private void init_input_feedback_sprites()
    {
        // on récupère les sprites du gamepad
        Sprite[] sprites = Resources.LoadAll<Sprite>(IF_gamepad_path);

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

        // on récupère les sprites du gamepad
        sprites = Resources.LoadAll<Sprite>(IF_kb_path);

        IF_base_sprites.Add("key_dark", sprites[0]);
        IF_clicked_sprites.Add("key_dark", sprites[1]);
        IF_base_sprites.Add("key_light", sprites[2]);
        IF_clicked_sprites.Add("key_light", sprites[3]);
    }
    public Sprite GetInputFeedbackSprite(string key, bool empty = true)
    {
        if (empty)
        {
            if (!IF_base_sprites.ContainsKey(key))
            {
                if (log) { Debug.LogWarning($"(SpriteBank) The key '{key}' does not exist in the input feedback sprites"); }
                return null;
            }
            return IF_base_sprites[key];
        }

        if (!IF_clicked_sprites.ContainsKey(key))
        {
            if (log) { Debug.LogWarning($"(SpriteBank) The key '{key}' does not exist in the input feedback clicked sprites"); }
            return null;
        }

        return IF_clicked_sprites[key];
    }

    [Header("Key Feedback Icons")]
    [SerializeField] private List<Sprite> key_feedback_icons = new List<Sprite>();
    [SerializeField] private List<string> key_feedback_keys = new List<string>();

    // GET KEY FEEDBACK ICON
    public Sprite GetKeyFeedbackIcon(string key_reference)
    {
        int index = key_feedback_keys.IndexOf(key_reference);
        if (index == -1 || index >= key_feedback_icons.Count) { return null; }
        return key_feedback_icons[index];
    }


    [Header("Logs")]
    [SerializeField] private bool log = false;
}