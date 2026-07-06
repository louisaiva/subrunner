using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class EntitiesDebug : MonoBehaviour, MultipleDebuggable
{
    [SerializeField] private int capablesCount = 0; // all the capables in the world
    private Dictionary<string, int> capablesTypesCount = new Dictionary<string, int>(); // count of each type of beings


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