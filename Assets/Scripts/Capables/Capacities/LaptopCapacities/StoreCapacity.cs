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
    [SerializeField] private int max_files = 10;
    public int MaxFiles => max_files;

    [Header("Files")]
    [SerializeField] private List<File> files = new List<File>();
    [SerializeField] private List<Key> keys = new List<Key>();
    [SerializeField] private List<Exploit> exploits = new List<Exploit>();
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
}

[Serializable]
public class File
{
    public string name;
    public string extension = "";
    public string data;
}