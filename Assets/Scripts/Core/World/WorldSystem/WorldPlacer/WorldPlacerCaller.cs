using UnityEngine;

public class WorldPlacerCaller : MonoBehaviour
{
    public void EnableGrid() => WorldPlacer.LazyInstance.ActivateGrid();
    public void DisableGrid() => WorldPlacer.LazyInstance.DeactivateGrid();
}