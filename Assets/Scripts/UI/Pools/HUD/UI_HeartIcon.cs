using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UI_HeartIcon : MonoBehaviour
{
    [SerializeField] private List<HeartSprite> heart_sprites; // from dead to full life
    [SerializeField] private Image heart_image;

    private void Update()
    {
        if (Controller.Perso == null) { return; }
        if (!Controller.Perso.TryGetCapacity<HealthCapacity>(out var health_capacity)) { return; }

        // on récupère le pourcentage de vie actuel du perso
        float life_percentage = health_capacity.LifePourcent;

        // on trouve le sprite correspondant dans la liste
        foreach (HeartSprite heart_sprite in heart_sprites)
        {
            if (heart_sprite.life_percentage_threshold >= life_percentage)
            {
                heart_image.sprite = heart_sprite.sprite;
            }
            else { break; }
        }
    }
}

[Serializable] public class HeartSprite
{
    public Sprite sprite;
    public float life_percentage_threshold;
}