using UnityEngine;
using System;
using System.Collections.Generic;

public class FileHolder : Item
{
    // items that can hold files

    [Header("File Holder")]
    [SerializeField] protected List<File> files = new List<File>();
    [SerializeField] protected int max_files = 1;
    [SerializeField] protected Transform root = null;
    [SerializeField] protected string root_name = "a/";

    // unity functions
    protected new void Awake()
    {
        base.Awake();

        // on change le type de l'item
        this.category = "fileholder";

        // on récupère le root
        root = transform.Find("root");
    }


    // FILE MANAGEMENT
    public void ClearFiles()
    {
        files.Clear();
    }

    public bool AddFile(File file)
    {
        // on vérifie qu'on peut ajouter un fichier
        if (files.Count < max_files)
        {
            // on ajoute le fichier
            files.Add(file);
            return true;
        }
        return false;
    }

    public bool RemoveFile(File file)
    {
        // on vérifie qu'on peut retirer un fichier
        if (files.Contains(file))
        {
            // on retire le fichier
            files.Remove(file);
            return true;
        }
        return false;
    }

    public List<File> GetFiles()
    {
        return files;
    }

}


[System.Serializable]
public class File
{
    // a file is NOT an item
    // it is a virtual object that can be found through different ways :
    // - in computers & others electronic devices
    // - in usb keys / cd-roms
    // - in your NOOD_OS

    [Header("File")]
    public string name = "nuclear_codes";
    public string extension = "";
    public string data = "0x6FF2A1C3";

    // CONSTRUCTOR
    public File(string name, string data = "", string extension="txt")
    {
        this.name = name;
        this.extension = extension;
        this.data = data;
    }

    // GETTERS
    public string GetName() { return name + (extension != "" ? "." + extension : ""); }
    // public string GetData() { return data; }

}