using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_Keyboard : MonoBehaviour
{
    public UI_InputText InputText;
    [SerializeField] private Color keys_color;
    [SerializeField] private Transform keys_min_holder;
    [SerializeField] private Transform keys_holder;
    [SerializeField] private Transform digits_holder;
    [SerializeField] private Transform symbols_holder;
    [SerializeField] private UI_CycleButton shift_btn;

    private List<string> keyboard_refs = new List<string>() { "a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z" };
    private List<string> qwerty = new List<string>()        { "q","w","e","r","t","y","u","i","o","p","a","s","d","f","g","h","j","k","l","z","x","c","v","b","n","m" };
    private List<string> azerty = new List<string>()        { "a","z","e","r","t","y","u","i","o","p","q","s","d","f","g","h","j","k","l","m","w","x","c","v","b","n" };

    private List<string> digits = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
    private List<string> symbols = new List<string>() { "&", "*", "$", "'", "(", "-", ")", "_", "°", "+" };

    private bool built = false;
    public void BuildKeyboardIfNotBuilt()
    {
        if (built) { return; }

        // letters
        foreach (string letter in qwerty) { instanciate_key(letter + "_min", keys_min_holder, binding: letter, real_letter: letter); }
        foreach (string letter in qwerty) { instanciate_key(letter, keys_holder, binding: letter, real_letter: letter.ToUpperInvariant()); }

        // digits & symbols
        for (int i=0; i<10; i++)
        {
            instanciate_key(digits[i], digits_holder);
            instanciate_key(symbols[i], symbols_holder, binding: digits[i]);
        }

        built = true;

        DisableSHIFT();
    }
    private void instanciate_key(string letter, Transform parent, string binding = null, string real_letter = null)
    {
        InputFeedback fb = InputManager.Instance.IF_Bank.Instantiate(binding ?? letter, parent, letter);
        fb.name = "key_" + letter;
        fb.gameObject.layer = LayerMask.NameToLayer("UI_Slot");
        ((EventFeedback)fb).simulate_click = true;

        UI_EventButton btn = fb.GetComponent<UI_EventButton>();
        btn.SetColor(keys_color);
        UI_KeyboardCaller caller = fb.AddComponent<UI_KeyboardCaller>();
        caller.letter = real_letter ?? letter;

        if (letter == "back") { btn.OnClick = caller.RemoveLastLetter; }
        else if (letter == "enter") { btn.OnClick = caller.ValidateText; }
        else { btn.OnClick = caller.WriteLetter; }

        // add image to enable graphic raycaster
        Image img = fb.AddComponent<Image>();
        img.color = new Color(0f,0f,0f,0f);
    }

    // TOGGLE MAJ
    public bool shift = false;
    public bool shift_once = false;
    public void ActivateSHIFT(bool once = false)
    {
        shift = !once;
        shift_once = once;
        digits_holder.gameObject.SetActive(true);
        symbols_holder.gameObject.SetActive(false);
        keys_holder.gameObject.SetActive(true);
        keys_min_holder.gameObject.SetActive(false);
    }
    public void DisableSHIFT()
    {
        shift = false;
        shift_once = false;
        digits_holder.gameObject.SetActive(false);
        symbols_holder.gameObject.SetActive(true);
        keys_holder.gameObject.SetActive(false);
        keys_min_holder.gameObject.SetActive(true);
    }

    public void Write(string letter)
    {
        InputText.Write(letter);
        if (shift_once) { shift_btn.OnPointerClick(null); shift_btn.OnPointerClick(null); } // click twice so it goes back to disabled
    }
}