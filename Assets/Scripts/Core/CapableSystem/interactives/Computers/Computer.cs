using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Computer : StaticDevice, Interactable, Onnable
{

    [Header("On / Off parameters")]
    [SerializeField] private bool is_on = false;
    [SerializeField] private bool is_moving = false;
    public bool IsOn { get => is_on; set => is_on = value; }
    public bool IsMoving { get => is_moving; set => is_moving = value; }
    public float power_off_delay = 5f;

    // INTERACTABLE
    public InteractCapacity Interactor => interactors.FirstOrDefault()?.GetCapacity<InteractCapacity>();
    public InteractType InteractionType => InteractType.Device;
    [SerializeField] private List<Capable> interactors = new List<Capable>(); // store all interactors, not just the one controlled

    // START
    protected virtual void Start()
    {
        IsOn = false;
        IsMoving = false;

        // we subscribe to the hover events
        GetCapacity<HoverCapacity>().OnHoverLost += OnHoverLost;
    }

    // ON INTERACT / HOVER LOST
    public void OnInteract(Capable interactor)
    {
        // only first interaction per interactor is authorized !!!
        if (interactors.Contains(interactor)) { return; }
        interactors.Add(interactor);
        if (log) { Debug.Log("(Computer) " + name + " was interacted by " + interactor.name); }

        // we power on if it's the first interactor we have !!
        if (interactors.Count == 1) { GetCapacity<OnOffCapacity>().PowerOn(); }
    }
    public void OnHoverLost(Capable interactor)
    {
        if (!interactors.Contains(interactor)) { return; }
        interactors.Remove(interactor);

        // if there is no more interactor we close the computer
        if (interactors.Count == 0) { GetCapacity<OnOffCapacity>().PowerOff(power_off_delay); }
    }

}