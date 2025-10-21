using UnityEngine;
using System;
using System.Collections;


public class Pot : Item, Usable
{
    [Header("Logs")]
    [SerializeField] private bool log_temp = false;

    [Header("Components")]
    private InteractCapacity interactor = null;
    private event Action<Item> OnUsabilityChanged = delegate { };
    private UI_InventoryMenu inventory_menu = null;

    protected override void Awake()
    {
        base.Awake();
        interactor = GetCapacity<InteractCapacity>();
        interactor.OnHoverSelect += (capable) => update_usability();
        interactor.OnHoverDeselect += (capable) => update_usability();
    }
    protected override void Start()
    {
        base.Start();
        inventory_menu = UI_Manager.Instance.GetPool("inventory") as UI_InventoryMenu;
    }

    // UPDATE LABEL & USABILITY
    private void update_usability()
    {
        // we check if we have an interactable
        HoverCapacity hover = interactor.CurrentHover;
        Capable interactable = hover?.capable;
        if (interactable != null)
        {
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

            // or a pastaaaa
            if (interactable is Pasta)
            {
                UseLabel = "put pasta in pot";
                usable_now = true;
                OnUsabilityChanged?.Invoke(this);
                return;
            }
        }


        // we have no interactable -> we check if we have pasta or wat
        if (has_pasta || has_water)
        {
            UseLabel = "empty pot";
            usable_now = true;
            OnUsabilityChanged?.Invoke(this);
            return;
        }

        // we reset the use label and usability
        if (!usable_now) { return; }
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

        if (interactor.CurrentHover == null ||
        (interactor.CurrentHover.capable is not Sink &&
        interactor.CurrentHover.capable is not Oven &&
        interactor.CurrentHover.capable is not Pasta))
        {
            // either we are full / pastaed and we want to empty the pot
            if (has_pasta) { RemovePasta(); }
            if (has_water) { Empty(); }
            return;
        }

        // we check if we interact with Pasta, we simply put pasta in
        if (interactor.CurrentHover?.capable is Pasta pasta)
        {
            if (Reference == "pot:burned") { return; }
            PutPastaIn();
            Destroy(pasta.gameObject);
            return;
        }

        // we use the interactable
        interactor.Interact();
    }


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
    public bool HasPasta { get { return has_pasta; } }

    [Header("Temperature")]
    [SerializeField] private float dissipation = 1.0f;
    [SerializeField] private float temperature = 0.0f;
    [SerializeField] private float limit_temperature = 110.0f;
    [SerializeField] private float stacked_temperature = 0f;
    [SerializeField] private float stack_limit = 100.0f;
    [SerializeField] private bool is_burned = false;


    // FILL UP !
    public void Fill()
    {
        // si le pot est burned, alors on le nettoie
        if (Reference == "pot:burned")
        {
            is_burned = false;
            update_state();
            return;
        }

        // on lance une coroutine de filling
        StartCoroutine(fill_coroutine());
    }
    private IEnumerator fill_coroutine()
    {
        // we play the filling_up anim
        anim_player.Play("fill_up");

        // we wait for the animation to end
        yield return new WaitWhile(() => anim_player.IsPlaying("fill_up"));

        // we set the pot as filled
        has_water = true;
        update_state();

        // on met à jour l'usabilite
        update_usability();
    }
    public void Empty()
    {
        // on vide l'eau du pot
        has_water = false;
        update_state();

        // on met à jour l'usabilite
        update_usability();
    }
    public void PutPastaIn()
    {
        has_pasta = true;
        update_state();

        // on met à jour l'usabilite
        update_usability();

    }
    public void RemovePasta()
    {
        // on vide le pot
        has_pasta = false;
        update_state();

        // on met à jour l'usabilite
        update_usability();
    }

    // UPDATE
    protected override void LateUpdate()
    {
        base.LateUpdate();

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

        if (log_temp) { Debug.Log($"(Pot) is at temperature {this.temperature + stacked_temperature}°C"); }
    }
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
    }

    // BOILING & BURNING
    private void update_boiling()
    {
        // checks if we need to start boiling
        if (temperature >= 100.0f && !HasEffect(Effect.Boiling))
        {
            AddEffect(Effect.Boiling, -888f);
            if (debug) { Debug.Log("(Pot) Now boiling !"); }
            update_state();
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
            is_burned = true;
            has_pasta = false; // les pates brulent
            has_water = false; // l'eau s'evapore
            update_state();

            // on met à jour l'usabilite
            UseLabel = "";
            usable_now = false;
            OnUsabilityChanged?.Invoke(this);
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

    // STATE UPDATING
    private void update_state()
    {
        // we update the pot state based on if it has pasta, water or is burnt

        // REFERENCE
        string reference = "pot:";
        if (is_burned) { reference += "burned"; }
        else if (has_water && has_pasta) { reference += "full_pasta"; }
        else if (has_water) { reference += "full"; }
        else if (has_pasta) { reference += "pasta"; }
        else { reference += "clean"; }
        Reference = reference;

        if (debug) { Debug.Log("(Pot) State updated to " + Reference); }

        // BURNING ANIMATIONS
        anim_player.ClearIdles();
        if (is_burned) { anim_player.AddToPile("idle_burned"); GetCapacity<HoverCapacity>()?.ChangeAnimation("hover_burned"); return; }

        // IDLE ANIMATIONS
        string contenu = "";
        if (has_water) { contenu += "full"; }
        if (has_pasta) { contenu += (contenu == "" ? "pasta" : "_pasta"); }

        if (contenu == "") { GetCapacity<HoverCapacity>()?.ChangeAnimation("hover"); }
        else { anim_player.AddToPile("idle_" + contenu); GetCapacity<HoverCapacity>()?.ChangeAnimation("hover_" + contenu); }

        // BOILING ANIMATIONS
        if (HasEffect(Effect.Boiling))
        {
            anim_player.StopPlaying("boiling");
            anim_player.StopPlaying("boiling_pasta");
            if (has_water) { anim_player.Play(has_pasta ? "boiling_pasta" : "boiling"); }
        }
    }
}