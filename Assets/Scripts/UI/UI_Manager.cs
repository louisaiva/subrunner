using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// This class is used to manage the UI elements
/// its transform is located at /ui
/// all their children are considered as UI elements
/// and are handled via "pools of UI elements"
/// </summary>

public class UI_Manager : MonoBehaviour
{

    [Header("Pools")]
    [SerializeField] private string current_pool = "";
    [SerializeField] private List<string> pools = new List<string>() { "hud", "pause", "inventory", "map" };
    private Dictionary<string, List<GameObject>> ui_pools = new Dictionary<string, List<GameObject>>();

    [Header("Debug")]
    public bool debug = false;

    // inputs
    private PlayerInputActions inputs;

    // START
    void Awake()
    {
        // on initialise les pools
        foreach (string pool in pools)
        {
            ui_pools[pool] = new List<GameObject>();
        }


        // on met manuellement quelques ui elements
        RegisterToPool("hud", GameObject.Find("/ui/hud/tutorials"));
        RegisterToPool("hud", GameObject.Find("/ui/hud/hp_xp"));
        RegisterToPool("hud", GameObject.Find("/ui/hud/kda"));
    }

    void Start()
    {
        // on récupère les inputs
        inputs = GameObject.Find("/utils/input_manager").GetComponent<InputManager>().inputs;
        // inputs.UI.Enable();
        // input_manager = GameObject.Find("/utils/input_manager").GetComponent<InputManager>();
        // inputs_actions = input_manager.inputs;
    }

    // UPDATE
    void Update()
    {
        // on affiche le hud si on affiche rien
        if (current_pool == "") { ShowPool("hud"); }
    }

    // POOL REGISTERING
    public void RegisterToPool(string pool_name, GameObject ui_element)
    {
        // if the gameobject is null, we return
        if (ui_element == null) { return; }

        // we create the pool if the pool does not exists
        if (!pools.Contains(pool_name)) { add_pool(pool_name); }

        // we check if the element is already in the pool
        else if (ui_pools[pool_name].Contains(ui_element))
        {
            if (debug) { Debug.LogWarning("(UI_Manager) " + ui_element.name + " tried to register to a pool it's already in : " + pool_name); }
            return;
        }

        if (debug) { Debug.Log("(UI_Manager) " + ui_element.name + " just registered to pool : " + pool_name); }

        // on ajoute l'élément au pool
        ui_pools[pool_name].Add(ui_element);
    }
    public void QuitPool(string pool_name, GameObject ui_element)
    {
        // we create the pool if the pool does not exists
        if (!pools.Contains(pool_name)) { add_pool(pool_name); }

        // we check if the element is not in the pool
        else if (!ui_pools[pool_name].Contains(ui_element))
        {
            if (debug) { Debug.LogWarning("(UI_Manager) " + ui_element.name + " tried to quit a pool it's not in : " + pool_name); }
            return;
        }

        if (debug) { Debug.Log("(UI_Manager) " + ui_element.name + " just quitted pool : " + pool_name); }

        // on retire l'élément du pool
        ui_pools[pool_name].Remove(ui_element);
    }
    private void add_pool(string pool_name)
    {
        pools.Add(pool_name);
        ui_pools[pool_name] = new List<GameObject>();
        if (debug) { Debug.Log("(UI_Manager) pool created : " + pool_name); }
    }

    // SHOW / HIDE
    public void ShowPool(string pool_name)
    {
        // we check if the pool exists
        if (!pools.Contains(pool_name))
        {
            if (debug) { Debug.LogWarning("(UI_Manager) tried to show a non-existing pool : " + pool_name); }
            return;
        }

        // we hide the current pool
        if (current_pool != "") { HidePool(current_pool); }

        // on affiche tous les éléments du pool
        if (debug) { Debug.Log("(UI_Manager) showing pool : " + pool_name); }
        foreach (GameObject ui_element in ui_pools[pool_name])
        {
            ui_element.SetActive(true);
        }

        // on désactive les inputs du joueur si c'est pas le hud
        if (pool_name == "hud") { inputs.perso.Enable(); }
        else
        {
            Debug.Log("perso enabled? " + inputs.perso.enabled);
            inputs.perso.Disable();
            Debug.Log("perso enabled after disable? " + inputs.perso.enabled);
        }


        // on met à jour le pool courant
        current_pool = pool_name;
    }
    public void HidePool(string pool_name)
    {
        // we check if the pool exists
        if (!pools.Contains(pool_name))
        {
            if (debug) { Debug.LogWarning("(UI_Manager) tried to hide a non-existing pool : " + pool_name); }
            return;
        }

        // on cache tous les éléments du pool
        if (debug) { Debug.Log("(UI_Manager) hiding pool : " + pool_name); }
        foreach (GameObject ui_element in ui_pools[pool_name])
        {
            ui_element.SetActive(false);
        }

        // on met à jour le pool courant
        current_pool = "";
    }
    public void HideEverything()
    {
        foreach (KeyValuePair<string, List<GameObject>> pool in ui_pools)
        {
            foreach (GameObject ui_element in pool.Value)
            {
                ui_element.SetActive(false);
            }
        }
    }

}