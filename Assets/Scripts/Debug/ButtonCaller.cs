using UnityEngine;

public class ButtonCaller : MonoBehaviour
{
    public void InsertTicketButton()
    {
        if (!UI_Manager.Instance.TryGetPool(out UI_DistributorPool pool)) { return; }
        pool.TryInsertTicket();
    }
}