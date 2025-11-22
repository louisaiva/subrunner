using UnityEngine;


[CreateAssetMenu(fileName = "File", menuName = "SO/File", order = 1)]
public class File : ScriptableObject, Descriptable
{
    // public new string name;
    public string extension = "";
    public string data;
    public Sprite icon;

    public virtual int Size => data.Length + name.Length + extension.Length;

    // DESCRIPTABLE
    public string Name => name + extension;
    public virtual string Description => data;
}





