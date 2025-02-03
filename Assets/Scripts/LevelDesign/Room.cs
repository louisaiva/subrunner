using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Room : MonoBehaviour
{
    
    [Header("Parents Transforms")]
    // public Transform lightsParent;
    public List<Light2D> lights = new List<Light2D>();

    [Header("Room")]
    public Collider2D roomCollider;

    [Header("Debug")]
    public bool debug = false;

    void Awake()
    {
        roomCollider = GetComponent<Collider2D>();

        findLights();

        // lightsParent = transform.Find("lights");
    }

    public void findLights()
    {
        foreach (Transform child in transform)
        {
            // todo : denestify this
            if (child.name == "lights" || child.name == "objects")
            {
                foreach (Transform obj in child)
                {
                    Light2D light2 = obj.GetComponent<Light2D>();
                    if (light2 != null)
                    {
                        lights.Add(light2);
                        continue;
                    }
                    foreach (Transform obj2 in obj)
                    {
                        Light2D light3 = obj2.GetComponent<Light2D>();
                        if (light3 != null)
                        {
                            lights.Add(light3);
                            break;
                        }
                    }
                }
                continue;
            }

            Light2D light = child.GetComponent<Light2D>();
            if (light != null)
            {
                lights.Add(light);
            }
        }
    }
    public void Hide()
    {

        if (debug) { Debug.Log("(Room) Hiding room " + gameObject.name); }
        foreach (Light2D light in lights)
        {
            light.enabled = false;
        }
    }
    public void Show()
    {
        if (debug) { Debug.Log("(Room) Showing room " + gameObject.name); }
        foreach (Light2D light in lights)
        {
            light.enabled = true;
        }
    }

}