using UnityEngine;

public class WorldNodeVisualizer : WorldCellVisualizer
{
    public bool IsPartOfRoom()
    {
        return WorldBuilder.LevelBuilder.GetRoomOfNode(this) != null;
    }
}