using UnityEngine;
using System.Collections.Generic;

public interface I_FileHolder
{
    // propriétés
    List<File> files { get; set; }
    int max_files { get; set; }
    Transform root { get; set; }
    string root_name { get; set; }


    // fonctions
    List<File> GetFiles();
}