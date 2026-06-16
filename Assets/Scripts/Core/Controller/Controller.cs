#pragma warning disable 1998

using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{

    ///
    //
    /// SINGLETON & SUB SYSTEMS
    //
    ///

    public static Controller _Instance;
    public static Controller LazyInstance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = FindFirstObjectByType<Controller>();
                if (_Instance == null) { Debug.LogError($"(Controller) No instance of Controller found in the scene."); }
            }
            return _Instance;
        }
    }

    public static PersoData _perso;
    public static Perso Perso
    {
        get
        {
            if (_perso == null) { return null; }
            return _perso.Perso;
        }
    }

    public static Capable Capable
    {
        get
        {
            if (_Instance == null) { return null; }
            return _Instance.ControlledCapable;
        }
    }

    ///
    //
    /// CLASS VARIABLES
    //
    ///

    // DATA
    public ControllerData data;

    // CAPABLE RTO STACK
    private Stack<string> stack = new Stack<string>(); // holds the capable id stack, controlled_capable is NOT in the stack
    private Capable controlled_capable;
    public Capable ControlledCapable { get { return controlled_capable; } }
    public string ID { get { return controlled_capable?.ID ?? ""; } }


    [Header("Components")]
    [SerializeField] private PersoInputsController pic;
    // [SerializeField] private UI_InputsController uic;
    [SerializeField] private HackableNavigator hackable_navigator;
    [SerializeField] private ExploitNavigator exploit_navigator;
    [SerializeField] private SeeThroughHandler see_through;


    [Header("Events")]
    public System.Action<string> OnCapableAddedToStack; // triggered only the first time a capable is controlled (until next time it's removed from stack)
    public System.Action<string> OnCapableRemovedFromStack;
    public System.Action<Capable> OnCapableControlled; // triggered each time a capable is controlled
    public System.Action<Capable> OnCapableUncontrolled;
    
    [Header("Logs")]
    [SerializeField] private bool log;
    [SerializeField] private bool log_perso;
    // [SerializeField] private bool enable_controller_inventory_logs;
    [SerializeField] private bool enable_controller_animplayer_logs;


    ///
    //
    /// MAIN CONTROL METHODS 
    //
    ///


    // STACK MANAGEMENT
    public bool Control(string capable_id)
    {
        if (string.IsNullOrEmpty(capable_id)) { return false; }
        if (controlled_capable != null && controlled_capable.ID == capable_id)
        {
            // if we are already controlling the capable, we do nothing
            if (log) { Debug.Log($"(Controller) Already controlling capable with id {capable_id}. Doing nothing."); }
            return true;
        }
        if (stack.Contains(capable_id))
        {
            // if we are already controlling the capable, we do nothing
            if (log) { Debug.LogError($"(Controller) Cannot control capable with id {capable_id} because it is already in the stack !!"); }
            return false;
        }

        // else we can control the new capable !

        // we add the current capable to the stack if there is one
        if (controlled_capable != null) { stack.Push(controlled_capable.ID); }
        control(capable_id);
        OnCapableAddedToStack?.Invoke(controlled_capable.ID);

        return true;
    }
    public bool Uncontrol(bool control_next_in_stack = true)
    {
        // we uncontrol the current capable, then we pop the stack and control the new top of the stack if there is one
        if (controlled_capable == null) { return false; }
        OnCapableRemovedFromStack?.Invoke( controlled_capable.ID);
        uncontrol();
        if (control_next_in_stack && stack.Count > 0) { return control(stack.Pop()); }
        return true;
    }
    public bool Uncontrol(string capable_id)
    {
        // here we want to uncontrol the capable with the given id, SO
        // we need to uncontrol the current capable, then pop until we find the capable name
        // and then control the new top of the stack if there is one
        if (controlled_capable == null) { return false; }
        if (controlled_capable.ID == capable_id) { return Uncontrol(); }
        if (!stack.Contains(capable_id))
        {
            // if the capable is not in the stack, we do nothing
            // if (log) { Debug.Log($"(Controller) Cannot uncontrol capable with id {capable_id} because it is not in the stack !!"); }
            return false;
        }

        // else we need to pop until we find the capable name
        Uncontrol(control_next_in_stack : false);
        bool found_target = false;
        while (stack.Count > 0 && !found_target)
        {
            string top_id = stack.Pop();
            if (top_id == capable_id) { found_target = true; }
            OnCapableRemovedFromStack?.Invoke(top_id);
        }
        if (!found_target)
        {
            Debug.LogError($"(Controller) Unexpected error while trying to uncontrol capable with id {capable_id} !!");
            return false;
        }

        // then we control the new top of the stack if there is one
        if (stack.Count > 0) { return control(stack.Pop()); }
        return true;
    }
    public bool UncontrolAll(bool log = false)
    {
        if (controlled_capable != null)
        {
            OnCapableRemovedFromStack?.Invoke(controlled_capable.ID);
            uncontrol();

            while (stack.Count > 0)
            {
                string top_id = stack.Pop();
                OnCapableRemovedFromStack?.Invoke(top_id);
            }
        }

        // then we move back the controller transform to the core
        if (LevelEngine.LazyInstance != null && LevelEngine.LazyInstance.transform.parent != null)
        {
            transform.parent = LevelEngine.LazyInstance.transform.parent;
            transform.localPosition = Vector3.zero;
            if (log) { Debug.Log($"(Controller) Uncontrolled all capables and moved controller transform back to LevelEngine parent."); }
        }
        else
        {
            transform.parent = null;
            transform.localPosition = Vector3.zero;
            if (log) { Debug.Log($"(Controller) Uncontrolled all capables and moved controller transform back to root."); }
        }

        if (log) { Debug.Log($"(Controller) CONTROLLER SUCCESSFULLY UNCONTROLLED ALL CAPABLES"); }

        return true;
    }

    ///
    //
    /// LOW LEVEL METHODS
    //
    ///

    // CONTROL / UNCONTROL
    private bool control(string capable_id)
    {
        // ensure the capable is loaded
        if (!CapableBank.LazyInstance.TryGetLoadedCapable(capable_id, out Capable capable))
        {
            capable = CapableEngine.LazyInstance.LoadCapableInstantly(capable_id);
            if (capable == null)
            {
                Debug.LogError($"(Controller) Could not load instantly capable '{capable_id}' so we won't control it oopsie");
                return false;
            }
        }
        if (capable is Perso perso) { _perso = (PersoData)perso.data; if (log_perso) {Debug.Log($"(Controller) new perso controlled: {perso.ID}, Controller.Perso is now {Controller.Perso.ID}"); } }

        if (enable_controller_animplayer_logs)
        {
            capable.AnimPlayer.log = true;
        }

        // next we uncontrol the current capable if there is one
        uncontrol();

        // we control the new capable
        control_capacities(capable);


        // we move the script sur le gameobject capable
        transform.parent = capable.transform;
        transform.localPosition = Vector3.zero;

        // we control the new capable
        controlled_capable = capable;
        data.controlled_capable_id = capable.ID;
        Debug.Log("(Controller) ++++++++++++++++++++++++++++ NOW CONTROLING " + controlled_capable.ID);
        controlled_capable.OnControlled();
        OnCapableControlled?.Invoke(controlled_capable);


        // we refresh the player chunk through ChunkEngine
        ChunkEngine.LazyInstance.RefreshPlayerChunk(capable);

        // on informe le debug manager qu'on controle un nouveau capable
        DebugManager.Instance.AddDebuggable(controlled_capable, "controller");
        return true;
    }
    private void uncontrol()
    {
        if (controlled_capable == null) { return; }
        if (controlled_capable is Perso)
        {
            if (log_perso) { Debug.Log($"(Controller) perso uncontrolled: {_perso.id}, Controller.Perso is now null"); }
            _perso = null;
        }
        if (!controlled_capable.Loaded)
        {
            // si pas loadé bah on a rien besoin de faire
            controlled_capable = null;
            return;
        }

        uncontrol_capacities(controlled_capable);

        // uncontrol the capable
        OnCapableUncontrolled?.Invoke(controlled_capable);
        controlled_capable.OnUncontrolled();
        Debug.Log("(Controller) ---------------------------- DONE CONTROLING " + controlled_capable.ID);
        controlled_capable = null;
    }

    // LOW LEVEL CONTROL METHODS
    private void control_capacities(Capable capa)
    {
        // on refresh la cam
        CameraFollow.Instance.AddTarget(capa);

        // register to the being died event of the new capable
        /* if (capa.TryGetCapacity(out HealthCapacity hcapa))
        {
            hcapa.OnDie += handle_being_died;
            if (log) { Debug.Log($"(Controller) Registered to OnDie event of capable with id {capa.ID}."); }
        } */

        // on ajoute le callback de changement de skin
        refresh_skin_based_parameters(capa.Skin);
        capa.AnimPlayer.OnSkinChange += refresh_skin_based_parameters;

        // on désactive le Brain si le nouveau capable est un IA
        if (capa is IA ia)
        {
            // désactive le cerveau
            ia.Brain?.gameObject.SetActive(false);

            // on remet le tag
            ia.gameObject.tag = "Controlled";
        }
        // clear les tags d'attaques si on a
        if (capa.TryGetCapacity(out AttackCapacity attack_capa)) { attack_capa.ClearTags(); }

        // on assigne les différents item pools de l'inventaire à leurs UI_ItemPool respectifs
        // attach_item_pools_to_ui(capa.Inventory, capa.ID);

        // on regarde si le capable est un device
        if (capa is Device device)
        {
            // on refresh le hackable navigator pour qu'il ait une nouvelle ConnectCapacity si jamais le capable a un device
            HackableNavigator.transform.localPosition = device.Connector.transform.localPosition;

            // on bascule en pool UI_Device
            UI_Manager.Instance.GetPool("device").GetComponent<UI_Device>().SetDevice(device);
            UI_Manager.Instance.SwitchTo("device", override_transition: true);
        }

        // check exp
        if (capa.TryGetCapacity(out ExpCapacity exp_capa) && exp_capa.edata.upgrade_points > 0)
        {
            // Debug.Log("(ExpCapacity) Player has " + exp_capa.edata.upgrade_points + $" upgrade points to spend ! (when loading {capa.ID})");
            UI_Manager.Instance.GetPool<UI_HUD>().PersoLeveledUP(exp_capa.edata.level, exp_capa.edata.upgrade_points);
        }
    }
    private void uncontrol_capacities(Capable capa)
    {
        capa.AnimPlayer.OnSkinChange -= refresh_skin_based_parameters; // on enlève le callback de changement de skin

        // register to the being died event of the new capable
        /* if (capa.TryGetCapacity(out HealthCapacity hcapa))
        {
            hcapa.OnDie -= handle_being_died;
            if (log) { Debug.Log($"(Controller) Unregistered from OnDie event of capable with id {capa.ID}."); }
        } */


        // clear les inputs & stoppe les déplacements
        capa.ClearInputs();
        if (capa.TryGetCapacity(out WalkCapacity walk_capa))
        {
            walk_capa.walk_percentage_target = 0f;
        }

        // reset le behaviour
        if (capa is IA old_ia)
        {
            // todo update this with MotorCapacity
            // on réactive l'ancien Brain si le capable actuel est une ia
            old_ia.Brain?.gameObject.SetActive(true);

            // on remet le tag
            old_ia.gameObject.tag = old_ia.BaseTag;
        }

        // reset les tags d'attaques si on a
        if (capa.TryGetCapacity(out AttackCapacity attack_capa)) { attack_capa.ResetTags(); }

        // on enleve le device du UI_Device
        if (capa is Device)
        {
            UI_Manager.Instance.GetPool<UI_Device>()?.ClearDevice();
            UI_Manager.Instance.UnstackPool("device", override_transition: true);
        }

        // on déconnecte la connect capacity
        if (capa.TryGetCapacity(out ConnectCapacity connect_capa)) { connect_capa.Disconnect(); }


        // reset l'inventory
        // unattach_ui_item_pools();
        if (capa.Inventory != null) { capa.Inventory.DisableLogs(); }

        // on refresh la cam
        CameraFollow.Instance.RemoveTarget(capa);
    }
    private void refresh_skin_based_parameters(string skin)
    {
        // if (log) { Debug.Log("(Controller) refreshing skin based parameters for skin " + skin + (Capable != null ? $"(on capable {Capable.ID})" : "")); }

        // on refresh le see through pour remettre la tete bien centrée
        see_through.Refresh(skin);
    }


    ///
    //
    /// DESPAWN / RESPAWN METHODS
    //
    ///

    // HANDLE DESPAWN
    public void handle_capable_despawned(CapableData data)
    {
        // if (log) { Debug.Log($"(Controller) capable with id {data.id} despawned, uncontrolling it if it was controlled"); }
        Uncontrol(data.id);
    }

    // RESPAWN
    public void RespawnPerso()
    {
        if (Perso != null) { return; } // if we already control a perso, we do nothing

        // we duplicate the data so we have one :D
        ControllerData new_data = data.Duplicate();

        // we unload the data (this.data will == null)
        UnloadData();

        // we get the controlled capable data so we can modify few things (heal max, apply respawn point, etc)
        // ! important : we duplicate EXISTING DATA so the capable will receive new id otherwise we will have 2 capables
        // ! sharing the same id (and a controlled id which is even worse)
        CapableData capable = CapableEngine.LazyInstance.DuplicateExistingData(new_data.controlled_capable_id);
        if (capable == null)
        {
            Debug.LogError($"(Controller) Cannot respawn perso because the saved controller data has a controlled capable id that doesn't exist !!");
            return;
        }
        new_data.controlled_capable_id = capable.id;

        // set position to 0,0
        capable.position = Vector3.zero;

        // we get the capacity ids
        List<CapacityData> capacities_data = CapacityEngine.LazyInstance.GetCapacitiesDataFromIDs(capable.capacities_ids);
        foreach (CapacityData capa_data in capacities_data)
        {
            if (capa_data is HealthCapacityData health_data)
            {
                // we heal the perso to max
                health_data.health = health_data.max_health;
                break;
            }
        }

        // then we load the controller data again, which will load the capable data we just modified
        LoadData(new_data, tp: false);
    }


    ///
    //
    /// GETTERS & OTHERS
    //
    ///


    // GETTERS
    public EndlessInput<T> GetEndlessInput<T>(string name) where T : struct
    {
        EndlessInput<T> endinp = PIC.get_endless_input<T>(name);
        return endinp;
    }
    public HackableNavigator HackableNavigator { get { return hackable_navigator; } }
    public ExploitNavigator ExploitNavigator { get { return exploit_navigator; } }
    public SeeThroughHandler SeeThrough { get { return see_through; } }
    public PersoInputsController PIC { get { return pic; } }



    // CLEAR STACK
    public void ClearStack()
    {
        // ? should we fire events here ?
        stack.Clear();
    }
    private void OnDestroy()
    {
        Debug.LogWarning($"(Controller) OnDestroy called holy shit that's terrible, fear the Controller.LazyInstance error log muahahah");
    }

    ///
    //
    /// DATA MANAGEMENT
    //
    ///

    
    public async Awaitable LoadWorldData(string world_id, bool log)
    {
        if (log) { Debug.Log($"(Controller) Loading controller data for world with id '{world_id}' ..."); }


        // get the controller data
        ControllerData data = LoadWorldControllerData(world_id);
        if (data == null) { return; }

        // load the data & control the initial capable
        if (log) { Debug.Log($"(Controller) Controller data ready to be loaded : {data.GetDetails()}"); }
        LoadData(data);

        // register to CapableEngine despawn event
        CapableEngine.LazyInstance.OnCapableDespawned += handle_capable_despawned;

        if (log) { Debug.Log($"(Controller) CONTROLLER SUCCESSFULLY LOADED for '{world_id}' !\n{data.GetDetails()}"); }

        // now we can launch the "intro" cinematics (only if this is the first spawn)
        if (data.play_intro_cinematic) { UI_Manager.Instance.GetPool<UI_CinematicPool>()?.PlayCinematic("intro"); }
        else { UI_Manager.Instance.SwitchToHUD(); }

        // in all cases we remove the intro cinematic flag so we don't play it again
        data.play_intro_cinematic = false;
    }
    public async Awaitable UnloadWorldData(bool log)
    {
        if (log) { Debug.Log($"(Controller) Unloading controller data ..."); }

        // unregister to CapableEngine despawn event
        CapableEngine.LazyInstance.OnCapableDespawned -= handle_capable_despawned;
        
        UncontrolAll(log);

        if (log) { Debug.Log($"(Controller) CONTROLLER SUCCESSFULLY UNLOADED !"); }
    }

    // DATA MANAGEMENT
    [Header("On Load Data parameters")]
    [Tooltip("If true, the controller will respawn the template capable EACH time the world is loaded ! THIS MEANS YOU LOSE INVENTORY & POSITION, don't enable this if you don't need it")]
    [SerializeField] private bool respawn_template = false;
    public void LoadData(ControllerData data, bool tp = true)
    {
        this.data = data;
        stack.Clear();

        #if !UNITY_EDITOR
        if (respawn_template) { Debug.LogError("(Controller) Respawning template on world loading is enabled! You will always lose your inventory & position! If you don't want this, please download another subrunner version :D"); }
        #endif


        // if we have a container id we need to load it instantly because otherwise we won't be able to load
        // the controller capable which is inside the container
        if (!string.IsNullOrEmpty(data.container_id))
        {
            if (!CapableBank.LazyInstance.TryGetLoadedCapable(data.container_id, out Capable container_capable))
            {
                container_capable = CapableEngine.LazyInstance.LoadCapableInstantly(data.container_id);
                if (container_capable == null)
                {
                    Debug.LogError($"(Controller) Could not load instantly container capable '{data.container_id}' so we probably won't be able to load controller");
                }
            }
        }


        string capable_id = data.controlled_capable_id;
        if (string.IsNullOrEmpty(capable_id)) { capable_id = data.capable_template; }
        if (string.IsNullOrEmpty(capable_id))
        {
            Debug.LogError($"(Controller) No capable id defined in controller data !!");
            return;
        }
        if (respawn_template) { capable_id = data.capable_template; } // we always assign the new perso as template

        // load the controlled capable
        if (!Control(capable_id))
        {
            Debug.LogError($"(Controller) Failed to control controller capable : '{capable_id}'");
            return;
        }

        // here we can tp the camera to the controlled capable position
        this.data.controlled_capable_id = Capable.ID;
        CameraFollow.Instance.AddTarget(Capable, tp: tp);

        // load the capable stack
        if (data.stack_capable_ids != null)
        {
            foreach (string id in data.stack_capable_ids)
            {
                if (!string.IsNullOrEmpty(id)) { stack.Push(id); }
            }
        }

    }
    public virtual void UnloadData()
    {
        // uncontrol everything
        UncontrolAll();

        this.data = null;

        // clear the data
        ClearStack();
    }

    // STATIC METHODS
    public static ControllerData LoadWorldControllerData(string world_id)
    {
        if (string.IsNullOrEmpty(world_id)) { return null; }
        WorldSaveData save = SaveEngine.GetWorldSave(world_id);
        if (save == null) { return null; }
        return save.controller;
    }
}