using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class EntitiesDebug : MonoBehaviour, MultipleDebuggable
{
    // [SerializeField] TextMeshProUGUI debug_text;
    [SerializeField] private int capablesCount = 0; // all the capables in the world
    // [SerializeField] private int corpsesCount = 0; // all the corpses in the world
    // [SerializeField] private int beingsCount = 0; // all the beings in the world
    private Dictionary<string,int> capablesTypesCount = new Dictionary<string,int>(); // count of each type of beings
    // [SerializeField] private int itemsCount = 0; // all the items in the world
    // [SerializeField] private int grabbedItemsCount = 0; // all the grabbed items in the world


    // START
    private void Start()
    {
        DebugManager.Instance.AddDebuggable(this, "entities");
    }

    // ENTITY
    public void AddCapable(Capable capable)
    {
        capablesCount++;
        // if (capable is Corpse) { corpsesCount++; }
        // else if (capable is Being being) { AddBeing(being); }
        // else if (capable is Item item) { AddItem(item); }

        string type = capable.GetType().Name;
        if (!capablesTypesCount.ContainsKey(type))
        {
            capablesTypesCount[type] = 0;
        }
        capablesTypesCount[type]++;
    }
    public void RemoveCapable(Capable capable)
    {
        capablesCount--;
        // if (capable is Corpse) { corpsesCount--; }
        // else if (capable is Being being) { RemoveBeing(being); }
        // else if (capable is Item item) { RemoveItem(item); }

        string type = capable.GetType().Name;
        if (!capablesTypesCount.ContainsKey(type)) { return; }
        capablesTypesCount[type]--;
        if (capablesTypesCount[type] <= 0)
        {
            capablesTypesCount.Remove(type);
        }
    }

    // BEINGS
    /* private void AddBeing(Being being)
    {
        beingsCount++;
        string type = being.GetType().Name;
        if (!capablesTypesCount.ContainsKey(type))
        {
            capablesTypesCount[type] = 0;
        }
        capablesTypesCount[type]++;
    }
    private void RemoveBeing(Being being)
    {
        beingsCount--;
        string type = being.GetType().Name;
        if (capablesTypesCount.ContainsKey(type))
        {
            capablesTypesCount[type]--;
            if (capablesTypesCount[type] <= 0)
            {
                capablesTypesCount.Remove(type);
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
    } */

    // DEBUGGABLE
    public string GetDebugText()
    {
        /* string text = "capables : " + capablesCount + "\n";
        text += ">>> corpses : " + corpsesCount + "\n";
        text += "\nitems : " + itemsCount + "\n";
        text += ">>> grabbed items : " + grabbedItemsCount + "\n";
        text += "\nbeings : " + beingsCount + "\n";
        foreach (KeyValuePair<string, int> entry in capablesTypesCount)
        {
            text += ">>> " + entry.Key.ToLower() + " : " + entry.Value + "\n";
        } */
        return string.Join("\n", GetDebugLines());
    }

    public List<string> GetDebugLines()
    {
        // we return a list of strings, one line for total capables count, and one for each type of capable with its count
        List<string> lines = new List<string>
        {
            "capables : " + capablesCount
        };
        foreach (KeyValuePair<string, int> entry in capablesTypesCount)
        {
            lines.Add(">>> " + entry.Key.ToLower() + " : " + entry.Value);
        }
        return lines;
    }
}