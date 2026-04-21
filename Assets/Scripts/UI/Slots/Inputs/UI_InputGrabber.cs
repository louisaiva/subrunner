using UnityEngine;
using TMPro;

public class UI_InputGrabber : MonoBehaviour
{
    [SerializeField] private string inputText;
    

    public void GrabFromInputField(string input)
    {
        inputText = input;
        display_reaction($"You entered: {inputText}");
    }

    private void display_reaction(string reaction)
    {
        Debug.Log($"(UI_InputGrabber) {reaction}");
    }
}