using System.IO;
using UnityEngine;

public class WorldFolderCaller : MonoBehaviour
{
    public string world_id;
    public string rest_of_path;
    public void OpenWorldFolder()
    {
        if (rest_of_path == "")
        {
            AppManager.OpenFolderInWorlds(world_id);
            return;
        }
        AppManager.OpenFolderInWorlds(Path.Combine(world_id, rest_of_path));
    }
}