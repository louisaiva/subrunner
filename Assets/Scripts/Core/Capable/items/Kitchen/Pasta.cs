using System.Collections;
using UnityEngine;

public class Pasta : Item
{

    public override void OnInteract(Capable interactor)
    {
        if (Reference != "food:pastas") { base.OnInteract(interactor); return; }
        if (interactor != Controller.Capable) { base.OnInteract(interactor); return; }
        
        // todo : here we can launch the final cutscene !
        Debug.Log($"(Pasta) {ID} was interact by {interactor.ID} :D");
    }

    public void LoadHover()
    {
        // Debug.Log($"(Pasta) {ID} Loading capacities : {string.Join(" ", dynamic_capacity_ids)}");
        if (CapableEngine.Instance.log_loading_extended) { Debug.Log($"(Item - OnDropped) {data.id} loading capacities : {string.Join(" ", dynamic_capacity_ids)}"); }
        CapacityEngine.Instance?.LoadCapacities(dynamic_capacity_ids, this);

        if (!TryGetCapacity(out HoverCapacity hc)) { Debug.LogError($"(Pasta) {ID} has no hover !!!"); return; }
        hc.HoverCollider.name = "pasta_collider";

        this._grabbed = false;
    }

}