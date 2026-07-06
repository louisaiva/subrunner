using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UI_InputText : UI_Slot
{
    [Header("Input")]
    [SerializeField] private TMP_InputField input_field;
    [SerializeField] private string inputText;
    [SerializeField] private TextMeshProUGUI title_text;
    [SerializeField] private TextMeshProUGUI placeholder_text;


    // CALLBACKS
    public void RegisterInput(string title, string placeholder, List<System.Action<string>> on_validate)
    {
        // set the title and placeholder
        title_text.text = title;
        placeholder_text.text = placeholder;

        // register the callback
        set_callback(on_validate);
    }
    private List<System.Action<string>> onValidateCallbacks;
    private void set_callback(List<System.Action<string>> callbacks)
    {
        onValidateCallbacks = callbacks;
    }
    private void remove_callback()
    {
        onValidateCallbacks = new List<System.Action<string>>();
    }


    // INPUT SELECTION
    public void SelectInput()
    {
        input_field.OnSelect(null);
        InputManager.Instance.EnableInputModule();
    }
    public void DeselectInput()
    {
        input_field.OnDeselect(null);
        InputManager.Instance.DisableInputModule();
    }


    // TMP methods
    public void ValidateInput(string input)
    {
        inputText = input;

        // fire the callback
        foreach (var callback in onValidateCallbacks)
        {
            callback(inputText);
        }

        // remove the callback
        remove_callback();
    }
}