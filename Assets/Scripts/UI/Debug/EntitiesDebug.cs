using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class EntitiesDebug : MonoBehaviour, Debuggable
{
    // [SerializeField] TextMeshProUGUI debug_text;
    [SerializeField] private int capablesCount = 0; // all the capables in the world
    [SerializeField] private int corpsesCount = 0; // all the corpses in the world
    [SerializeField] private int beingsCount = 0; // all the beings in the world
    private Dictionary<string,int> beingsTypesCount = new Dictionary<string,int>(); // count of each type of beings
    [SerializeField] private int itemsCount = 0; // all the items in the world
    [SerializeField] private int grabbedItemsCount = 0; // all the grabbed items in the world


    // START
    private void Start()
    {
        DebugManager.Instance.AddDebuggable(this, "entities");
    }

    // ENTITY
    public void AddEntity(Capable capable)
    {
        capablesCount++;
        if (capable is Corpse) { corpsesCount++; }
        else if (capable is Being being) { AddBeing(being); }
        else if (capable is Item item) { AddItem(item); }
    }
    public void RemoveEntity(Capable capable)
    {
        capablesCount--;
        if (capable is Corpse) { corpsesCount--; }
        else if (capable is Being being) { RemoveBeing(being); }
        else if (capable is Item item) { RemoveItem(item); }
    }

    // BEINGS
    private void AddBeing(Being being)
    {
        beingsCount++;
        string type = being.GetType().Name;
        if (!beingsTypesCount.ContainsKey(type))
        {
            beingsTypesCount[type] = 0;
        }
        beingsTypesCount[type]++;
    }
    private void RemoveBeing(Being being)
    {
        beingsCount--;
        string type = being.GetType().Name;
        if (beingsTypesCount.ContainsKey(type))
        {
            beingsTypesCount[type]--;
            if (beingsTypesCount[type] <= 0)
            {
                beingsTypesCount.Remove(type);
            }
        }
    }

    // ITEMS
    private void AddItem(Item item)
    {
        itemsCount++;
        if (item.Grabbed) { grabbedItemsCount++; }

        item.OnGrabbed += (it, holder) => { grabbedItemsCount++; };
        item.OnDropped += (it) => { grabbedItemsCount--; };
    }
    private void RemoveItem(Item item)
    {
        itemsCount--;
        if (item.Grabbed) { grabbedItemsCount--; }
    }

    // DEBUGGABLE
    public string GetDebugText()
    {
        string text = "capables : " + capablesCount + "\n";
        text += ">>> corpses : " + corpsesCount + "\n";
        text += "\nitems : " + itemsCount + "\n";
        text += ">>> grabbed items : " + grabbedItemsCount + "\n";
        text += "\nbeings : " + beingsCount + "\n";
        foreach (KeyValuePair<string, int> entry in beingsTypesCount)
        {
            text += ">>> " + entry.Key.ToLower() + " : " + entry.Value + "\n";
        }
        return text;
    }
}