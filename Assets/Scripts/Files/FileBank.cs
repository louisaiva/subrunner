using System.Collections.Generic;
using UnityEngine;

public class FileBank : Singleton<FileBank>
{
    public List<File> files = new List<File>();
    public List<File> keys = new List<File>();
    public List<File> programs = new List<File>();
    public List<File> exploits = new List<File>();

    public Exploit Nmap => (Exploit)exploits.Find(file => file.name == "nmap");
    public FileExploit TypePassword => (FileExploit)exploits.Find(file => file.name == "type_password");

    public T GetFile<T>(string name) where T : File
    {
        if (typeof(T) == typeof(Key))
        {
            return (T)keys.Find(file => file.name == name);
        }
        else if (typeof(T) == typeof(Exploit) || typeof(T) == typeof(FileExploit) || typeof(T) == typeof(DamageExploit))
        {
            return (T)exploits.Find(file => file.name == name);
        }
        else if (typeof(T) == typeof(Program))
        {
            return (T)programs.Find(file => file.name == name);
        }
        else
        {
            return (T)files.Find(file => file.name == name);
        }
    }
}