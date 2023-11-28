using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(AnimationHandler))]
public class Chest : MonoBehaviour
{

    // la classe CHEST sert à créer des coffres
    // peut contenir des objets physiques
    // pas obligatoirement alignée avec les tiles

    // mettre un champ de force qui attire les objets vers le coffre,
    // comme ça on peut juste drop les objets à côté du coffre

    // ANIMATION
    private AnimationHandler anim_handler;
    private ChestAnims anims = new ChestAnims();

    // OPENING
    public bool is_open = false;

    // STOCKAGE
    private int max_items = 4;
    List<Item> items = new List<Item>();


    // UI

    
    // unity functions
    void Start()
    {
        // on récupère l'animation handler
        anim_handler = GetComponent<AnimationHandler>();

        // on récupère l'ui
    }

    void Update()
    {
        // on met à jour les animations
        if (is_open)
        {
            // on regarde si on a fini l'animation
            if (anim_handler.IsForcing()) { return; }

            // on met à jour les animations
            anim_handler.ChangeAnim(anims.idle_open);
        }
        else
        {
            // on regarde si on a fini l'animation
            if (anim_handler.IsForcing()) { return; }

            // on met à jour les animations
            anim_handler.ChangeAnim(anims.idle_closed);
        }
    }

    // MAIN FUNCTIONS

    public void open()
    {
        // on met à jour les animations
        if (!anim_handler.ChangeAnimTilEnd(anims.openin)) { return; }

        // on ouvre le coffre
        is_open = true;
    }

    public void close()
    {
        // on arrête de forcer l'animation -> pour être sûr qu'on puisse fermer le coffre
        anim_handler.StopForcing();

        // on met à jour les animations
        if (!anim_handler.ChangeAnimTilEnd(anims.closin)) { return; }

        // on ferme le coffre
        is_open = false;
    }


    // STOCKAGE

    public bool addItem(Item item)
    {
        // on regarde si on peut ajouter l'item
        if (items.Count >= max_items) { return false; }

        // on ajoute l'item au coffre
        items.Add(item);

        // on met à jour l'UI
        // updateUI();

        return true;
    }

    public bool removeItem(Item item)
    {
        // on regarde si on peut retirer l'item
        if (!items.Contains(item)) { return false; }

        // on retire l'item du coffre
        items.Remove(item);

        // on met à jour l'UI
        // updateUI();

        return true;
    }

    // GETTERS

    public int getMaxItems()
    {
        return max_items;
    }

    public List<Item> getItems()
    {
        return items;
    }
}

public class ChestAnims
{

    // ANIMATIONS
    public string idle_open = "chest_idle_open";
    public string idle_closed = "chest_idle_closed";
    public string openin = "chest_openin";
    public string closin = "chest_closin";
    // public string hackin = "chest_hacked";

}