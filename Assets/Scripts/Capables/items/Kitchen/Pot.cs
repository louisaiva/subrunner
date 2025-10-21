using UnityEngine;
using System;
using System.Collections;


public class Pot : Item, Usable
{
    [Header("Components")]
    private InteractCapacity interactor = null;
    private event Action<Item> OnUsabilityChanged = delegate { };
    private UI_InventoryMenu inventory_menu = null;

    protected override void Awake()
    {
        base.Awake();
        interactor = GetCapacity<InteractCapacity>();
        interactor.OnHoverSelect += on_interactor_hover;
        interactor.OnHoverDeselect += on_interactor_unhover;
    }
    protected override void Start()
    {
        base.Start();
        inventory_menu = UI_Manager.Instance.GetPool("inventory") as UI_InventoryMenu;
    }

    // UPDATE LABEL & USABILITY
    private void on_interactor_hover(Capable interactable)
    {
        // we check if we are hovering an oven
        if (interactable is Oven oven)
        {
            UseLabel = "heat pot";
            usable_now = true;
            OnUsabilityChanged?.Invoke(this);
            return;
        }

        if (interactable is Sink)
        {
            UseLabel = "fill pot";
            usable_now = true;
            OnUsabilityChanged?.Invoke(this);
            return;
        }

        if (interactable is Pasta)
        {
            UseLabel = "put pasta in pot";
            usable_now = true;
            OnUsabilityChanged?.Invoke(this);
            return;
        }

        if (!usable_now) { return; }
        on_interactor_unhover(interactable);
    }
    private void on_interactor_unhover(Capable interactable)
    {
        // we reset the use label and usability
        UseLabel = "";
        usable_now = false;
        OnUsabilityChanged?.Invoke(this);
    }

    // USABLE
    public string UseLabel { get; set; } = "";
    public bool usable_now = false;
    public void Use(Capable user)
    {
        // we check if we can interact with something
        if (!usable_now) { return; }

        // we check if we interact with Pasta, we simply put pasta in
        if (interactor.CurrentHover?.capable is Pasta pasta)
        {
            PutPastaIn();
            Destroy(pasta.gameObject);
            return;
        }

        // we use the interactable
        interactor.Interact();
    }


    // on grabbed / dropped

    // BEING GRABBED / DROPPED
    protected override async void on_grabbed()
    {
        base.on_grabbed();

        // we subscribe to the InventoryMenu On
        if (Holder == Perso.Instance) { OnUsabilityChanged += inventory_menu.UpdateIF; }
    }
    protected override void on_dropped()
    {
        // we unsubscribe to the InventoryMenu On
        if (Holder == Perso.Instance) { OnUsabilityChanged -= inventory_menu.UpdateIF; }

        // we drop
        base.on_dropped();
    }



    // COOKING
    [Header("Cooking")]
    [SerializeField] private bool has_pasta = false;
    [SerializeField] private bool has_water = false;
    public bool IsFull { get { return has_water; } }

    [Header("Temperature")]
    [SerializeField] private float dissipation = 1.0f;
    [SerializeField] private float temperature = 0.0f;
    [SerializeField] private float limit_temperature = 110.0f;
    [SerializeField] private float stacked_temperature = 0f;
    [SerializeField] private float stack_limit = 100.0f;


    // FILL UP !
    public void Fill()
    {
        // on lance une coroutine de filling
        StartCoroutine(fill_coroutine());
    }
    private IEnumerator fill_coroutine()
    {
        // we play the filling_up anim
        anim_player.Play("fill_up");
        anim_player.AddToPile("idle_full");
        GetCapacity<HoverCapacity>()?.ChangeAnimation("hover_full");

        // we wait for the animation to end
        yield return new WaitWhile(() => anim_player.IsPlaying("fill_up"));

        // we set the pot as filled
        has_water = true;
        Reference = "pot:full";
    }

    // HEAT UP !
    public void TransmitHeat(float dt)
    {
        // we add the temperature;
        this.temperature += dt;
        // we stacked the temperature above limit
        if (temperature > limit_temperature)
        {
            temperature = limit_temperature;
            stacked_temperature += dt;
            if (stacked_temperature > stack_limit)
            {
                stacked_temperature = stack_limit;
            }
        }

        // we check for boiling
        update_boiling();

        // we check for burning
        update_burning();

        // we cool down a bit the stacked temperature above limit
        stacked_temperature -= Time.deltaTime * dissipation;
        if (stacked_temperature <= 0.0f)
        {
            stacked_temperature = 0.0f;
            temperature -= Time.deltaTime * dissipation;
            if (temperature < 0.0f) { temperature = 0.0f; }
        }

        if (debug) { Debug.Log($"(Pot) {dt} heat transmitted, now has {this.temperature + stacked_temperature}°C"); }
    }
    public void PutPastaIn()
    {
        has_pasta = true;
        anim_player.AddToPile("full_pasta");
        if (HasEffect(Effect.Boiling))
        {
            anim_player.Play("boiling_pasta");
        }
    }


    // BOILING
    private void update_boiling()
    {
        // checks if we need to start boiling
        if (temperature >= 100.0f && !HasEffect(Effect.Boiling))
        {
            AddEffect(Effect.Boiling, -888f);
            if (debug) { Debug.Log("(Pot) Now boiling !"); }

            // we play the animation
            anim_player.Play(has_pasta ? "boiling_pasta" : "boiling");
            return;
        }

        // checks if we need to stop boiling
        if (temperature < 100.0f && HasEffect(Effect.Boiling))
        {
            RemoveEffect(Effect.Boiling);

            // we play the animation
            anim_player.StopPlaying("boiling");
            anim_player.StopPlaying("boiling_pasta");
        }
    }

    // BURNING
    private void update_burning()
    {
        // we check if we start burning
        if (stacked_temperature >= stack_limit && !HasEffect(Effect.Burning))
        {
            AddEffect(Effect.Burning, -888f);
            if (debug) { Debug.Log("(Pot) Now burning !"); }

            // we play the burning up animation
            anim_player.Play("burn_up");
            anim_player.AddToPile("burning");

            // we set the hover animation to hover_burned
            anim_player.AddToPile("idle_burned");
            GetCapacity<HoverCapacity>()?.ChangeAnimation("hover_burned");
            Reference = "pot:burned";
            return;
        }

        // we check if we stop burning
        if (stacked_temperature <= 0.0f && HasEffect(Effect.Burning))
        {
            RemoveEffect(Effect.Burning);
            if (debug) { Debug.Log("(Pot) Stopped burning"); }

            // we stop the burning anims
            anim_player.Play("burn_down");
            anim_player.StopPlaying("burning");
        }
    }
}