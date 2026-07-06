using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Changes an action (and so the IF components) as we want it
/// </summary>
public class ActionSwitcher : MonoBehaviour
{
    [Header("Current Action")]
    // [SerializeField] private InputActionReference action_ref;
    // [SerializeField] private string action_ref;
    [SerializeField] private string kb_ref;
    [SerializeField] private string gm_ref;

    [Header("IFs")]
    [SerializeField] private GameObject kb;
    [SerializeField] private GameObject gmpd;

    private InputSwitcher _switcher;
    private InputSwitcher switcher
    {
        get
        {
            if (_switcher != null) { return _switcher; }
            _switcher = GetComponent<InputSwitcher>();
            return _switcher;
        }
    }

    [Header("Logs")]
    [SerializeField] private bool log = false;


    public async void SwitchAction(string action_name, Color? color=null, bool log_callbacks=false)
    {
        // we get the binding names of the action
        InputManager.Instance.GetActionBindingForAction(action_name, ref kb_ref, ref gm_ref);

        if (log) { Debug.Log($"(ActionSwitcher) Got 2 bindings for action '{action_name}' :         keyboard : '{kb_ref}'         ///            gamepad : '{gm_ref}'"); }

        if (switcher != null) { switcher.ClearIFs(); }
        Transform gmpd_parent = gmpd.transform.parent;
        Transform kb_parent = kb.transform.parent;
        Destroy(gmpd);
        Destroy(kb);

        await System.Threading.Tasks.Task.Yield();

        // kb = 
        gmpd = InputManager.Instance.IF_Bank.Instantiate(gm_ref, gmpd_parent, gamepad: true).gameObject;
        kb = InputManager.Instance.IF_Bank.Instantiate(kb_ref, kb_parent, gamepad: false).gameObject;

        if (log)
        {

            if (gmpd == null) { Debug.LogError($"(ActionSwitcher) New gamepad IF for '{action_name}' and binding '{gm_ref}' could not be instantiated by IF_Bank !"); }
            if (kb == null) { Debug.LogError($"(ActionSwitcher) New keyboard IF for '{action_name}' and binding '{kb_ref}' could not be instantiated by IF_Bank !"); }
        }

        if (log_callbacks)
        {
            // enable logs callbacks on the IFs
            if (gmpd != null && gmpd.TryGetComponent(out InputFeedback gmpdIF)) { gmpdIF.log_callbacks = true; }
            if (kb != null && kb.TryGetComponent(out InputFeedback kbIF)) { kbIF.log_callbacks = true; }
        }

        if (switcher != null)
        {
            switcher.ClearIFs();
            switcher.AddIF(gmpd, gamepad:true);
            switcher.AddIF(kb, gamepad:false);
        }

        if (color == null) { return; }
        set_color(color.Value);
    }

    private void set_color(Color color)
    {
        UI_EventButton[] IFs = GetComponentsInChildren<UI_EventButton>(includeInactive: true);
        foreach (UI_EventButton IF in IFs)
        {
            if (IF == null) { continue; }
            IF.SetColor(color);
        }
        ManualImageFeedback[] MIFs = GetComponentsInChildren<ManualImageFeedback>(includeInactive: true);
        foreach (ManualImageFeedback mif in MIFs)
        {
            if (mif == null) { continue; }
            mif.SetColor(color);
        }
    }
}