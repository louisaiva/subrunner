using UnityEngine;

public class ButtonCaller : MonoBehaviour
{
    public void InsertTicketButton()
    {
        if (!UI_Manager.Instance.TryGetPool(out UI_DistributorPool pool)) { return; }
        pool.TryInsertTicket();
    }
    public void CookMeal()
    {
        if (!UI_Manager.Instance.TryGetPool(out UI_OrdererPool pool)) { return; }
        pool.CookMeal();
    }
}