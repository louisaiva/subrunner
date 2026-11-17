using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;

public class UI_InputText : UI_Slot
{
    [Header("Input")]
    [SerializeField] private TMP_InputField input_field;
    [SerializeField] private string inputText;
    [SerializeField] private TextMeshProUGUI title_text;
    [SerializeField] private TextMeshProUGUI placeholder_text;


    // CALLBACKS
    private List<System.Action<string>> onValidateCallbacks;
    public void RegisterInput(string title, string placeholder, List<System.Action<string>> on_validate)
    {
        // set the title and placeholder
        title_text.text = title;
        placeholder_text.text = placeholder;

        // register the callback
        Debug.Log($"(UI_InputText) Setting {on_validate.Count} callbacks.");
        onValidateCallbacks = on_validate;
    }
    private void OnDisable()
    {
        Debug.Log("(UI_InputText) disabling the input text. Resetting text & callbakcs");
        input_field.SetTextWithoutNotify("");
        inputText = "";
        onValidateCallbacks = new List<System.Action<string>>();
    }


    // INPUT SELECTION
    public void SelectInput()
    {
        // input_field.OnSelect(null);
        InputManager.Instance.EnableInputModule();
    }
    public void DeselectInput()
    {
        // input_field.OnDeselect(null);
        InputManager.Instance.DisableInputModule();
    }
    public void Write(string letters)
    {
        input_field.SetTextWithoutNotify(input_field.text + letters);
    }
    public void EraseLastLetter()
    {
        string new_text = input_field.text;
        if (new_text.Length > 0)
        {
            new_text = new_text.Remove(new_text.Length - 1);
        }
        input_field.SetTextWithoutNotify(new_text);
    }

    // VALIDATE INPUT
    public void ValidateCurrentText()
    {
        inputText = input_field.text;
        Validate();
    }
    public void ValidateInput(string input)
    {
        inputText = input;
        Validate();
    }
    public void Validate()
    {
        Debug.Log("Validating with " + onValidateCallbacks.Count + " callbacks");

        // fire the callback
        foreach (var callback in onValidateCallbacks)
        {
            Debug.Log("Calling " + callback);
            callback(inputText);
        }

        // remove the callback
        // remove_callback();
    }
}