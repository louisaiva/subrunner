using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
using UnityEngine.EventSystems;

public interface I_UI_Slottable
{

    List<GameObject> GetSlots(ref Vector2 base_position, ref float angle_threshold, ref float angle_multiplicator);
    bool IsYourSlot(GameObject slot);
    Vector2 SavedPosition { get; }

    // MonoBehaviour functions
    GameObject gameObject { get; }
}

public interface Slottable
{
    public List<UI_Slot> GetSlots();
    public bool IsYourSlot(UI_Slot slot);
    public void Enable(bool ingame);
    public void Disable();
    public UI_Slot StartingSlot { get; }

    // MonoBehaviour functions
    public GameObject gameObject { get; }
    public string name { get; }
}

public interface Droppable
{
    void OnPointerDropped(PointerEventData eventData);
}

public interface ItemReceivable
{
    GameObject gameObject { get; }
    void OnPointerDragEnter(UI_ItemStack moving_ui_item);
}
public interface ItemMovable
{
    void OnPointerDragDown();
}