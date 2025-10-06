using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Network module
/// </summary>
public class Module_Network : Module
{
    public float USB_Range = 1f;
    public float IR_Range = 0f;
    public float BT_Range = 0f;
    public float WIFI_Range = 0f;

    protected override void apply_upgrade()
    {
        // we update the usb range
        USB_Range = get_upgrade_effect("usb cable");
        // IR_Range = get_upgrade_effect("infrared antenna");
        // BT_Range = get_upgrade_effect("bluetooth antenna");
        // WIFI_Range = get_upgrade_effect("wifi antenna");
    }
}