#pragma warning disable 4014
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using PrimeTween;
#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// This class is used to manage the UI elements
/// its transform is located at /ui
/// all their children are considered as UI elements
/// and are handled via "pools of UI elements"
/// </summary>
public class UI_Manager : Singleton<UI_Manager>
{

    [Header("Pools")]
    [SerializeField] private List<UI_Pool> pools = new List<UI_Pool>();
    [SerializeField] private UI_Pool current_pool;
    // [SerializeField] private UI_Pool last_pool;
    public string CurrentPool { get => current_pool.Reference; }


    [Header("Background effect")]
    [SerializeField] protected Image bg;
    [SerializeField] protected Vector2Int bg_alpha_range = new Vector2Int(0, 245);
    [SerializeField] protected float transition_duration = 0.2f;



    [Header("Logs")]
    public bool debug = false;

    // inputs
    // private PlayerInputActions inputs;
    private InputManager input_manager;

    // START
    protected override void Awake()
    {
        base.Awake();

        // we wake up all the pools
        foreach (UI_Pool pool in pools)
        {
            pool.gameObject.SetActive(true);
        }

    }
    void Start()
    {
        // on récupère les inputs
        input_manager = GameObject.Find("/utils/input_manager").GetComponent<InputManager>();

        // on mets les callbacks des menus
        input_manager.inputs.menus.inventory.performed += ctx => { TogglePool("inventory"); };
        input_manager.inputs.menus.pause.performed += ctx => { TogglePool("pause"); };
        // inputs.menus.map.performed += ctx => { TogglePool("map"); };
        input_manager.inputs.UI.cancel.performed += ctx => { HandleCancelInput(ctx.ReadValue<float>()); };

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
        if (pool_name == current_pool.Reference) { SwitchTo("hud", false); }
        else { SwitchTo(pool_name, false); }
    }
    public void SwitchTo(string pool_name, bool force = true, float override_duration = default)
    {
        // check if we have a pool to switch to
        UI_Pool pool = GetPool(pool_name);
        if (!pool) { return; }

        // we check if we have an override duration
        float duration = override_duration != default ? override_duration : transition_duration;
        switch_to(pool, force, duration);
    }
    private async void switch_to(UI_Pool pool, bool force = true, float override_duration = default)
    {
        
        // check if this pool is not the same as the current one
        if (pool == current_pool) { return; }

        // we prepare the transition duration
        float duration = override_duration != default ? override_duration : transition_duration;
        if (pool.Reference == "game_over") { duration = (pool as UI_GameOver).transition_duration; }
        else if (current_pool != null && current_pool.Reference == "game_over") { duration = (current_pool as UI_GameOver).transition_duration; }

        // we check if we have a current pool
        if (current_pool != null)
        {
            // checks if the current pool can be forcely hidden
            if (!force && !current_pool.CanBeHidden)
            {
                if (debug) { Debug.LogWarning("(UI_Manager) tried to hide a pool that cannot be hidden : " + current_pool.Reference); }
                return;
            }

            if (!current_pool.Available)
            {
                if (debug) { Debug.LogWarning("(UI_Manager) tried to switch to a pool that is not available : " + current_pool.Reference); }
                return;
            }


            // we transition to the right bg/timescale effect
            if (current_pool.StopTime != pool.StopTime)
            {
                float final_timescale = pool.StopTime ? 0f : 1f;
                // if (pool.Reference == "game_over") { final_timescale = (pool as UI_GameOver).final_timescale; }
                TransitionTimeScale(pool.StopTime, duration, final_timescale );
            }
            if (current_pool.HasBackground != pool.HasBackground) { TransitionBackground(pool.HasBackground, duration); }
            
            // we hide the current pool
            await current_pool.Hide(duration / 2f);
        }
        else
        {
            // on active le background & time parameters
            TransitionTimeScale(pool.StopTime, duration / 2f);
            TransitionBackground(pool.HasBackground, duration / 2f);
        }

        // we show the new pool
        current_pool = pool;
        await current_pool.Show(duration / 2f);
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

    // CANCEL INPUT HANDLING
    private void HandleCancelInput(float input)
    {
        if (input > 0.5f) { return; } // we only handle the release of the input

        // check if we can cancel the pool
        if (!current_pool.CanBeCanceled)
        {
            if (debug) { Debug.LogWarning("(UI_Manager) tried to cancel a pool that cannot be canceled : " + current_pool.Reference); }
            return;
        }

        // we switch to hud
        SwitchTo("hud");
    }

    // TRANSITIONS
    public async Awaitable TransitionBackground(bool show, float duration)
    {
        await bg.GetComponent<PauseMenuBackgroundEffect>().Transition(show, duration);
    }
    public async Awaitable TransitionTimeScale(bool stop_time, float duration, float override_final_timescale = default)
    {
        // we check if we have an override final timescale
        float final_timescale = stop_time ? 0f : 1f;
        if (override_final_timescale != default) { final_timescale = override_final_timescale; }

        await Tween.GlobalTimeScale(final_timescale, duration, Ease.OutQuad);
    }

    // todo move this to PauseMenuBackgroundEffect
#if UNITY_EDITOR
    [CustomEditor(typeof(UI_Manager))]
    public class UI_ManagerEditor : Editor
    {
        private bool bg_showed = false;
        public override void OnInspectorGUI()
        {
            UI_Manager manager = (UI_Manager)target;
            if (!bg_showed && GUILayout.Button("Show Background"))
            {
                manager.bg.color = new Color(manager.bg.color.r, manager.bg.color.g, manager.bg.color.b, manager.bg_alpha_range.y / 255f);
                bg_showed = true;
            }
            if (bg_showed && GUILayout.Button("Hide Background"))
            {
                manager.bg.color = new Color(manager.bg.color.r, manager.bg.color.g, manager.bg.color.b, manager.bg_alpha_range.x / 255f);
                bg_showed = false;
            }

            DrawDefaultInspector();
        }
    }
#endif
}