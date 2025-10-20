using System.Collections.Generic;
using UnityEngine;

public class FileBank : Singleton<FileBank>
{
    public List<File> files = new List<File>();
    public List<File> keys = new List<File>();
    public List<File> programs = new List<File>();
    public List<File> exploits = new List<File>();

    [Header("File Icons")]
    public Sprite file_icon;
    public Sprite key_icon;
    public Sprite program_icon;
    public Sprite exploit_icon;
    public Sprite scan_icon;

    public Exploit Nmap => (Exploit)exploits.Find(file => file.name == "nmap");
    public FileExploit TypePassword
    {
        get
        {
            FileExploit exploit = ScriptableObject.CreateInstance<FileExploit>();
            exploit.name = "type_password";
            exploit.data = "this program simply types a password from a .key file. won't work if you don't have the file for the selected target. fastest way to unlock things tho.";
            exploit.targets = "anything that requires a password, doors, computers, etc.";
            exploit.file = null;
            exploit.icon = key_icon;
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