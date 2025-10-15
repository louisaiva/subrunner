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
    [SerializeField] private string hud_stack = "hud"; // the default stack when going back to hud

    [Header("Pools")]
    private List<UI_Pool> pools = new List<UI_Pool>();
    [SerializeField]
    private UI_Pool current_pool
    {
        get
        {
            if (pool_stack.Count > 0) { return pool_stack[pool_stack.Count - 1]; }
            return null;
        }
        set
        {
            if (pool_stack.Count > 0) { pool_stack[pool_stack.Count - 1] = value; }
            else { pool_stack.Add(value); }
        }
    }
    public string CurrentPool
    {
        get
        {
            if (pool_stack.Count > 0) { return pool_stack[pool_stack.Count - 1].Reference; }
            return null;
        }
    }
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

    // UI POOL SWITCH
    public void TogglePool(string pool_name)
    {
        // we check if the pool is already shown
        if (pool_name == current_pool.Reference) { SwitchToHUD(); }
        else { SwitchTo(pool_name, false); }
    }
    public void SwitchTo(string pool_name, bool force = true)
    {
        // check if we have a pool to switch to
        UI_Pool pool = GetPool(pool_name);
        if (!pool) { return; }

        if (current_transition != null)
        {
            if (log_switching) { Debug.LogWarning($"(UI_Manager) can't switch to pool : {pool.Reference} from {(current_pool != null ? current_pool.Reference : "null")} because a transition is already in progress"); }
            return;
        }
        current_transition = StartCoroutine(switch_pool_coroutine(new List<UI_Pool> { pool }, force));
    }
    public void SwitchToHUD()
    {
        if (current_transition != null)
        {
            if (log_switching) { Debug.LogWarning($"(UI_Manager) can't switch to HUD group from {(current_pool != null ? current_pool.Reference : "null")} because a transition is already in progress"); }
            return;
        }

        // we switch to the hud stack
        current_transition = StartCoroutine(switch_pool_coroutine(get_stack_from_string(hud_stack)));
    }

    // UI POOL SWITCH LOW LEVEL
    private IEnumerator switch_pool_coroutine(List<UI_Pool> stack, bool force = true)
    {
        // we check that we are not switching to the same pool
        UI_Pool pool = stack[stack.Count - 1];
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

        // we hide all stacked pool except last one (which is the current one)
        for (int i = 0; i < pool_stack.Count - 1; i++)
        {
            UI_Pool stacked_pool = pool_stack[i];
            if (stacked_pool != null)
            {
                stacked_pool.HideCoroutine(same_pool_elements, current_pool.TransitionSettings.Duration);
            }
        }

        // we hide the current pool
        if (current_pool != null) { yield return current_pool.HideCoroutine(same_pool_elements); }

        // we set the bg sibling index to current pool
        bg.transform.SetSiblingIndex(pools.IndexOf(pool));

        // we show all the pools in the pool_stack
        pool_stack.Clear();
        for (int i = 0; i < stack.Count - 1; i++)
        {
            pool_stack.Add(stack[i]);
            pool_stack[i].StackShowCoroutine(pool.TransitionSettings.Duration);
        }
        pool_stack.Add(pool);

        // we show the new pool
        yield return pool.ShowCoroutine(same_pool_elements);

        // we invoke the OnPoolSwitched event
        OnPoolSwitched?.Invoke(pool.Reference);

        // we clear the current transition
        current_transition = null;
        if (log) { Debug.Log($"(UI_Manager) switched to pool : {pool.Reference}"); }
    }
    private List<GameObject> get_common_elements(UI_Pool pool_a, UI_Pool pool_b)
    {
        if (pool_a == null || pool_b == null) { return new List<GameObject>(); }

        List<GameObject> common_elements = pool_a.UIElements;
        common_elements = common_elements.Where(x => pool_b.UIElements.Contains(x)).ToList();
        return common_elements;
    }

    // UI POOL STACK/UNSTACK & CANCELING
    public void StackPool(string pool_name)
    {
        // check if we have a pool to switch to
        UI_Pool pool = GetPool(pool_name);
        if (!pool) { return; }

        if (current_transition != null)
        {
            if (log_switching) { Debug.LogWarning($"(UI_Manager) can't switch to pool : {pool.Reference} from {(current_pool != null ? current_pool.Reference : "null")} because a transition is already in progress"); }
            return;
        }
        current_transition = StartCoroutine(stack_pool_coroutine(pool));
    }
    public void UnstackPool(string pool_name)
    {
        // check if we have a pool to unstack
        UI_Pool pool = GetPool(pool_name);
        if (!pool) { return; }
        if (!pool_stack.Contains(pool))
        {
            if (log && pool.Reference != "hud") { Debug.LogWarning("(UI_Manager) tried to unstack a pool that is not in the stack : " + pool.Reference); }
            return;
        }

        // check if we can hide the pool
        if (!pool.TransitionSettings.CanBeHidden)
        {
            if (log && pool.Reference != "hud") { Debug.LogWarning("(UI_Manager) tried to unstack a pool that cannot be hidden : " + pool.Reference); }
            return;
        }

        if (current_transition != null)
        {
            if (log_switching) { Debug.LogWarning($"(UI_Manager) can't unstack pool : {pool.Reference} from {PoolStack} because a transition is already in progress"); }
            return;
        }
        current_transition = StartCoroutine(unstack_pool_coroutine(pool));
    }
    public void UnstackCurrentPool() => UnstackPool(current_pool.Reference);
    public void CancelCurrentPool()
    {
        // check if we can cancel the pool
        if (!current_pool.TransitionSettings.CanBeCanceled)
        {
            if (log && current_pool.Reference != "hud") { Debug.LogWarning("(UI_Manager) tried to cancel a pool that cannot be canceled : " + current_pool.Reference); }
            return;
        }
        UnstackCurrentPool();
    }

    // UI POOL STACK/UNSTACK LOW LEVEL
    private IEnumerator stack_pool_coroutine(UI_Pool pool)
    {
        // we check that we are not stacking the same pool
        if (pool == current_pool) { yield break; }

        // we get the transition duration
        float duration = 0f;
        if (current_pool != null) { duration += current_pool.TransitionSettings.Duration; }
        duration += pool.TransitionSettings.Duration;

        // we get the common elements between the two pools
        // List<GameObject> same_pool_elements = get_common_elements(current_pool, pool);

        if (log) { Debug.Log($"(UI_Manager) stacking : {PoolStack} -> {PoolStack + "/" + pool.Reference} (duration : " + duration + ")"); }


        // we transition to the right bg/timescale/effect
        if (Time.timeScale != pool.TransitionSettings.TimeScale) { TransitionTimeScale(pool.TransitionSettings.TimeScale, duration); }
        if (bg.Alpha != pool.TransitionSettings.BackgroundAlpha) { TransitionBackground(pool.TransitionSettings.BackgroundAlpha, duration); }

        // we hide the current pool
        if (current_pool != null) { yield return current_pool.StackHideCoroutine(); }

        // we add the pool to the stack
        pool_stack.Add(pool);

        // we set the bg sibling index to current pool
        bg.transform.SetSiblingIndex(pools.IndexOf(pool));

        // we show the new pool
        yield return pool.ShowCoroutine();

        // we invoke the OnPoolSwitched event
        OnPoolSwitched?.Invoke(pool.Reference);

        // we clear the current transition
        current_transition = null;
        if (log) { Debug.Log($"(UI_Manager) stacked pool : {PoolStack}"); }
    }
    private IEnumerator unstack_pool_coroutine(UI_Pool pool)
    {
        // we check that we are unstacking a stacked pool
        if (!pool_stack.Contains(pool)) { yield break; }

        // we get the next pool
        List<UI_Pool> next_stack = new List<UI_Pool>(pool_stack);
        next_stack.Remove(pool);

        // if we have an empty next pool we simply switch to hud instead of unstacking
        if (next_stack.Count == 0)
        {
            if (log_switching) { Debug.Log("(UI_Manager) unstacking the last pool, switching to hud instead"); }
            yield return switch_pool_coroutine(get_stack_from_string(hud_stack));
            yield break;
        }

        // we get the next pool
        UI_Pool next_pool = next_stack[next_stack.Count - 1];

        // we get the transition duration
        float duration = 0f;
        duration += pool.TransitionSettings.Duration;
        duration += next_pool.TransitionSettings.Duration;

        if (log) { Debug.Log($"(UI_Manager) unstacking : {PoolStack} -> {PoolStack.Replace("/" + pool.Reference, "")} (duration : " + duration + ")"); }

        // we transition to the right bg/timescale/effect
        if (Time.timeScale != next_pool.TransitionSettings.TimeScale) { TransitionTimeScale(next_pool.TransitionSettings.TimeScale, duration); }
        if (bg.Alpha != next_pool.TransitionSettings.BackgroundAlpha) { TransitionBackground(next_pool.TransitionSettings.BackgroundAlpha, duration); }

        // we hide the pool
        yield return pool.HideCoroutine();

        // we remove the pool from the stack
        pool_stack.Remove(pool);

        // we set the bg sibling index to current pool
        bg.transform.SetSiblingIndex(pools.IndexOf(next_pool));

        // we show the next pool
        yield return next_pool.ShowCoroutine();

        // we invoke the OnPoolSwitched event
        OnPoolSwitched?.Invoke(next_pool.Reference);

        // we clear the current transition
        current_transition = null;
        if (log) { Debug.Log($"(UI_Manager) unstacked pool : {pool.Reference} ({PoolStack})"); }
    }


    // HUD STACKING
    public void StackOnHUD(string pool_name)
    {
        // we check if we are not already stacked in the hud
        if (hud_stack.Contains(pool_name)) { return; }
        hud_stack += "/" + pool_name;

        // we stack the pool (only if we are currently showing hud)
        if (IsOnHUD()) { StackPool(pool_name); }
    }
    public void UnstackFromHUD(string pool_name)
    {
        // we check if we are stacked in the hud
        if (!hud_stack.Contains(pool_name)) { return; }
        hud_stack = string.Join("/", hud_stack.Split('/').Where(x => x != pool_name).ToArray());

        // we unstack the pool (only if we are currently showing hud)
        if (IsOnHUD()) { UnstackPool(pool_name); }
    }
    /* private IEnumerator reapply_hud_stack_coroutine()
    {
        // we unstack all the pools
        while (pool_stack.Count > 1)
        {
            yield return unstack_pool_coroutine(pool_stack[pool_stack.Count - 1]);
        }

        // we stack all the pools in the hud stack
        foreach (string pool_name in hud_stack.Split('/').Where(x => x != "hud" && x != ""))
        {
            UI_Pool pool = GetPool(pool_name);
            if (pool != null)
            {
                yield return stack_pool_coroutine(pool);
            }
        }
    } */

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
    private List<UI_Pool> get_stack_from_string(string stack_string)
    {
        List<UI_Pool> stack = new List<UI_Pool>();
        List<string> string_stack = stack_string.Split('/').Where(x => x != "").ToList();
        for (int i = 0; i < string_stack.Count; i++)
        {
            UI_Pool pool = GetPool(string_stack[i]);
            if (pool != null) { stack.Add(pool); }
        }
        return stack;
    }
    public bool IsStacked(string pool_name)
    {
        UI_Pool pool = GetPool(pool_name);
        if (pool == null) { return false; }
        return pool_stack.Contains(pool);
    }
    public bool IsOnHUD()
    {
        return PoolStack.StartsWith("/hud");
    }
}