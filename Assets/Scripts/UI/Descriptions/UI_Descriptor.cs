using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// this class handles multiple description FOR Descriptables
/// to show their name + their data
/// </summary>
public class UI_Descriptor : MonoBehaviour
{
    [Header("Writers")]
    [SerializeField] protected UI_Writer name_desc;
    [SerializeField] protected UI_Writer data_desc;

    [Header("Empty writing")]
    [SerializeField] protected string empty_name = "empty";
    [SerializeField] protected string empty_data = "";

    [Header("Logs")]
    public bool log = false;

    // private Action<UI_Slot> slot_hover_callback;

    // ON ENABLE / DISABLE
    protected void OnEnable() { register_callbacks(); }
    protected void OnDisable()
    {
        unregister_callbacks();
        name_desc.Clear();
        data_desc.Clear();
    }


    // CALLBACKS REGISTER
    protected virtual void register_callbacks()
    {
        UI_Navigator.Instance.OnSlotHoverEnter += handle_ui_slot_hover;
    }
    protected virtual void unregister_callbacks()
    {
        UI_Navigator.Instance.OnSlotHoverEnter -= handle_ui_slot_hover;
    }


    // CALLBACKS HANDLERS    
    protected virtual void handle_ui_slot_hover(UI_Slot slot)
    {
        if (slot is not Descriptable descriptable) { Describe(null); return; }
        Describe(descriptable);
    }

    // DESCRIPTION
    public virtual void Describe(Descriptable descriptable)
    {
        // we set the name & data description
        name_desc.Write(descriptable == null ? empty_name : descriptable.Name);
        data_desc.Write(descriptable == null ? empty_data : descriptable.Description);
    }

}


public interface Descriptable
{
    public string Name { get; }
    public string Description { get; }
}