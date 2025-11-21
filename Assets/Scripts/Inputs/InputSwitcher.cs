using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// enables / disables gameobjects based on input type
/// </summary>
public class InputSwitcher : MonoBehaviour
{

    [Header("Keyboard & mouse GameObjects")]
    [SerializeField] private List<GameObject> kbs;

    [Header("Gamepad GameObjects")]
    [SerializeField] private List<GameObject> gmpds;


    // SUBSCRIBE / UNSUBSCRIBE TO INPUT TYPE CHANGES
    void OnEnable()
    {
        InputManager.Instance.OnInputTypeChanged += switch_feedback;

        // Initial switch based on current input type
        switch_feedback(InputManager.Instance.CurrentInputType);
    }
    void OnDisable()
    {
        // Unsubscribe from input type change event
        InputManager.Instance.OnInputTypeChanged -= switch_feedback;
    }

    // METHOD TO SWITCH FEEDBACK BASED ON INPUT TYPE
    private void switch_feedback(string input_type)
    {
        for (int i = 0; i < kbs.Count; i++)
        {
            kbs[i].SetActive(input_type == "keyboard");
        }
        for (int i = 0; i < gmpds.Count; i++)
        {
            gmpds[i].SetActive(input_type == "gamepad");
        }
    }
}