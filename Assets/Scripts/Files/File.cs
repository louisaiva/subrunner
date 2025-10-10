using UnityEngine;


[CreateAssetMenu(fileName = "File", menuName = "SO/File", order = 1)]
public class File : ScriptableObject
{
    // public new string name;
    public string extension = "";
    public string data;

    public virtual int Size => data.Length + name.Length + extension.Length;
}





