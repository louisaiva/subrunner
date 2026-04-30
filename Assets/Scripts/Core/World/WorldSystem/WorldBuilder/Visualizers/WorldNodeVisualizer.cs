using UnityEngine;

public class WorldNodeVisualizer : WorldCellVisualizer
{
    public bool IsPartOfRoom()
    {
        return LevelBuilder.StaticInstance.GetRoomOfNode(this) != null;
    }
}