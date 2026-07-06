using UnityEngine;

/// <summary>
/// this particular ui_descriptor also listens to on slot hover exit,
/// so we only show the description on the hover. if no slot is hovered
/// and if the current pool is a Descriptable, then we show the description of the pool
/// </summary>
public class UI_HoverDescriptor : UI_Descriptor
{


    // CALLBACKS REGISTER
    protected override void register_callbacks()
    {
        UI_Navigator.Instance.OnSlotHoverEnter += handle_ui_slot_hover;
        UI_Navigator.Instance.OnSlotHoverExit += handle_ui_slot_hover_exit;
    }
    protected override void unregister_callbacks()
    {
        UI_Navigator.Instance.OnSlotHoverEnter -= handle_ui_slot_hover;
        UI_Navigator.Instance.OnSlotHoverExit -= handle_ui_slot_hover_exit;
    }


    // CALLBACKS HANDLERS
    protected void handle_ui_slot_hover_exit(UI_Slot slot) { Describe(null); }

    // DESCRIPTION
    public override void Describe(Descriptable descriptable)
    {
        if (descriptable == null && UI_Manager.Instance.GetCurrentPool() is Descriptable descriptable_pool)
        { 
            descriptable = descriptable_pool;
        }
        base.Describe(descriptable);
    }
}