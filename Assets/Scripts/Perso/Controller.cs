#pragma warning disable 4014
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [Header("Current capable")]
    public Capable Capable
    {
        get
        {
            if (_capable == null)
                _capable = transform.parent.GetComponent<Capable>();
            return _capable;
        }
    }
    [SerializeField] private Capable _capable;

    [Header("Capable stack")]
    public List<Capable> stack = new List<Capable>();

    [Header("Components")]
    public PersoInputsController PIC;
    public UI_InputsController UIC;
    public HackableNavigator HackableNavigator;
    public ExploitNavigator ExploitNavigator;
    [SerializeField] private SeeThroughHandler see_through;
    public Room current_room { get; set; }

    [Header("UI Statics elements")]
    [SerializeField] private UI_Inventory perso_quick_inventory;
    [SerializeField] private GameObject life_bar;
    [SerializeField] private GameObject shortcuts;

    [Header("Events")]
    public System.Action<Capable> OnCapableControlled;
    public System.Action<Capable> OnCapableUncontrolled;

    [Header("Log")]
    [SerializeField] private bool log = false;
    [SerializeField] private bool log_ui_attachment = false;

    // AWAKE
    public static Controller Instance { get; private set; }
    public static System.Action<Controller> OnInstanceRemoved { get; set; }
    public static System.Action<Controller> OnInstanceSet { get; set; }
    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (log) { Debug.LogWarning("(Controller) multiple instances of Controller detected! Destroying the old one."); }
            Destroy(Instance.gameObject);
            OnInstanceRemoved?.Invoke(Instance);
            Instance = null;
        }
        Instance = this;
        OnInstanceSet?.Invoke(Instance);
    }

    // START
    private void Start()
    {
        // on récupère les composants
        UI_Pool hud = UI_Manager.Instance.GetPool("hud");
        life_bar = hud.transform.Find("life_bar").gameObject;
        shortcuts = hud.transform.Find("shortcuts_if").gameObject;
        // perso_quick_inventory = UI_Manager.Instance.GetPool("quick_inventory").transform.Find("perso_quick_inventory").GetComponent<UI_Inventory>();
        // perso_quick_inventory = (UI_Manager.Instance.GetPool("quick_inventory") as UI_ChestPool).UI;

        // on controlle le capable actuel
        stack.Clear();
        stack.Add(Capable);
        control(Capable);
    }

    // CHANGE CAPABLE TARGET HIGH LEVEL
    public void ChangeCapableTarget(Capable new_target, float duration = -888f, bool add_to_stack = true)
    {
        // checks if the target is the same
        if (new_target == Capable) { return; }

        if (log) { Debug.Log("(Controller) " + name + " is changing capable target to " + new_target.name + (add_to_stack ? " and adding to stack" : "")); }

        // on décontrole l'ancienne target
        uncontrol_capable(Capable);

        // on déplace le script sur le gameobject capable
        transform.parent = new_target.transform;
        transform.localPosition = Vector3.zero;

        // on ajoute la target à la stack
        if (add_to_stack) { stack.Add(new_target); }
        _capable = new_target;


        // on controle la nouvelle target
        control(new_target, duration);
    }
    public void BreakCapableTarget(Capable target)
    {
        if (!stack.Contains(target) || stack.Count <= 1) { return; }

        if (log) { Debug.Log("(Controller) " + name + " is breaking capable target : " + target.name); }

        // on parcourt toute la stack depuis la fin pour voir jusqu'ou on remonte dans la stack
        for (int i = stack.Count - 1; i >= 0; --i)
        {
            Capable capa = stack[i];
            stack.RemoveAt(i);
            if (capa == target) { break; }
        }

        // on change de target
        ChangeCapableTarget(stack.Last(), add_to_stack: false);
    }
    public void ResetCapableTarget(bool control_nothing = false)
    {
        if (log) { Debug.Log("(Controller) " + name + " is resetting capable target to Perso"); }

        CancelInvoke("ResetCapableTarget");

        // soit on clear tout carrément on décontrole giga tout
        if (control_nothing) { uncontrol_capable(stack[stack.Count - 1]); }

        // on clear la stack
        stack.Clear();

        // soit on change la target pour le perso (ajoute automatiquement à la stack)
        if (!control_nothing) { ChangeCapableTarget(Perso.Instance, add_to_stack: true); }
    }


    // CHANGE CAPABLE low level
    private void uncontrol_capable(Capable capa)
    {
        // reset les inputs de l'ancien capable
        capa.ClearInputs();
        if (capa.GetCapacity<WalkCapacity>() != null)
        {
            capa.GetCapacity<WalkCapacity>().walk_percentage_target = 0f;
        }

        // reset le behaviour
        if (capa is IA old_ia)
        {
            // on réactive l'ancien Brain si le capable actuel est une ia
            old_ia.Brain?.gameObject.SetActive(true);

            // on remet le tag
            old_ia.gameObject.tag = old_ia.BaseTag;

            // reset les tags d'attaques si on a
            if (old_ia.HasCapacity<AttackCapacity>())
            {
                old_ia.GetCapacity<AttackCapacity>().ResetTags();
            }
        }

        if (capa is Device)
        {
            // on enleve le device du UI_Device
            UI_Manager.Instance.GetPool<UI_Device>()?.ClearDevice();
            UI_Manager.Instance.UnstackPool("device", override_transition: true);
        }

        // reset l'inventory
        // capa?.Inventory?.RemoveUI(perso_quick_inventory);
        perso_quick_inventory.Inventory = null;
        unattach_ui_item_pools();


        // on cache l'hp bar & shortcuts seulement si c'est le perso
        UI_HUD hud = UI_Manager.Instance.GetPool("hud").GetComponent<UI_HUD>();
        if (capa is Perso)
        {
            hud.QuitPool(life_bar); life_bar.GetComponent<Transitioner>().Hide();
            hud.QuitPool(shortcuts); shortcuts.GetComponent<Transitioner>().Hide();
        }

        // on déconnecte la connect capacity
        if (capa.HasCapacity<ConnectCapacity>())
        {
            capa.GetCapacity<ConnectCapacity>().Disconnect();
        }

        capa.anim_player.OnSkinChange -= refresh_skin_based_parameters; // on enlève le callback de changement de skin

        OnCapableUncontrolled?.Invoke(capa);

        if (log) { Debug.Log("(Controller) " + name + " is done controlling " + capa.name); }
    }
    private void control(Capable capa, float duration = -888f)
    {
        // on refresh la cam
        CameraFollow.Instance.RefreshTarget(capa);

        // si on a une durée, on reviens au perso après la durée
        CancelInvoke("ResetCapableTarget");
        if (duration != -888f) { Invoke("ResetCapableTarget", duration); }

        // on ajoute le callback de changement de skin
        refresh_skin_based_parameters(capa.Skin);
        capa.anim_player.OnSkinChange += refresh_skin_based_parameters;

        // on désactive le Brain si le nouveau capable est un IA
        if (capa is IA ia)
        {
            // désactive le cerveau
            ia.Brain?.gameObject.SetActive(false);

            // on remet le tag
            ia.gameObject.tag = "Controlled";

            // on clear les tags d'attaque pour pouvoir attaquer des gens
            if (ia.HasCapacity<AttackCapacity>())
            {
                ia.GetCapacity<AttackCapacity>().ClearTags();
            }
        }

        // on met le perso_quick_inventory sur la target si elle a un inventaire
        // capa?.Inventory?.AddUI(perso_quick_inventory);
        // perso_quick_inventory.Refresh();

        // on assigne les différents item pools de l'inventaire à leurs UI_ItemPool respectifs
        attach_item_pools_to_ui(capa.Inventory);



        // on affiche l'hp bar & shortcuts seulement si c'est le perso
        UI_HUD hud = UI_Manager.Instance.GetPool("hud").GetComponent<UI_HUD>();
        if (capa is Perso)
        {
            hud.RegisterToPool(life_bar, is_stacked: true);
            hud.RegisterToPool(shortcuts, is_stacked: true);
        }

        // on regarde si le capable est un device
        if (capa is Device device)
        {
            // on refresh le hackable navigator pour qu'il ait une nouvelle ConnectCapacity si jamais le capable a un device
            HackableNavigator.transform.localPosition = device.Connector.transform.localPosition;

            // on bascule en pool UI_Device
            UI_Manager.Instance.GetPool("device").GetComponent<UI_Device>().SetDevice(device);
            UI_Manager.Instance.SwitchTo("device", override_transition: true);
        }

        if (log) { Debug.Log("(Controller) " + name + " is now controlling " + capa.name); }

        OnCapableControlled?.Invoke(capa);

        // on informe le debug manager qu'on controle un nouveau capable
        DebugManager.Instance.AddDebuggable(capa, "controller");
    }
    private void refresh_skin_based_parameters(string skin)
    {
        if (log) { Debug.Log("(Controller) refreshing skin based parameters for skin " + skin + " on capable " + Capable.name); }

        // on refresh le see through pour remettre la tete bien centrée
        see_through.Refresh(skin);
    }



    // low level control / uncontrol helpers
    private void attach_item_pools_to_ui(Inventory inventory)
    {
        if (inventory == null) { return; }

        // on récupère les ui_item_pools du ui_inventoryMenu
        UI_InventoryMenu inventory_menu = UI_Manager.Instance.GetPool<UI_InventoryMenu>();
        List<UI_ItemPool> ui_pools = inventory_menu.GetItemPools();
        
        // we go through all itempools in inventory
        for (int i=0; i<inventory.pools.Count; ++i)
        {
            ItemPool pool = inventory.pools[i];
            if (pool == null) { continue; }

            // on regarde si on a un ui_item_pool qui a la même pool_id
            UI_ItemPool ui_pool = ui_pools.Find(p => p.PoolID == pool.PoolID);
            if (ui_pool == null)
            {
                // if we don't have a ui pool for this item pool, we skip it
                if (log_ui_attachment) { Debug.LogWarning($"(Controller) No UI_ItemPool found for ItemPool with id {pool.PoolID} in inventory of capable {Capable.name}! Skipping UI attachment for this pool."); }
                continue;
            }

            if (log_ui_attachment) { Debug.Log($"(Controller) Attaching ItemPool with id {pool.PoolID} to UI_ItemPool {ui_pool.name} for capable {Capable.name}."); }

            // on attache la pool à l'ui pool
            ui_pool.AttachToPool(pool);
        }
    }
    private void unattach_ui_item_pools()
    {
        // on récupère les ui_item_pools du ui_inventoryMenu
        UI_InventoryMenu inventory_menu = UI_Manager.Instance.GetPool<UI_InventoryMenu>();
        List<UI_ItemPool> ui_pools = inventory_menu.GetItemPools();

        // on détache tous les ui pools de leur pool
        for (int i = 0; i < ui_pools.Count; ++i) { ui_pools[i].DetachFromPool(); }
    }




    // GETTERS
    public EndlessInput<T> GetEndlessInput<T>(string name) where T : struct
    {
        EndlessInput<T> endinp = PIC.get_endless_input<T>(name);
        if (endinp != null) { return endinp; }
        endinp = UIC.get_endless_input<T>(name);
        return endinp;
    }


    // ON DESTROY
    protected virtual void OnDestroy()
    {
        if (Instance == this)
        {
            OnInstanceRemoved?.Invoke(Instance);
            Instance = null;
        }
    }
}