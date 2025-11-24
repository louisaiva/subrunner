#pragma warning disable 4014
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Pool : MonoBehaviour
{
    [Header("Pool parameters")]
    public string Reference = "pool";
    public bool Showed = false;
    public bool Stacked = false; // if true the pool is stacked below another one
    private bool prepared = false;

    [Header("Transition parameters")]
    public UI_PoolSettings Settings;
    // public PoolTransitionSettings TransitionSettings = PoolTransitionSettings.InGameDefault;
    protected Coroutine current_transition = null;


    [Header("Pool navigation parameters")]
    [SerializeField] protected float angle_threshold = 45f;
    [SerializeField] protected float angle_multiplicator = 0f;

    [Header("UI Elements")]
    [SerializeField] protected List<GameObject> ui_elements = new List<GameObject>();
    public List<GameObject> UIElements => ui_elements;

    [Header("Stacked UI Elements")]
    [SerializeField] protected List<GameObject> stacked_elements = new List<GameObject>(); // elements that will stay showed when stacking a pool on top of this pool
    // stacked elements must also be in ui_elements

    [Header("Logs")]
    [SerializeField] protected bool log = false;
    [SerializeField] protected bool log_elements_showing = false;

    // AWAKE
    protected virtual void Awake()
    {
        if (prepared) { return; }
        // we hide the pool for the first time
        for (int i = 0; i < ui_elements.Count; i++)
        {
            ui_elements[i].SetActive(Showed);
        }
    }

    // COROUTINE GESTION
    private void StopCoroutineIfAny()
    {
        if (current_transition == null) { return; }
    
        // on arrete la current_transition
        StopCoroutine(current_transition);
        current_transition = null;
        if (log) { Debug.Log("(UI_Pool) stopped current transition for pool : " + Reference); }
    }

    // SHOW / HIDE
    public IEnumerator ShowCoroutine(List<GameObject> dont_show = null, float duration_override = -1f, bool was_stacked = false)
    {
        // stop any started coroutine
        StopCoroutineIfAny();

        // on lance l'affichage
        current_transition = StartCoroutine(show_coroutine(dont_show, duration_override, was_stacked));
        yield return current_transition;

        Showed = true;
        Stacked = false;

        // on enable
        yield return StartCoroutine(enable_coroutine());

        // on clear la transition
        current_transition = null;
    }
    public IEnumerator HideCoroutine(List<GameObject> dont_hide = null, float duration_override = -1f)
    {
        // stop any started coroutine
        StopCoroutineIfAny();

        // on lance le disabling
        yield return StartCoroutine(disable_coroutine());
        // disable();

        // on lance le hiding
        current_transition = StartCoroutine(hide_coroutine(dont_hide, duration_override));
        yield return current_transition;

        // on clear la transition
        current_transition = null;
        Showed = false;
        Stacked = false;
    }
    public IEnumerator StackHideCoroutine(float duration_override = -1f, bool disable = true)
    {
        // stop any started coroutine
        StopCoroutineIfAny();

        // on lance le disabling
        if (disable) { yield return StartCoroutine(disable_coroutine()); }

        // on lance le hiding
        current_transition = StartCoroutine(hide_coroutine(stacked_elements, duration_override, stacking: true));
        yield return current_transition;

        // on clear la transition
        current_transition = null;
        Showed = true;
        Stacked = true;
    }
    public IEnumerator StackShowCoroutine(float duration_override = -1f, bool enable = true)
    {
        // stop any started coroutine
        StopCoroutineIfAny();

        // on veut afficher que les stacked elements !
        List<GameObject> dont_show = new List<GameObject>();
        for (int i = 0; i < ui_elements.Count; i++)
        {
            if (!stacked_elements.Contains(ui_elements[i])) { dont_show.Add(ui_elements[i]); }
        }

        // on lance l'affichage
        current_transition = StartCoroutine(show_coroutine(dont_show, duration_override));
        yield return current_transition;

        Showed = true;
        Stacked = true;
        
        // on enable
        if (enable) { yield return StartCoroutine(enable_coroutine()); }

        // on clear la transition
        current_transition = null;
    }

    // LOW SHOWING
    protected virtual IEnumerator show_coroutine(List<GameObject> dont_show = null, float duration_override = -1f, bool was_stacked = false)
    {
        if (Settings.UsePersoInputs) { InputManager.Instance.EnablePersoInputs(); }

        // on récupère la duration
        float duration = duration_override >= 0f ? duration_override : Settings.Duration;

        // on lance la transitions des elements avec transitionners et on retient les autres
        List<GameObject> uis_without_transitioner = new List<GameObject>();
        for (int i = 0; i < ui_elements.Count; i++)
        {
            GameObject ui = ui_elements[i];
            if (dont_show != null && dont_show.Contains(ui)) { continue; }

            // on regarde si l'element a un transitioner
            Transitioner transitioner = ui.GetComponent<Transitioner>();
            if (transitioner == null) { uis_without_transitioner.Add(ui); continue; }

            // on lance la transition
            ui_elements[i].SetActive(true);
            transitioner.Show(duration);
            if (log_elements_showing) { Debug.Log($"(UI_Pool) showing ui element {ui.name} with transitioner (duration : {duration})"); }
        }

        // on attend la fin de la transition
        yield return new WaitForSecondsRealtime(duration);

        // on affiche les éléments restants
        for (int i = 0; i < uis_without_transitioner.Count; i++)
        {
            uis_without_transitioner[i].SetActive(true);
            if (log_elements_showing) { Debug.Log($"(UI_Pool) showing ui element {uis_without_transitioner[i].name} instantly"); }
        }

        // on desactive les perso inputs si on ne les utilise pas
        if (!Settings.UsePersoInputs) { InputManager.Instance.DisablePersoInputs(); }
        if (log) { Debug.Log("(UI_Pool) show_coroutine succeeded ! pool : " + Reference); }
    }
    protected virtual IEnumerator hide_coroutine(List<GameObject> dont_hide = null, float duration_override = -1f, bool stacking = false)
    {
        // on récupère la duration
        float duration = duration_override >= 0f ? duration_override : Settings.Duration;

        // on lance la transitions des elements avec transitionners et on cache direct les autres
        List<GameObject> uis_with_transitioner = new List<GameObject>();
        for (int i = 0; i < ui_elements.Count; i++)
        {
            GameObject ui = ui_elements[i];
            if (dont_hide != null && dont_hide.Contains(ui)) { continue; }

            // on regarde si l'element a un transitioner
            Transitioner transitioner = ui.GetComponent<Transitioner>();
            if (transitioner == null)
            {
                if (log_elements_showing) { Debug.Log($"(UI_Pool) hiding ui element {ui.name} instantly"); }
                ui.SetActive(false);
                continue;
            }

            // on lance la transition
            uis_with_transitioner.Add(ui);
            transitioner.Hide(duration);
            if (log_elements_showing) { Debug.Log($"(UI_Pool) hiding ui element {ui.name} with transitioner (duration : {duration})"); }
        }

        // on attend la fin de la transition
        yield return new WaitForSecondsRealtime(duration);

        // on désactive finalement tous les elements avec un transitioner
        for (int i = 0; i < uis_with_transitioner.Count; i++) { uis_with_transitioner[i].SetActive(false); }
        if (log) { Debug.Log("(UI_Pool) hide_coroutine succeeded ! pool : " + Reference); }
    }

    // LOW ENABLING
    protected virtual IEnumerator enable_coroutine() { yield break; } // only useful for overriding properly for pools that don't need to override the showing
    protected virtual IEnumerator disable_coroutine() { yield break; }

    // PREPARING POOl
    public void PreparePool()
    {
        if (log) {  Debug.Log($"(UI_Pool) Preparing pool : {Reference}"); }

        // on cache tous les éléments du pool via canvas group
        for (int i = 0; i < ui_elements.Count; i++)
        {
            // on prepare seulement les elements qui ont un canvas group
            if (!ui_elements[i].TryGetComponent(out CanvasGroup cg)) { continue; }

            // on cache l'element abruptement
            cg.alpha = 0f;

            // on active l'element (va appeler Awake + Start dans les prochaines frames)
            // (et donc va préparer l'élement pour que son prochain show soit bcp plus rapide (on espere))
            ui_elements[i].SetActive(true);
        }
        prepared = true;
    }

    // REGISTER ELEMENTS
    public void RegisterToPool(GameObject ui_element, bool is_stacked = false)
    {
        // we check if the element is already in the pool
        if (ui_elements.Contains(ui_element))
        {
            if (log) { Debug.LogWarning("(UI_Pool) " + ui_element.name + " tried to register to a pool it's already in : " + Reference); }
            return;
        }

        if (log) { Debug.Log("(UI_Pool) " + ui_element.name + " just registered to pool : " + Reference); }

        // on ajoute l'élément au pool
        ui_elements.Add(ui_element);
        if (is_stacked && !stacked_elements.Contains(ui_element)) { stacked_elements.Add(ui_element); }

        // we show/hide the element if the pool is showed
        UI_Manager manager = UI_Manager.Instance;
        if (manager.CurrentPool == Reference) { ui_element.SetActive(true); }
        else if (manager.IsStacked(Reference) && is_stacked) { ui_element.SetActive(true); }
        else { ui_element.SetActive(false); }
    }
    public void QuitPool(GameObject ui_element)
    {
        // we check if the element is already in the pool
        if (!ui_elements.Contains(ui_element))
        {
            if (log) { Debug.LogWarning("(UI_Pool) " + ui_element.name + " tried to quit a pool it's not in : " + Reference); }
            return;
        }

        if (log) { Debug.Log("(UI_Pool) " + ui_element.name + " just quit pool : " + Reference); }
        // bool is_stacked = stacked_elements.Contains(ui_element);

        // we hide the element
        ui_element.SetActive(false);

        // on enlève l'élément du pool
        ui_elements.Remove(ui_element);
        if (stacked_elements.Contains(ui_element)) { stacked_elements.Remove(ui_element); }
    }
}

