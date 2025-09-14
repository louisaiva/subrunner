using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// capacity that allow you to feel temperature
/// (and explodes if you overheat)
/// </summary>
public class TempCapacity : Capacity
{

    [Header("Temp")]
    public float Temp = 0f;
    public float MaxTemp = 70f;

    [Header("Blind parameters")]
    [SerializeField] private float blind_temp = 50f;
    [SerializeField] private float variation = 0.1f;
    [SerializeField] private float frequency = 0.5f;

    protected override void Update()
    {
        Temp = variation * Mathf.Sin(Time.unscaledTime * frequency) + blind_temp;
    }
}

