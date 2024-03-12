using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class FileBank : MonoBehaviour
{
    
    // this bank is used to store all the files in the game
    List<File> txt_files = new List<File>();
    List<File> png_files = new List<File>();
    List<File> exe_files = new List<File>();
    List<File> mp3_files = new List<File>();

    // hints
    List<File> txt_hints = new List<File>();

    // CONSTRUCTORS
    public void Awake()
    {
        // on load les files depuis le json "files.json"
        string json = Resources.Load<TextAsset>("json/files").text;
        // Dictionary<string, Dictionary<string,string>> files = JsonUtility.FromJson<Dictionary<string, Dictionary<string,string>>>(json);
        Dictionary<string, Dictionary<string, string>> files = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);

        txt_files = files["txt"].Select(f => new File(f.Key, f.Value, "txt")).ToList();
        png_files = files["png"].Select(f => new File(f.Key, f.Value, "png")).ToList();
        exe_files = files["exe"].Select(f => new File(f.Key, f.Value, "exe")).ToList();
        mp3_files = files["mp3"].Select(f => new File(f.Key, f.Value, "mp3")).ToList();

        // on load les blank hints
        string json_hints = Resources.Load<TextAsset>("json/files_hints").text;
        Dictionary<string, Dictionary<string, string>> hints = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json_hints);
        txt_hints = hints["txt"].Select(f => new File(f.Key, f.Value, "txt")).ToList();

    }

    // GETTERS
    public File getRandomFile(string ext="all")
    {
        if (ext == "all")
        {
            int r = Random.Range(0, 4);
            if (r == 0) { return getRandomFile("txt"); }
            if (r == 1) { return getRandomFile("png"); }
            if (r == 2) { return getRandomFile("exe"); }
            if (r == 3) { return getRandomFile("mp3"); }
        }

        File file = new File(".", "", "txt");

        if (ext == "txt" && txt_files.Count > 0)
        {
            int r = Random.Range(0, txt_files.Count);
            file = txt_files[r];
        }
        else if (ext == "png" && png_files.Count > 0)
        {
            int r = Random.Range(0, png_files.Count);
            file = png_files[r];
        }
        else if (ext == "exe" && exe_files.Count > 0)
        {
            int r = Random.Range(0, exe_files.Count);
            file = exe_files[r];
        }
        else if (ext == "mp3" && mp3_files.Count > 0)
        {
            int r = Random.Range(0, mp3_files.Count);
            file = mp3_files[r];
        }
        else
        {
            Debug.LogWarning("(FileBank) no file found for ext " + ext);
        }

        return file;
    }

    public List<File> getRandomFiles(int n, string ext="all")
    {
        List<File> files = new List<File>();
        for (int i = 0; i < n; i++)
        {
            files.Add(getRandomFile(ext));
        }
        return files;
    }

    // HINTS
    public File generateRandomTxtHint(string comp_name,string password)
    {
        if (txt_hints.Count == 0)
        {
            Debug.LogWarning("(FileBank) no hint found");
            return new File(".", "", "txt");
        }

        // on récupère un fichier txt hint random
        File base_hint = txt_hints[Random.Range(0, txt_hints.Count)];
        File hint = new File(base_hint.name, base_hint.data, base_hint.extension);

        // on change toutes les occurences de "<pass>" par le password et "<comp>" par le comp_name
        hint.data = hint.data.Replace("<pass>", password);
        hint.data = hint.data.Replace("<comp>", comp_name);
        hint.name = hint.name.Replace("<pass>", password);
        hint.name = hint.name.Replace("<comp>", comp_name);

        // on l'ajoute à la liste des files
        txt_files.Add(hint);

        // Debug.Log("(FileBank) generated hint : " + hint.name + " (" + hint.data +")");

        // on le retourne
        return hint;
    }
}