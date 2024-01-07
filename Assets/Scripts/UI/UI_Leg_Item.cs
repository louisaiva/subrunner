using UnityEngine;

public class UI_Leg_Item : UI_Item
{

    // unity functions
    protected new void Start()
    {
        base.Start();

        // on récupère les sprites
        Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheets/item_slots");
        base_sprite = sprites[2];
        hoover_sprite = sprites[3];
    }
}