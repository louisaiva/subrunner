using UnityEngine;

public class UI_KeyboardCaller : MonoBehaviour
{
    public string letter = "";
    public void WriteLetter()
    {
        GetComponentInParent<UI_Keyboard>().InputText.Write(letter);
    }
    public void ValidateText()
    {
        Debug.Log("(UI_KeyboardCaller) Validate Actual Text");
        GetComponentInParent<UI_Keyboard>().InputText.ValidateCurrentText();
    }
    public void RemoveLastLetter()
    {
        Debug.Log("(UI_KeyboardCaller) Erase last letter");
        GetComponentInParent<UI_Keyboard>().InputText.EraseLastLetter();
    }
}