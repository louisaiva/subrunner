using UnityEngine;
using TMPro;

public class EntitiesDebug : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI debug_text;
    [SerializeField] private int beingsCount = 0;
    [SerializeField] private int capablesCount = 0; // capables but not beings !!! a fridge would go there but not the player

    // ENTITY
    public void AddEntity(Capable capable)
    {
        if (capable is Being) { AddBeing(); }
        else { AddCapable(); }
    }
    public void RemoveEntity(Capable capable)
    {
        if (capable is Being) { RemoveBeing(); }
        else { RemoveCapable(); }
    }

    // BEINGS
    private void AddBeing()
    {
        beingsCount++;
        refresh();
    }
    private void RemoveBeing()
    {
        if (beingsCount <= 0) { return; } // we can't remove a being if there are none

        beingsCount--;
        refresh();
    }

    // CAPABLES
    private void AddCapable()
    {
        capablesCount++;
        refresh();
    }
    private void RemoveCapable()
    {
        if (capablesCount <= 0) { return; } // we can't remove a being if there are none

        capablesCount--;
        refresh();
    }

    // REFRESH TEXT
    private void refresh()
    {
        string text = "E-N-T-I-T-I-E-S";
        text += "\n" + (beingsCount > 0 ? beingsCount : "no") + " entities in the world";
        text += "\n" + (capablesCount > 0 ? capablesCount : "no") + " objects in the world";

        debug_text.text = text;
    }
}