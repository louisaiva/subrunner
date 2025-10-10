using System.Collections.Generic;
using UnityEngine;

public class FileBank : Singleton<FileBank>
{
    public List<File> files = new List<File>();
    public List<File> keys = new List<File>();
    public List<File> programs = new List<File>();
    public List<File> exploits = new List<File>();

    public Exploit Nmap => (Exploit)exploits.Find(file => file.name == "nmap");
    public FileExploit TypePassword
    {
        get
        {
            FileExploit exploit = ScriptableObject.CreateInstance<FileExploit>();
            exploit.name = "type_password";
            exploit.file = null;
            exploit.cores_cost = 1;
            exploit.base_duration = 0.1f;
            exploit.security_level = 1;
            return exploit;
        }
    }

    public T GetFile<T>(string name) where T : File
    {
        if (typeof(T) == typeof(Key))
        {
            return (T)keys.Find(file => file.name == name);
        }
        else if (typeof(T) == typeof(Exploit) || typeof(T) == typeof(FileExploit) || typeof(T) == typeof(DamageExploit)
                 || typeof(T) == typeof(WaitExploit) || typeof(T) == typeof(TimerExploit))
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