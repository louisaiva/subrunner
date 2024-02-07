using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(AnimationHandler),typeof(SpriteRenderer),typeof(Animator))]
public class GlowingItemEffect : MonoBehaviour
{

    private Perso perso;



    [Header("Animations")]
    private List<string> anims_apparitions = new List<string>
                        {
                            "legendary_item_apparition_1",
                            "legendary_item_apparition_2",
                            "legendary_item_apparition_3",
                            "legendary_item_apparition_4",
                            "legendary_item_apparition_5",
                            "legendary_item_apparition_6",
                            "legendary_item_apparition_7",
                            "legendary_item_apparition_8",
                        };
    private string anim_turning = "legendary_item_turning";
    private AnimationHandler animation_handler;


    [Header("Distance")]
    private float apparition_radius = 2f;
    private float glowing_radius = 0.5f;


    void Start()
    {
        perso = GameObject.Find("/perso").GetComponent<Perso>();
        animation_handler = GetComponent<AnimationHandler>();
    }

    void Update()
    {
        // on check la distance entre le perso et l'item
        float distance = Vector2.Distance(perso.transform.position, transform.position);

        // on vérifie déjà si on est pas méga loin
        if (distance > apparition_radius)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            return;
        }
        else if (!GetComponent<SpriteRenderer>().enabled)
        {
            GetComponent<SpriteRenderer>().enabled = true;
        }


        // on vérifie le glow
        if (distance < glowing_radius)
        {
            // on calcule la vitesse de rotation
            float cycle_duration = (distance / glowing_radius);

            // on joue l'animation de rotation
            animation_handler.ChangeAnim(anim_turning, cycle_duration);

        }
        else if (distance < apparition_radius)
        {
            // on divise la distance en 8
            float step = (1 - ((distance - glowing_radius) / (apparition_radius - glowing_radius)))*8f;

            // on joue l'animation d'apparition
            animation_handler.ChangeAnim(anims_apparitions[(int)step], 1f);
        }
    }


}