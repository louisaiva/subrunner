#pragma warning disable 4014
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class Pot : Item, Usable
{
    [Header("Logs")]
    [SerializeField] private bool log_temp = false;

    [Header("Components")]
    private InteractCapacity interactor = null;
    private event Action<Item> OnUsabilityChanged = delegate { };
    private UI_InventoryMenu inventory_menu = null;

    // AWAKE & START
    protected override void Awake()
    {
        base.Awake();
        interactor = GetCapacity<InteractCapacity>();
        interactor.OnHoverSelect += (capable) => update_usability();
        interactor.OnHoverDeselect += (capable) => update_usability();

        // on subscribe aux events de l'inventaire
        Inventory.OnItemGrabbed += (item) =>
        {
            update_state();
            update_usability();
        };
        Inventory.OnItemDropped += (item) =>
        {
            update_state();
            update_usability();
        };
    }
    protected override void Start()
    {
        base.Start();
        inventory_menu = UI_Manager.Instance.GetPool("inventory") as UI_InventoryMenu;
    }


    // USABLE
    public string UseLabel { get; set; } = "";
    public bool usable_now = false;
    public void Use(Capable user)
    {
        // we check if we can interact with something
        if (interactor.interactable == null) { Empty(); return; }

        // we check if we interact with some food but we are burned ://
        if (interactor.interactable is Food && Reference == "pot:burned") { return; }

        // we use the interactable
        interactor.Interact();
    }
    private void update_usability()
    {
        // we check if we have an interactable
        Interactable interactable = interactor.interactable;
        string label = "";

        if (interactable is Oven) { label = "heat pot"; } // we check if it's an oven
        else if (interactable is Sink) { label = "fill pot"; } // or a sink
        else if (interactable is Food && Reference != "pot:burned") { label = "put food in"; } // or some food        
        else if (HasFood || has_water) { label = "empty pot"; } // we have no interesting interactable ://

        if (UseLabel == label) { return; }

        // we set the label
        UseLabel = label;
        usable_now = (label != "");
        OnUsabilityChanged?.Invoke(this);
    }

    // BEING GRABBED / DROPPED
    protected override void on_grabbed()
    {
        base.on_grabbed();

        // we subscribe to the InventoryMenu On
        if (Holder == Perso.Instance) { OnUsabilityChanged += inventory_menu.UpdateIFLabels; }
    }
    protected override void on_dropped()
    {
        // we unsubscribe to the InventoryMenu On
        if (Holder == Perso.Instance) { OnUsabilityChanged -= inventory_menu.UpdateIFLabels; }

        // we drop
        base.on_dropped();
    }



    // COOKING
    [Header("Cooking")]
    [SerializeField] private bool has_water = false;
    public bool HasFood { get { return Inventory.Count > 0; } }

    [Header("Temperature")]
    [SerializeField] private float dissipation = 1.0f;
    [SerializeField] private float temperature = 0.0f;
    [SerializeField] private float limit_temperature = 110.0f;
    [SerializeField] private float stacked_temperature = 0f;
    [SerializeField] private float stack_limit = 100.0f;
    [SerializeField] private bool is_burned = false;


    // FILL UP !
    public void Fill() { StartCoroutine(fill_coroutine()); }
    private IEnumerator fill_coroutine()
    {
        // si le pot est burned, alors on le nettoie
        if (Reference == "pot:burned")
        {
            yield return new WaitForSeconds(2.0f); // on attend un peu
            is_burned = false;
            update_state();
        }

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
    public void Empty(bool delete_food = false)
    {
        if (delete_food) { DestroyAllItems(); } // on DETRUIT tous les items de l'inventaire
        else { DropAllItems(); } // on les drop juste par terre

        // on vide l'eau du pot
        has_water = false;
        update_state();
        update_usability();
    }

    // UPDATE // todo make a TemperatureCapacity for this ?
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
            Empty(delete_food: true); // la nourriture crame & l'eau s'evapore
            // update_state(); // c fait automatiquement dans empty

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
        else if (has_water && HasFood) { reference += "full_pasta"; }
        else if (has_water) { reference += "full"; }
        else if (HasFood) { reference += "pasta"; }
        else { reference += "clean"; }
        Reference = reference;

        if (debug) { Debug.Log("(Pot) State updated to " + Reference); }

        // BURNING ANIMATIONS
        anim_player.ClearIdles();
        if (is_burned) { anim_player.AddToPile("idle_burned"); GetCapacity<HoverCapacity>()?.ChangeAnimation("hover_burned"); return; }

        // IDLE ANIMATIONS
        string contenu = "";
        if (has_water) { contenu += "full"; }
        if (HasFood) { contenu += (contenu == "" ? "pasta" : "_pasta"); }

        if (contenu == "") { GetCapacity<HoverCapacity>()?.ChangeAnimation("hover"); }
        else { anim_player.AddToPile("idle_" + contenu); GetCapacity<HoverCapacity>()?.ChangeAnimation("hover_" + contenu); }

        // BOILING ANIMATIONS
        if (HasEffect(Effect.Boiling))
        {
            anim_player.StopPlaying("boiling");
            anim_player.StopPlaying("boiling_pasta");
            if (has_water) { anim_player.Play(HasFood ? "boiling_pasta" : "boiling"); }
        }
    }
}