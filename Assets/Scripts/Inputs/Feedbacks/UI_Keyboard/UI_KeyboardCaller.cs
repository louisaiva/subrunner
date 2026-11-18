using UnityEngine;

public class UI_KeyboardCaller : MonoBehaviour
{
    private static bool log = false;

    public string letter = "";
    public void WriteLetter()
    {
        GetComponentInParent<UI_Keyboard>().Write(letter);
    }
    public void ValidateText()
    {
        if (log) { Debug.Log("(UI_KeyboardCaller) Validate Actual Text"); }
        GetComponentInParent<UI_Keyboard>().InputText.ValidateCurrentText();
    }
    public void RemoveLastLetter()
    {
        if (log) { Debug.Log("(UI_KeyboardCaller) Erase last letter"); }
        GetComponentInParent<UI_Keyboard>().InputText.EraseLastLetter();
    }
    public void ActivateShiftOnce()
    {
        if (log) { Debug.Log("(UI_KeyboardCaller) Activate shift once"); }
        GetComponentInParent<UI_Keyboard>().ActivateSHIFT(once:true);
    }
    public void ActivateShift()
    {
        if (log) { Debug.Log("(UI_KeyboardCaller) Activate shift"); }
        GetComponentInParent<UI_Keyboard>().ActivateSHIFT(once: false);
    }
    public void DisableShift()
    {
        if (log) { Debug.Log("(UI_KeyboardCaller) Deactivate shift"); }
        GetComponentInParent<UI_Keyboard>().DisableSHIFT();
    }
}