using UnityEngine;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// This class is used to manage the UI elements
/// its transform is located at /ui
/// all their children are considered as UI elements
/// and are handled via "pools of UI elements"
/// </summary>

public class UI_Manager : MonoBehaviour
{

    [Header("Pools")]
    [SerializeField] private List<UI_Pool> pools = new List<UI_Pool>();
    [SerializeField] private UI_Pool current_pool;
    // [SerializeField] private UI_Pool last_pool;
    public string CurrentPool { get => current_pool.Reference; }

    


    [Header("Logs")]
    public bool debug = false;

    // inputs
    private PlayerInputActions inputs;

    // START
    private void Awake()
    {
        // we wake up all the pools
        foreach (UI_Pool pool in pools)
        {
            pool.gameObject.SetActive(true);
        }
    }
    void Start()
    {
        // on récupère les inputs
        inputs = GameObject.Find("/utils/input_manager").GetComponent<InputManager>().inputs;

        // on mets les callbacks des menus
        inputs.menus.inventory.performed += ctx => { TogglePool("inventory"); };
        inputs.menus.pause.performed += ctx => { TogglePool("pause"); };
        // inputs.menus.map.performed += ctx => { TogglePool("map"); };
        inputs.UI.cancel.performed += ctx => { SwitchTo("hud"); };

        // we try to switch to current_pool if it is something
        if (current_pool != null)
        {
            // only for debug & prototype purpose
            string start_pool = current_pool.Reference;
            current_pool = null;
            SwitchTo(start_pool);
            return;
        }

        // on show le hud
        SwitchTo("hud");
    }


    // UI POOL MANAGEMENT
    public void TogglePool(string pool_name)
    {
        // we check if the pool is already shown
        if (pool_name == current_pool.Reference) { SwitchTo("hud");}
        else { SwitchTo(pool_name); }
    }
    public void SwitchTo(string pool_name)
    {
        // check if we have a pool to switch to
        UI_Pool pool = GetPool(pool_name);
        if (!pool) { return; }
        SwitchTo(pool);
    }
    public void SwitchTo(UI_Pool pool)
    {
        // check if this pool is not the same as the current one
        if (pool == current_pool) { return; }

        // we check if we have a current pool
        if (current_pool != null)
        {
            // checks if the current pool can be forcely hidden
            if (!current_pool.CanBeHidden)
            {
                if (debug) { Debug.LogWarning("(UI_Manager) tried to hide a pool that cannot be hidden : " + current_pool.Reference); }
                return;
            }

            // we hide the current pool
            current_pool.Hide();
            // last_pool = current_pool;
        }

        // we show the new pool
        current_pool = pool;
        current_pool.Show();

        // we activate the cancel callback if needed
        if (current_pool.HasCancelAction) { inputs.UI.cancel.Enable(); }
        else { inputs.UI.cancel.Disable(); }
    }

    // GETTERS
    public UI_Pool GetPool(string reference)
    {
        // we try to find the pool
        foreach (UI_Pool pool in pools)
        {
            if (pool.Reference == reference) { return pool; }
        }

        if (debug) { Debug.LogWarning("(UI_Manager) tried to get a non-existing pool : " + reference); }

        // if we don't find it, we return null
        return null;
    }
}