using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_Keyboard : MonoBehaviour
{
    public UI_InputText InputText;
    [SerializeField] private Color keys_color;
    [SerializeField] private Transform keys_holder;

    private List<string> keyboard_refs = new List<string>()
    {
        "a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z"
    };
    private bool built = false;

    public void BuildKeyboardIfNotBuilt()
    {
        if (built) { return; }

        foreach (string letter in keyboard_refs)
        {
            instanciate_letter(letter);
        }
        built = true;
    }
    private void instanciate_letter(string letter)
    {
        InputFeedback fb = InputManager.Instance.IF_Bank.Instantiate(letter, keys_holder, gamepad: false);
        fb.name = "key_" + letter;
        fb.gameObject.layer = LayerMask.NameToLayer("UI_Slot");
        ((EventFeedback)fb).simulate_click = true;

        UI_EventButton btn = fb.GetComponent<UI_EventButton>();
        btn.SetColor(keys_color);
        UI_KeyboardCaller caller = fb.AddComponent<UI_KeyboardCaller>();
        caller.letter = letter;
        if (letter == "space") { caller.letter = " "; }

        if (letter == "back") { btn.OnClick = caller.RemoveLastLetter; }
        else if (letter == "enter") { btn.OnClick = caller.ValidateText; }
        else { btn.OnClick = caller.WriteLetter; }

        // add image to enable graphic raycaster
        Image img = fb.AddComponent<Image>();
        img.color = new Color(0f,0f,0f,0f);
    }
}