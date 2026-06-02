using UnityEngine;

public class HackableDoor : Door, Lockable
{
    [Header("Lockable")]
    [SerializeField] private float locking_interval = 5f; // the time before the door is locked again after being unlocked
    public bool Locked { get; private set; } = true;
    [SerializeField] private Key key; // the key needed to hack this door
    public Key Key => key; // return the key
    public string Password => key.data; // return the key password


    [Header("Components")]
    private HoverCapacity hoverer;

    // START
    protected override void Start()
    {
        base.Start();

        // get the hover capacity
        hoverer = GetCapacity<HoverCapacity>();
        if (hoverer == null) { Debug.LogError($"(HackableDoor) {name} has no HoverCapacity component"); }

        // we play the idle_locked if we are locked
        if (Locked) { (Visual as AnimPlayer)?.Play("idle_locked"); }
    }

    // INTERACTION
    public override void OnInteract(Capable interactor)
    {
        if (!Locked) { base.OnInteract(interactor); return; }

        // we are locked, we check if interactor has a device with the right key (instant hack)
        if (interactor is Hacker hacker)
        {
            Device device = hacker.Laptop;
            if (device != null && device.HasKeyFor(this))
            {
                Unlock();
                base.OnInteract(interactor);
                return;
            }
        }

        // else we are locked we can't interact.
        if (log) { Debug.Log($"(HackableDoor) {interactor.name} tried to interact with locked door {name}"); }

        // if we have a hover capacity we try to update its animation (so it will reset the animation, which alert the player it's locked)
        hoverer.ChangeAnimation("hover_locked");
    }

    // UNLOCKING
    public async void Unlock()
    {
        Locked = false;
        if (log) { Debug.Log($"(HackableDoor) {name} is now unlocked"); }
        AnimPlayer anim_player = Visual as AnimPlayer;
        if (anim_player == null) { if (log) { Debug.LogError($"(HackableDoor) {name} has no AnimPlayer component"); } return; }

        // we play unlock animation
        anim_player.Play("unlock");

        // we update the hover animation to show a nice unlocked anim
        hoverer.ChangeAnimation("hover");

        // we stop playing idle_locked
        anim_player.StopPlaying("idle_locked");

        // we wait for the unlock animation to stop
        while (anim_player.IsShowing("unlock")) { await System.Threading.Tasks.Task.Yield(); }

        // we open the door
        open();

        // we invoke the locking after x seconds
        Invoke(nameof(Lock), locking_interval);
    }
    public void Lock()
    {
        CancelInvoke();
        Locked = true;
        if (log) { Debug.Log($"(HackableDoor) {name} is now locked"); }
        AnimPlayer anim_player = Visual as AnimPlayer;
        if (anim_player == null) { if (log) { Debug.LogError($"(HackableDoor) {name} has no AnimPlayer component"); } return; }


        // we play lock animation
        anim_player.Play("lock");

        // we update the hover animation to show the locked anim
        hoverer.ChangeAnimation("hover_locked");

        // we start to play idle_locked again
        anim_player.AddToPile("idle_locked");

        // if we are open we close ourselves
        if (is_open)
        {
            if (log) { Debug.Log($"(HackableDoor) {name} is open, closing it now"); }
            close();
        }
    }
}