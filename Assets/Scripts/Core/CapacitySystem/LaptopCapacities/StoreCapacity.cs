using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// capacity of a module:hdd
/// can store Files
/// </summary>
public class StoreCapacity : Capacity
{

    [Header("Store parameters")]
    public string DiskLetter = "c:";
    private int capacity = 4096; // in bytes (octets)
    public int Capacity => capacity;
    public int SpaceLeft => capacity - SpaceUsed;
    public int SpaceUsed => Files.Sum(file => file.Size);

    [Header("Files")]
    [SerializeField] private List<File> files = new List<File>();
    [SerializeField] private List<Key> keys = new List<Key>();
    [SerializeField] private List<Exploit> exploits = new List<Exploit>();



    // WRITING FILES
    public bool CanStore(File file)
    {
        return (SpaceLeft >= file.Size);
    }
    public bool Store(File file)
    {
        if (!CanStore(file)) { return false; }

        if (file is Key key)
        {
            keys.Add(key);
        }
        else if (file is Exploit exploit)
        {
            exploits.Add(exploit);
        }
        else
        {
            files.Add(file);
        }
        return true;
    }




    // GETTERS
    public List<File> Files => files.Concat(keys.Cast<File>()).Concat(exploits.Cast<File>()).ToList();
    public File GetFile(string fileName)
    {
        return Files.Find(file => file.name == fileName);
    }
    public List<Exploit> GetExploits()
    {
        return exploits;
    }
    public List<Key> GetKeys()
    {
        return keys;
    }

    // SETTERS
    public void SetCapacity(int new_capacity)
    {
        if (new_capacity < SpaceUsed)
        {
            Debug.LogWarning($"(StoreCapacity) cannot set disk capacity to {new_capacity} bytes, not enough space for existing files ({SpaceUsed} bytes used)");
            return;
        }
        capacity = new_capacity;
        if (log) { Debug.Log($"(StoreCapacity) {capable.name} disk capacity set to {capacity} bytes"); }
    }
}