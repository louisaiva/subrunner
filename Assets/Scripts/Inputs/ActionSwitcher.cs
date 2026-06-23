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
    [SerializeField] private string action_ref;
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


    public void SwitchAction(string action_name)
    {
        action_ref = action_name;
        
        // we get the binding names of the action
        InputManager.Instance.GetActionBindingForAction(action_name, ref kb_ref, ref gm_ref);

        Debug.Log($"(ActionSwitcher) Got 2 bindings for action '{action_name}' :         keyboard : '{kb_ref}'         ///            gamepad : '{gm_ref}'");

        // kb = 
        InputFeedback new_gm = InputManager.Instance.IF_Bank.Instantiate(gm_ref, gmpd.transform.parent, gamepad: true);
        InputFeedback new_kb = InputManager.Instance.IF_Bank.Instantiate(kb_ref, kb.transform.parent, gamepad: false);
        Destroy(gmpd);
        Destroy(kb);
        gmpd = new_gm.gameObject;
        kb = new_kb.gameObject;

        if (switcher != null)
        {
            switcher.ClearIFs();
            switcher.AddIF(gmpd, gamepad:true);
            switcher.AddIF(kb, gamepad:false);
        }
    }

    public void SetColor(Color color)
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