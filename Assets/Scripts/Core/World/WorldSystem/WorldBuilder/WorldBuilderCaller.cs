using UnityEngine;

public class WorldBuilderCaller : MonoBehaviour
{
    private LevelBuilder _worldBuilder;
    private LevelBuilder worldBuilder
    {
        get
        {
            if (_worldBuilder == null) { _worldBuilder = LevelBuilder.StaticInstance; }
            return _worldBuilder;
        }
    }


    // tools
    public void SelectTool(string tool_type) => worldBuilder.SelectTool(tool_type);


    // general builders
    public void BuildWorld() => worldBuilder.Build();
    public void EraseAll() => worldBuilder.Erase();
    public void ClearTilemaps() => worldBuilder.ClearTilemaps();
    public void SaveData() => worldBuilder.SaveData();

    // specifics builders
    public void Build(string builder) => worldBuilder.Build(builder);
}