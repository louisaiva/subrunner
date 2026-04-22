using UnityEngine;

public class WorldNodeVisualizer : WorldCellVisualizer
{
    public bool IsPartOfRoom()
    {
        return WorldBuilder.StaticInstance.GetRoomOfNode(this) != null;
    }
}