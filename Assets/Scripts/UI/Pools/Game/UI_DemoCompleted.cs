using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class UI_DemoCompleted : UI_SlottablePool
{

    protected override void before_adding_to_stack()
    {
        World.Instance.Timer.StopTimer();
    }
    protected override void after_removed_from_stack()
    {
        World.Instance.Timer.StartTimer();
    }

}