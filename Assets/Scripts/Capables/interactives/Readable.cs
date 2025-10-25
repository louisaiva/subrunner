using UnityEngine;

public class Readable : Capable, Interactable
{
    [SerializeField] private GameObject ui_readable_prefab = null;

    public InteractCapacity Interactor => null;
    public InteractType InteractionType => InteractType.Other;
    public void OnInteract(Capable interactor)
    {
        // we show the ui_paper pool
        (UI_Manager.Instance.GetPool("paper") as UI_Paper).SetReadable(ui_readable_prefab);
        UI_Manager.Instance.StackPool("paper");
    }
}