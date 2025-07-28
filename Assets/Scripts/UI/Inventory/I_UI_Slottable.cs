using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;

public interface I_UI_Slottable
{

    List<GameObject> GetSlots(ref Vector2 base_position, ref float angle_threshold, ref float angle_multiplicator);
    bool IsYourSlot(GameObject slot);
    Vector2 SavedPosition { get; }

    // MonoBehaviour functions
    GameObject gameObject { get; }
}