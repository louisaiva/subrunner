using UnityEngine;

public class SpriteBank : MonoBehaviour
{
    // handle the sprites

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
}