using UnityEngine;

public class WorldBuilderCaller : MonoBehaviour
{
    private WorldBuilder _worldBuilder;
    private WorldBuilder worldBuilder
    {
        get
        {
            if (_worldBuilder == null) { _worldBuilder = WorldBuilder.Instance; }
            return _worldBuilder;
        }
    }


    public void BuildWorld() => worldBuilder.Build();
    public void EraseAll() => worldBuilder.Erase();
    public void ClearTilemaps() => worldBuilder.ClearTilemaps();
    public void SaveData() => worldBuilder.SaveData();

    // specifics builders
    public void Build(string builder) => worldBuilder.Build(builder);
}