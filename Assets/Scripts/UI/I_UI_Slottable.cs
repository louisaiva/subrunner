using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public interface I_UI_Slottable
{

    // callback
    System.Action<InputAction.CallbackContext> CancelCallback { get; }
    // void define_callback();

    List<GameObject> GetSlots(ref Vector2 base_position, ref float angle_threshold, ref float angle_multiplicator);
}