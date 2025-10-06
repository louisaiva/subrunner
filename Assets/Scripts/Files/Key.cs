using UnityEngine;


[CreateAssetMenu(fileName = "Key", menuName = "SO/Key", order = 6)]
public class Key : File
{
    // public string key_type; // SHA, AES, RSA

    public bool Matches(string target_key)
    {
        return data == target_key;
    }
}