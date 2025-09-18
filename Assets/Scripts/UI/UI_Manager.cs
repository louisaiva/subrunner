#pragma warning disable 4014
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using PrimeTween;
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
    public string CurrentPool { get => current_pool.Reference; }
    public bool InPool(string pool_name)
    {
        if (current_pool == null) { return false; }
        return current_pool.Reference == pool_name;
    }
    public bool InPools(List<string> pool_names)
    {
        foreach (string pool_name in pool_names)
        {
            if (InPool(pool_name)) { return true; }
        }
        return false;
    }


    [Header("Transitions")]
    [SerializeField] protected float transition_duration = 0.2f;
    private PauseMenuBackgroundEffect bg;


    [Header("Logs")]
    public bool log = false;
    public bool log_availability = false;
    public bool log_switching = false;

    // inputs
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

        // we get the background effect
        bg = transform.Find("bg").GetComponent<PauseMenuBackgroundEffect>();

    }
    void Start()
    {
        // on récupère les inputs
        input_manager = InputManager.Instance;

        // on mets les callbacks des menus
        input_manager.inputs.menus.inventory.performed += ctx => { TogglePool("inventory"); };
        input_manager.inputs.menus.pause.performed += ctx => { TogglePool("pause"); };
        // input_manager.inputs.menus.hacking.performed += ctx => { HandleHackingInput(ctx.ReadValue<float>()); };
        input_manager.inputs.perso.select_hackable.performed += ctx => { HandleHackingInput(ctx.ReadValue<Vector2>().magnitude); };
        // inputs.menus.map.performed += ctx => { TogglePool("map"); };
        input_manager.inputs.UI.cancel.performed += ctx => { HandleCancelInput(ctx.ReadValue<float>()); };

        // we try to switch to current_pool if it is something
        if (current_pool != null)
        {
            // only for log & prototype purpose
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

        if (log_switching) { Debug.Log($"(UI_Manager) trying to switch to pool : {pool.Reference} from {(current_pool != null ? current_pool.Reference : "null")}"); }

        // we check if we have an override duration
        float duration = override_duration != default ? override_duration : transition_duration;
        switch_to(pool, force, duration);
    }
    private async void switch_to(UI_Pool pool, bool force = true, float override_duration = default)
    {
        // check if this pool is not the same as the current one
        if (pool == current_pool) { return; }

        // todo : verify that the next pool is available before switching

        // we prepare the transition duration
        float duration = override_duration != default ? override_duration : transition_duration;
        if (pool.Reference == "game_over") { duration = (pool as UI_GameOver).transition_duration; }
        else if (current_pool != null && current_pool.Reference == "game_over") { duration = (current_pool as UI_GameOver).transition_duration; }

        // we prepare the lists of the gameobjects to ignore
        List<GameObject> same_pool_elements = null;

        // we check if we have a current pool
        if (current_pool != null)
        {
            if (log_switching) { Debug.Log($"(UI_Manager - switch_to) current pool is not null : {current_pool.Reference}"); }

            // checks if the current pool can be forcely hidden
            if (!force && !current_pool.TransitionSettings.CanBeHidden)
            {
                if (log && current_pool.Reference != "hud") { Debug.LogWarning("(UI_Manager) tried to hide a pool that cannot be hidden : " + current_pool.Reference); }
                return;
            }

            if (!current_pool.Available)
            {
                if (log) { Debug.LogWarning("(UI_Manager) tried to switch pools while the current pool is not available : " + current_pool.Reference); }
                return;
            }

            if (log_switching) { Debug.Log($"(UI_Manager - switch_to) current pool is available and can be hidden"); }


            // we filter the same_pool_elements_list so only elements that are in both pools stay inside it
            List<GameObject> current_pool_elements = current_pool.UIElements;
            same_pool_elements = pool.UIElements;
            same_pool_elements = same_pool_elements.Where(x => current_pool_elements.Contains(x)).ToList();

            if (log_switching) { Debug.Log($"(UI_Manager - switch_to) same pool elements contains {same_pool_elements.Count} elements"); }


            // we transition to the right bg/timescale/effect
            if (current_pool.TransitionSettings.TimeScale != pool.TransitionSettings.TimeScale)
            {
                TransitionTimeScale(pool.TransitionSettings.TimeScale, duration);
            }
            if (log_switching) { Debug.Log($"(UI_Manager - switch_to) transitionned time scale"); }

            if (current_pool.TransitionSettings.BackgroundAlpha != pool.TransitionSettings.BackgroundAlpha)
            {
                TransitionBackground(pool.TransitionSettings.BackgroundAlpha, duration);
            }
            if (log_switching) { Debug.Log($"(UI_Manager - switch_to) transitionned background"); }

            // we hide the current pool
            if (log_switching) { Debug.Log($"(UI_Manager - switch_to) launching current pool hide"); }
            await current_pool.Hide(duration / 2f, same_pool_elements);
            if (log_switching) { Debug.Log($"(UI_Manager - switch_to) current pool hidden successfully"); }
        }
        else
        {
            // on active le background & time parameters
            TransitionTimeScale(pool.TransitionSettings.TimeScale, duration / 2f);
            TransitionBackground(pool.TransitionSettings.BackgroundAlpha, duration / 2f);
            if (log_switching) { Debug.Log($"(UI_Manager - switch_to) current pool is null, transitionned bg & time scale"); }

        }

        if (log) { Debug.Log("(UI_Manager) switching to pool : " + pool.Reference); }

        // we show the new pool
        current_pool = pool;
        if (log_switching) { Debug.Log($"(UI_Manager - switch_to) launching next pool show"); }
        await current_pool.Show(duration / 2f, same_pool_elements);
        if (log_switching) { Debug.Log($"(UI_Manager - switch_to) next pool shown successfully"); }
    }

    // GETTERS
    public UI_Pool GetPool(string reference)
    {
        // we try to find the pool
        foreach (UI_Pool pool in pools)
        {
            if (pool.Reference == reference) { return pool; }
        }

        if (log) { Debug.LogWarning("(UI_Manager) tried to get a non-existing pool : " + reference); }

        // if we don't find it, we return null
        return null;
    }

    // INPUT HANDLING
    private void HandleCancelInput(float input)
    {
        // todo store a pool cancel stack to go back to previous pool

        if (input > 0.5f) { return; } // we only handle the release of the input

        // check if we can cancel the pool
        if (!current_pool.TransitionSettings.CanBeCanceled)
        {
            if (log && current_pool.Reference != "hud") { Debug.LogWarning("(UI_Manager) tried to cancel a pool that cannot be canceled : " + current_pool.Reference); }
            return;
        }

        if (current_pool.Reference == "hdd")
        {
            // we switch to inventory
            SwitchTo("inventory");
            return;
        }

        // we switch to hud
        SwitchTo("hud");
    }
    private void HandleHackingInput(float input)
    {

        // we activate the hacking ui when input is pressed > 0.5
        // and disable it when released < 0.5
        if (input < InputManager.Instance.JOYSTICK_MIN_THRESHOLD)
        {
            if (current_pool.Reference == "hacking") { SwitchTo("hud"); }
            return;
        }

        // if (log) { Debug.Log("(UI_Manager) hacking menu input received : " + input); }

        // we check if we can switch to hacking
        if (current_pool.Reference == "hud" && GetPool("hacking").Available)
        {
            SwitchTo("hacking");
        }
    }


    // TRANSITIONS
    public async Awaitable TransitionBackground(float bg_alpha, float duration)
    {
        bg.TransitionEffect(bg_alpha > 0f, duration);
        await bg.TransitionAlpha(bg_alpha > 0f, duration, bg_alpha);
    }
    public async Awaitable TransitionTimeScale(float time_scale, float duration)
    {
        // we check if we have an override final timescale
        float final_timescale = time_scale;
        if (Time.timeScale == final_timescale) { return; } // if we are already at the right timescale, we do nothing

        await Tween.GlobalTimeScale(final_timescale, duration, Ease.OutQuad);
    }

}