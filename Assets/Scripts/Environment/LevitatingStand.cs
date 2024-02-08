using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevitatingStand : Stand
{

    [Header("Levitating Stand")]
    [SerializeField] private float levitation_speed = 0.1f;
    [SerializeField] private float levitation_amplitude = 0.1f;

    private void Update()
    {
        foreach (Transform child in transform)
        {
            // on vérifie que ce n'est pas softlight
            if (new string[] {"soft_light"}.Contains(child.name)) { continue; }

            // on déplace le gameobject de +offset pour le placer sur le stand
            child.localPosition = new Vector3(slot_offset.x, slot_offset.y + levitation_amplitude * Mathf.Sin(Time.time * levitation_speed), 0);
        }
    }

}