
using System;
using UnityEngine;

public interface Detector
{
    public T FindClosestCapableOfType<T>(CapableData looker_data) where T : Capable;
}
public interface FoodDetector : Detector
{
    public Food FindClosestFood(CapableData looker_data, EatData edata);
}
public interface TrashDetector : Detector
{
    public ItemData FindClosestTrash(CapableData looker_data, bool force_loaded = false);
    public ItemData FindClosestInteractableTrash(CapableData looker_data, InteractData idata, bool force_loaded = false);
}
public interface HealthDetector : Detector
{
    public HealthCapacity FindClosestHealthCapacity(IAData looker_data);
}

// [Serializable] public class SerializedCapableTarget
// {
//     public string capable_id;
//     public Vector2 position;
//     private Capable _loaded_capable;
//     public Capable LoadedTarget
//     {
//         get
//         {
//             // check if we already have a loaded capable and if it's still valid
//             try
//             {
//                 if (_loaded_capable != null && _loaded_capable.gameObject != null) { return _loaded_capable; }
//             }
//             catch (MissingReferenceException)
//             {
//                 // the loaded capable has been destroyed, we set it to null
//                 _loaded_capable = null;
//             }

//             // we first check if the capable is loaded in the bank
//             if (CapableBank.Instance == null) { return null; }
//             _loaded_capable = CapableBank.Instance.GetLoadedCapable(capable_id);
//             if (_loaded_capable != null) { return _loaded_capable; }

//             // else we check if this is an outsider
//             if (CapableSystem.Instance != null && CapableSystem.Instance.TryGetOutsider(capable_id, out Capable outsider))
//             {
//                 _loaded_capable = outsider;
//                 return _loaded_capable;
//             }

//             // we got nothing (maybe unloaded capable or wrong id)
//             return null;
//         }
//     }

//     // CONSTRUCTORS
//     public SerializedCapableTarget(Capable capable)
//     {
//         if (capable == null || capable.data == null) { return; }
//         capable_id = capable.data.id;
//         position = capable.transform.position;
//         _loaded_capable = capable;
//     }

//     public void SetTarget(Capable capable)
//     {
//         if (capable == null || capable.data == null) { return; }
//         this.capable_id = capable.data.id;
//         this.position = capable.transform.position;
//         this._loaded_capable = capable;
//     }
// }