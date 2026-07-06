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
            if (AppManager.OpenFolderInWorlds(world_id)) { return; }
        }
        else if (AppManager.OpenFolderInWorlds(Path.Combine(world_id, rest_of_path))) { return; }

        // we failed to open the folder, maybe we have a single file world save, we open the main world folder instead
        AppManager.OpenWorldsFolder();
        return;
    }
}