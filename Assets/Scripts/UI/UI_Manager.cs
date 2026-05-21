#pragma warning disable 4014
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
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
    public event System.Action<string> OnPoolSwitched = delegate { }; // ? useful ??

    [Header("Transitions")]
    private PauseMenuBackgroundEffect bg;
    public PauseMenuBackgroundEffect BackgroundEffect => bg;
    private Coroutine current_transition = null;
    public bool IsInTransition => current_transition != null;


    [Header("Logs")]
    public bool log = false;
    public bool log_extended = false;

    // START & AWAKE
    protected override void Awake()
    {
        base.Awake();

        // we get all the pools & wake up all the pools
        foreach (Transform child in transform)
        {
            UI_Pool pool = child.GetComponent<UI_Pool>();
            if (pool == null) { continue; }
            if (!pool.gameObject.activeSelf) { continue; }
            pools.Add(pool);
            // pool.gameObject.SetActive(true);
        }

        // we get the background effect
        bg = transform.Find("bg").GetComponent<PauseMenuBackgroundEffect>();

    }
    void Start()
    {
        // we try to switch to current_pool if it is something
        if (current_pool == null || IsInTransition) { return; }
        
        // only for log & prototype purpose
        string start_pool = current_pool.Reference;
        current_pool = null;
        SwitchTo(start_pool);        
    }

    /// <summary>
    /// should happen before UI_Manager.Start() to take effect !!
    /// this method assign the pool to the pool stack, in order for
    /// UI_Manager to switch to it on Start()
    /// </summary>
    /// <param name="pool"></param>
    public void AssignStartPool(UI_Pool pool)
    {
        if (pool == null) { return; }
        pool_stack.Add(pool);
    }

    // TIMESCALE & BG & EFFECTS TRANSITIONS
    public void TransitionEffects(UI_PoolSettings settings, float duration)
    {
        // we transition to the right bg/timescale/effect
        if (Time.timeScale != settings.TimeScale) { TransitionTimeScale(settings.TimeScale, duration); }
        if (bg.Alpha != settings.BackgroundAlpha) { bg.TransitionAlpha(settings.BackgroundAlpha > 0f, duration, settings.BackgroundAlpha); }
        PostProcessManager ppm = PostProcessManager.Instance;
        if (ppm.Chroma != settings.ChromaticAberration) { ppm.TransitionChroma(settings.ChromaticAberration, duration); }
        if (ppm.Bloom != settings.Bloom) { ppm.TransitionBloom(settings.Bloom, duration); }
    }
    public async Awaitable TransitionTimeScale(float time_scale, float duration)
    {
        // we check if we have an override final timescale
        float final_timescale = time_scale;
        if (Time.timeScale == final_timescale) { return; } // if we are already at the right timescale, we do nothing

        await Tween.GlobalTimeScale(final_timescale, duration, Ease.OutQuad);
    }

    // UI POOL SWITCH
    public void TogglePool(string pool_name, bool stacking = false)
    {
        // if we don't want stacking we simmply switch to the pool
        if (!stacking)
        {
            // we check if the pool is already shown
            if (current_pool != null && pool_name == current_pool.Reference) { SwitchToHUD(); }
            else { SwitchTo(pool_name, false); }
            return;
        }

        // otherwise we stack/unstack it
        if (IsStacked(pool_name)) { UnstackPool(pool_name); }
        else { StackPool(pool_name); }
    }

    /// <summary>
    /// this is the main method of the UI_Manager.
    /// it is used to switch between UI_Pool. call the switch_pool_coroutine that allow us to switch between multiple UI_Pool stacks.
    /// 
    /// </summary>
    /// <param name="pool_name">the pool we want to switch to. must exist</param>
    /// <param name="force">should we force the switching even if the current pool is not hiddenable ?</param>
    /// <param name="override_transition">should we force the switching even if we are still in transition ? may break things</param>
    public void SwitchTo(string pool_name, bool force = false, bool override_transition = false)
    {
        // check if we have a pool to switch to
        UI_Pool pool = GetPool(pool_name);
        if (!pool) { return; }

        if (current_transition != null && !override_transition)
        {
            if (log_extended) { Debug.LogWarning($"(UI_Manager) can't switch to pool : {pool.Reference} from {(current_pool != null ? current_pool.Reference : "null")} because a transition is already in progress"); }
            return;
        }
        else if (current_transition != null && override_transition)
        {
            if (log) { Debug.LogWarning($"(UI_Manager) overriding transition to switch to pool : {pool.Reference} /!\\ will break the last transition"); }
            StopCoroutine(current_transition);
            current_transition = null;
        }
        current_transition = StartCoroutine(switch_pool_coroutine(new List<UI_Pool> { pool }, force));
    }
    public void SwitchToHUD(bool force = false, bool override_transition = false)
    {
        if (current_transition != null && !override_transition)
        {
            if (log_extended) { Debug.LogWarning($"(UI_Manager) can't switch to HUD group from {(current_pool != null ? current_pool.Reference : "null")} because a transition is already in progress"); }
            return;
        }
        else if (current_transition != null && override_transition)
        {
            if (log) { Debug.LogWarning($"(UI_Manager) overriding transition to switch to HUD group /!\\ will break the last transition"); }
            StopCoroutine(current_transition);
            current_transition = null;
        }

        // we switch to the hud stack
        current_transition = StartCoroutine(switch_pool_coroutine(get_stack_from_string(hud_stack), force));

    }

    // UI POOL SWITCH LOW LEVEL
    private IEnumerator switch_pool_coroutine(List<UI_Pool> stack, bool force = false)
    {
        // we check that we are not switching to the same pool
        UI_Pool pool = stack[stack.Count - 1];
        if (pool == current_pool) { yield break; }

        // we check if the current pool can be hidden (if not we can't switch)
        // checks if the current pool can be forcely hidden
        if (current_pool != null && !force && !current_pool.Settings.CanBeHidden)
        {
            if (log_extended) { Debug.LogWarning("(UI_Manager) tried to hide a pool that cannot be hidden : " + current_pool.Reference); }
            yield break;
        }

        // we get the transition duration
        float duration = 0f;
        if (current_pool != null) { duration += current_pool.Settings.Duration; }
        duration += pool.Settings.Duration;

        // we get the common elements between the two pools
        List<GameObject> same_pool_elements = get_common_elements(current_pool, pool);

        if (log) { Debug.Log($"(UI_Manager) switching : {(current_pool?.Reference ?? " / ")} -> {pool.Reference} (duration : " + duration + ")"); }

        // transition effects
        TransitionEffects(pool.Settings, duration);

        // we hide all stacked pool except last one (which is the current one)
        for (int i = 0; i < pool_stack.Count - 1; i++)
        {
            UI_Pool stacked_pool = pool_stack[i];
            if (stacked_pool != null)
            {
                if (log_extended) { Debug.Log($"(UI_Manager) hiding stacked pool : {stacked_pool.Reference}"); }
                stacked_pool.StartCoroutine(stacked_pool.HideCoroutine(same_pool_elements, current_pool.Settings.Duration));
            }
        }

        // we hide the current pool
        if (current_pool != null) { yield return current_pool.HideCoroutine(same_pool_elements); }

        // we set the bg sibling index to current pool
        bg.transform.SetSiblingIndex(pools.IndexOf(pool));

        // we show all the pools in the pool_stack
        pool_stack.Clear();
        bool is_hud = stack[0].Reference == "hud";
        for (int i = 0; i < stack.Count - 1; i++)
        {
            UI_Pool stacked_pool = stack[i];
            pool_stack.Add(stacked_pool);
            if (log_extended) { Debug.Log($"(UI_Manager) showing stacked pool : {stacked_pool.Reference}"); }
            stacked_pool.StartCoroutine(stacked_pool.StackShowCoroutine(pool.Settings.Duration, enable: is_hud));
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
    public void StackPool(string pool_name, bool override_transition = false)
    {
        // check if we have a pool to switch to
        UI_Pool pool = GetPool(pool_name);
        if (!pool) { return; }

        if (current_transition != null && !override_transition)
        {
            if (log_extended) { Debug.LogWarning($"(UI_Manager) can't stack pool : {pool.Reference} on {(current_pool != null ? current_pool.Reference : "/")} because a transition is already in progress"); }
            return;
        }
        else if (current_transition != null && override_transition)
        {
            if (log) { Debug.LogWarning($"(UI_Manager) override stacking pool : {pool.Reference} /!\\ may break the last transition"); }
            StopCoroutine(current_transition);
            current_transition = null;
        }
        current_transition = StartCoroutine(stack_pool_coroutine(pool));
    }
    public void UnstackPool(string pool_name, bool override_transition = false)
    {
        // check if we have a pool to unstack
        UI_Pool pool = GetPool(pool_name);
        if (!pool) { return; }
        if (!pool_stack.Contains(pool) && current_transition == null)
        {
            if (log_extended) { Debug.LogWarning("(UI_Manager) tried to unstack a pool that is not in the stack : " + pool.Reference); }
            return;
        }

        // check if we can hide the pool
        if (!pool.Settings.CanBeHidden)
        {
            if (log_extended) { Debug.LogWarning("(UI_Manager) tried to unstack a pool that cannot be hidden : " + pool.Reference); }
            return;
        }

        if (current_transition != null && !override_transition)
        {
            if (log_extended) { Debug.LogWarning($"(UI_Manager) can't unstack pool : {pool.Reference} from {PoolStack} because a transition is already in progress"); }
            return;
        }
        else if (current_transition != null && override_transition)
        {
            if (log) { Debug.LogWarning($"(UI_Manager) override stacking pool : {pool.Reference} /!\\ may break the last transition"); }
            StopCoroutine(current_transition);
            current_transition = null;
        }
        current_transition = StartCoroutine(unstack_pool_coroutine(pool));
    }
    public void UnstackCurrentPool(bool override_transition = false) => UnstackPool(current_pool.Reference, override_transition);
    public void CancelCurrentPool()
    {
        // check if we can cancel the pool
        if (current_pool == null)
        {
            if (log_extended) { Debug.LogWarning("(UI_Manager) tried to cancel a pool while no pool is currently shown"); }
            return;
        }
        if (!current_pool.Settings.CanBeCanceled)
        {
            if (log_extended && !IsOnHUD()) { Debug.LogWarning("(UI_Manager) tried to cancel a pool that cannot be canceled : " + current_pool.Reference); }
            return;
        }
        UnstackCurrentPool();
    }

    // HUD STACKING
    /* public void StackOnHUD(string pool_name, bool override_transition = false)
    {
        // we check if we are not already stacked in the hud
        if (hud_stack.Contains(pool_name)) { return; }
        hud_stack += "/" + pool_name;

        if (log_extended) { Debug.Log("(UI_Manager) added " + pool_name + " to hud stack, new hud stack : " + hud_stack); }

        // we stack the pool (only if we are currently showing hud)
        if (IsOnHUD()) { StackPool(pool_name, override_transition); }
    }
    public void UnstackFromHUD(string pool_name, bool override_transition = false)
    {
        // we check if we are stacked in the hud
        if (!hud_stack.Contains(pool_name)) { return; }
        hud_stack = string.Join("/", hud_stack.Split('/').Where(x => x != pool_name).ToArray());

        if (log_extended) { Debug.Log("(UI_Manager) removed " + pool_name + " from hud stack, new hud stack : " + hud_stack); }

        // we unstack the pool (only if we are currently showing hud)
        if (IsOnHUD()) { UnstackPool(pool_name, override_transition); }
    } */

    // UI POOL STACK/UNSTACK LOW LEVEL
    private IEnumerator stack_pool_coroutine(UI_Pool pool)
    {
        // we check that we are not stacking the same pool
        if (pool == current_pool) { yield break; }

        // we get the transition duration
        float duration = 0f;
        if (current_pool != null) { duration += current_pool.Settings.Duration; }
        duration += pool.Settings.Duration;

        // we get the common elements between the two pools
        if (log) { Debug.Log($"(UI_Manager) stacking : {PoolStack} -> {PoolStack + "/" + pool.Reference} (duration : " + duration + ")"); }


        // we transition the effects
        TransitionEffects(pool.Settings, duration);

        // we hide the current pool
        bool is_on_hud = pool_stack.Contains(GetPool("hud"));
        if (current_pool != null) { yield return current_pool.StackHideCoroutine(disable: !is_on_hud); }

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
            if (log_extended) { Debug.Log("(UI_Manager) unstacking the last pool, switching to hud instead"); }
            yield return switch_pool_coroutine(get_stack_from_string(hud_stack));
            yield break;
        }

        // we get the next pool
        UI_Pool next_pool = next_stack[next_stack.Count - 1];

        // we get the transition duration
        float duration = 0f;
        duration += pool.Settings.Duration;
        duration += next_pool.Settings.Duration;

        if (log) { Debug.Log($"(UI_Manager) unstacking : {PoolStack} -> {PoolStack.Replace("/" + pool.Reference, "")} (duration : " + duration + ")"); }

        // we transition to the right bg/timescale/effect
        // if (Time.timeScale != next_pool.Settings.TimeScale) { TransitionTimeScale(next_pool.Settings.TimeScale, duration); }
        // if (bg.Alpha != next_pool.Settings.BackgroundAlpha) { TransitionBackground(next_pool.Settings.BackgroundAlpha, duration); }
        // if (bg.Chroma != next_pool.Settings.ChromaticAberration) { TransitionChroma(next_pool.Settings.ChromaticAberration, duration); }
        TransitionEffects(next_pool.Settings, duration);

        // we hide the pool
        yield return pool.HideCoroutine();

        // we remove the pool from the stack
        pool_stack.Remove(pool);

        // we set the bg sibling index to current pool
        bg.transform.SetSiblingIndex(pools.IndexOf(next_pool));

        // we show the next pool
        yield return next_pool.ShowCoroutine(was_stacked: true);

        // we invoke the OnPoolSwitched event
        OnPoolSwitched?.Invoke(next_pool.Reference);

        // we clear the current transition
        current_transition = null;
        if (log) { Debug.Log($"(UI_Manager) unstacked pool : {pool.Reference} ({PoolStack})"); }
    }




    // SPECIFIC UI POOLS THAT NEED REFRESHING
    // todo rework this method so it can take non-inventory menu UI_Pool and still work (ex : UI_Chest)
    public void RefreshInventoryMenu()
    {
        // si on a un UI_InventoryMenu dans nos uis alors on refresh ses UI_ItemPools
        if (CurrentPool != "inventory") { return; }
        UI_InventoryMenu inventory_menu = GetPool<UI_InventoryMenu>();
        if (inventory_menu == null) { return; }
        inventory_menu.RefreshItemPools();
    }




    // UI POPUPS
    public void OpenInputPopup(string title, string placeholder, System.Action<string> on_validate)
    {
        // set the input slot callback
        UI_InputText input = GetPool<UI_PopupInputText>().InputText;
        input.RegisterInput(title, placeholder, new List<System.Action<string>> { on_validate, (string x) => { CloseInputPopup(x); } });

        // show the stacked simple_input pool
        StackPool("popup_text");
    }
    public void CloseInputPopup(string text="")
    {
        UnstackPool("popup_text");
    }



    public void OpenQuestionPopup(string title, string question, System.Action on_validate)
    {
        OpenQuestionPopup(title, question, new List<System.Action> { on_validate });
    }
    public void OpenQuestionPopup(string title, string question, List<System.Action> on_validate)
    {
        // set the input slot callback
        if (!TryGetPool<UI_PopupOkNo>(out UI_PopupOkNo popup)) { return; }

        // add the popup closing callback to the on_validate callbacks
        // on_validate.Insert(0, () => { CloseQuestionPopup(); });
        popup.RegisterDialog(title, question, on_validate);

        // show the stacked popup ok no pool
        StackPool("popup_okno");
    }
    /* public void CloseQuestionPopup()
    {
        UnstackPool("popup_okno");
    } */






    // GETTERS
    public UI_Pool GetCurrentPool()
    {
        return current_pool;
    }
    public UI_Pool GetPool(string reference)
    {
        // we try to find the pool
        foreach (UI_Pool pool in pools)
        {
            if (pool.Reference == reference) { return pool; }
        }

        if (log_extended) { Debug.LogWarning("(UI_Manager) tried to get a non-existing pool : " + reference); }

        // if we don't find it, we return null
        return null;
    }
    public T GetPool<T>() where T : UI_Pool
    {
        // we try to find the pool
        for (int i = 0; i < pools.Count; i++)
        {
            UI_Pool pool = pools[i];
            if (pool is T typed_pool) { return typed_pool; }
        }

        Debug.LogError($"(UI_Manager) did not find any matching pool of type {typeof(T).Name}");

        return null;
    }
    public bool TryGetPool<T>(out T pool) where T : UI_Pool
    {
        pool = GetPool<T>();
        return pool != null;
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



    // STATIC METHODS
    public static Vector2 WorldToCanvasLocal(Vector3 worldPos, RectTransform canvasRect, Canvas canvas, Camera worldCamera)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPos);
        Camera uiCamera = null;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
        {
            uiCamera = canvas.worldCamera;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            uiCamera,
            out Vector2 localPoint
        );

        return localPoint;
    }
    private Canvas _canvas;
    public Canvas Canvas
    {
        get
        {
            if (_canvas == null) { _canvas = GetComponent<Canvas>(); }
            if (_canvas == null) { Debug.LogError($"(UI_Manager) No Canvas found on the {name} game object. Please add one to the scene."); }
            return _canvas;
        }
    }


    // CALLER METHODS
    public void ChangeUIMode(string mode = "screenspace_overlay")
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) { return; }

        if (mode == "worldspace")
        {
            canvas.renderMode = RenderMode.WorldSpace;
            return;
        }
        if (mode == "screenspace_camera")
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            return;
        }
        if (mode == "screenspace_overlay")
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            return;
        }
    }
}