[Serializable]
public class PoolTransitionSettings
{
    public float Duration = 0.05f; // default duration of the transition
    public bool CanBeHidden = true; // if true, the pool can be hidden when switching to another pool
    public bool CanBeCanceled = false; // if true, the UI_Manager will save the last pool and go back to it with B
    public bool UsePersoInputs = true; // if true, the UI_Manager will activate the inputs.perso when the pool is showed
    public float TimeScale = 1f; // time scale when the pool is showed
    public float BackgroundAlpha = 0f; // alpha of the background when the pool is showed
    public float ChromaticAberration = 0f; // chromatic aberration effect intensity when the pool is showed
    public float Bloom = 0f; // bloom effect intensity when the pool is showed

    public bool StopTime => TimeScale == 0f;
    public bool HasBackground => BackgroundAlpha > 0f;

    public PoolTransitionSettings(float duration = 0.1f, bool can_be_hidden = true, bool can_be_canceled = false,
            bool use_perso_inputs = true, float time_scale = 1f, float background_alpha = 0f,
            float chromatic_aberration = 0f, float bloom = 0f)
    {
        Duration = duration;
        CanBeHidden = can_be_hidden;
        CanBeCanceled = can_be_canceled;
        UsePersoInputs = use_perso_inputs;
        TimeScale = time_scale;
        BackgroundAlpha = background_alpha;
        ChromaticAberration = chromatic_aberration;
        Bloom = bloom;
    }
    public static PoolTransitionSettings InGameDefault => new PoolTransitionSettings();
    public static PoolTransitionSettings InMenuDefault => new PoolTransitionSettings(can_be_canceled: true, use_perso_inputs: false, time_scale: 0f, background_alpha: 0.96f);
}
