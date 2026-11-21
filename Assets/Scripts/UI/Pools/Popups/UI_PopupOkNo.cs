using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_PopupOkNo : UI_SlottablePool, Descriptable
{

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI title_text;
    [SerializeField] private TextMeshProUGUI question_text;

    [Header("Buttons")]
    [SerializeField] private UI_EventButton ok_button;


    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        yield return base.enable_coroutine();

        // register to callbacks
        ok_button.OnClick += on_ok_clicked;
    }
    protected override IEnumerator disable_coroutine()
    {
        // unregister from callbacks
        ok_button.OnClick -= on_ok_clicked;

        yield return base.disable_coroutine();
    }



    // CALLBACKS
    private List<System.Action> onValidateCallbacks;
    public void RegisterDialog(string title, string question, List<System.Action> on_validate)
    {
        // set the title and placeholder
        title_text.text = title;
        question_text.text = question;

        // register the callback
        set_callbacks(on_validate);
    }
    private void set_callbacks(List<System.Action> callbacks)
    {
        onValidateCallbacks = callbacks;
    }


    // BUTTON CALLBACKS
    private void on_ok_clicked()
    {
        UI_Manager.Instance.UnstackPool("popup_okno");

        // trigger the callbacks
        if (onValidateCallbacks == null) { return; }
        if (onValidateCallbacks.Count == 0) { return; }
        
        foreach (var callback in onValidateCallbacks) { callback(); }
    }







    ///
    //
    /// CUSTOMIZATION
    //
    ///



    [Header("Colors")]
    [SerializeField] private UI_Colorant colorant;
    public void ResetColor() { ApplyColor(); }
    public void ApplyColor(Color? color = null) => colorant.ApplyColor(color);



    [Header("Description")]
    [SerializeField] private string desc_name = "";
    [SerializeField] private string desc = "";
    public string Name => string.IsNullOrEmpty(desc_name) ? null : desc_name;
    public string Description => string.IsNullOrEmpty(desc) ? null : desc;
    public void SetNameAndDescription(string n, string d) { desc_name = n; desc = d; }
}