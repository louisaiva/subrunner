#pragma warning disable 4014
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using PrimeTween;
using System.Collections;
/// <summary>
/// This class is used to manage the UI elements
/// its transform is located at /ui
/// all their children are considered as UI elements
/// and are handled via "pools of UI elements"
/// </summary>
public class UI_Manager : Singleton<UI_Manager>
{

    [Header("Pool stack")]
    [SerializeField] private List<UI_Pool> pool_stack = new List<UI_Pool>();
    public string PoolStack => "/" + string.Join("/", pool_stack.Select(x => x.Reference).ToArray());

    [Header("Pools")]
    private List<UI_Pool> pools = new List<UI_Pool>();
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
    public event System.Action<string> OnPoolSwitched = delegate { };

    [Header("Transitions")]
    private PauseMenuBackgroundEffect bg;
    private Coroutine current_transition = null;


    [Header("Logs")]
    public bool log = false;
    public bool log_availability = false;
    public bool log_switching = false;

    // START & AWAKE
    protected override void Awake()
    {
        base.Awake();

        // we get all the pools & wake up all the pools
        foreach (Transform child in transform)
        {
            UI_Pool pool = child.GetComponent<UI_Pool>();
            if (pool == null) { continue; }
            pools.Add(pool);
            pool.gameObject.SetActive(true);
        }

        // we get the background effect
        bg = transform.Find("bg").GetComponent<PauseMenuBackgroundEffect>();

    }
    void Start()
    {
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

    // TIMESCALE & BG & EFFECTS TRANSITIONS
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

    // UI POOL SHOW/HIDE
    public void TogglePool(string pool_name)
    {
        // we check if the pool is already shown
        if (pool_name == current_pool.Reference) { SwitchTo("hud", false); }
        else { SwitchTo(pool_name, false); }
    }
    public void SwitchTo(string pool_name, bool force = true)
    {
        // check if we have a pool to switch to
        UI_Pool pool = GetPool(pool_name);
        if (!pool) { return; }

        
        // switch_to(pool, force);
        if (current_transition != null)
        {
            if (log_switching) { Debug.LogWarning($"(UI_Manager) can't switch to pool : {pool.Reference} from {(current_pool != null ? current_pool.Reference : "null")} because a transition is already in progress"); }
            return;
        }
        // if (log_switching) { Debug.Log($"(UI_Manager) trying to switch to pool : {pool.Reference} from {(current_pool != null ? current_pool.Reference : "null")}"); }
        current_transition = StartCoroutine(switch_pool_coroutine(pool, force));
    }

    /* public void StackPool(string pool_name)
    {
        // check if we have a pool to switch to
        UI_Pool pool = GetPool(pool_name);
        if (!pool) { return; }

        if (log_switching) { Debug.Log($"(UI_Manager) trying to stack pool : {pool.Reference} on top of {(current_pool != null ? current_pool.Reference : "null")}"); }
        switch_to(pool, false);
    } */

    // UI POOL SHOW/HIDE LOW LEVEL
    private IEnumerator switch_pool_coroutine(UI_Pool pool, bool force = true)
    {
        // we check that we are not switching to the same pool
        if (pool == current_pool) { yield break; }

        // we check if the current pool can be hidden (if not we can't switch)
        // checks if the current pool can be forcely hidden
        if (current_pool != null && !force && !current_pool.TransitionSettings.CanBeHidden)
        {
            if (log && current_pool.Reference != "hud") { Debug.LogWarning("(UI_Manager) tried to hide a pool that cannot be hidden : " + current_pool.Reference); }
            yield break;
        }

        // we get the transition duration
        float duration = 0f;
        if (current_pool != null) { duration += current_pool.TransitionSettings.Duration; }
        duration += pool.TransitionSettings.Duration;

        // we get the common elements between the two pools
        List<GameObject> same_pool_elements = get_common_elements(current_pool, pool);

        if (log) { Debug.Log($"(UI_Manager) switching : {(current_pool?.Reference ?? " / ")} -> {pool.Reference} (duration : " + duration + ")"); }


        // we transition to the right bg/timescale/effect
        if (Time.timeScale != pool.TransitionSettings.TimeScale) { TransitionTimeScale(pool.TransitionSettings.TimeScale, duration); }
        if (bg.Alpha != pool.TransitionSettings.BackgroundAlpha) { TransitionBackground(pool.TransitionSettings.BackgroundAlpha, duration); }

        // we hide the current pool
        if (current_pool != null) { yield return current_pool.HideCoroutine(same_pool_elements); }

        // we show the new pool
        current_pool = pool;
        yield return pool.ShowCoroutine(same_pool_elements);

        // we invoke the OnPoolSwitched event
        OnPoolSwitched?.Invoke(pool.Reference);

        // we clear the current transition
        current_transition = null;
    }
    private List<GameObject> get_common_elements(UI_Pool pool_a, UI_Pool pool_b)
    {
        if (pool_a == null || pool_b == null) { return new List<GameObject>(); }

        List<GameObject> common_elements = pool_a.UIElements;
        common_elements = common_elements.Where(x => pool_b.UIElements.Contains(x)).ToList();
        return common_elements;
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

    // CANCEL POOLS
    public void Cancel(string pool_name)
    {
        // todo make a stack so we pop the last pool right here
        // for now we just go back to hud
        SwitchTo("hud");
    }
    public void CancelCurrentPool()
    {
        // todo store a pool cancel stack to go back to previous pool

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
}