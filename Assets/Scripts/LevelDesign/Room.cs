using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Room : MonoBehaviour
{
    
    [Header("Lights")]
    // public Transform lightsParent;
    public bool Alight = true;
    public List<Light2D> lights = new List<Light2D>();
    public int FindPriority = 0;
    // the priority which who the Level.cs will find the lights
    // the more close to infinity you are, the more you will be found first
    // if their is several Collider2D overlapping for the perso Room

    [Header("Room")]
    public Collider2D RoomCollider;

    [Header("Debug")]
    public bool debug = false;
    public bool loaded = false;

    public void Awake()
    {
        if (loaded) { return; }
        
        RoomCollider = GetComponent<Collider2D>();

        findLights();

        loaded = true;

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
        Alight = false;
    }
    public void Show()
    {
        if (debug) { Debug.Log("(Room) Showing room " + gameObject.name); }
        foreach (Light2D light in lights)
        {
            light.enabled = true;
        }
        Alight = true;
    }

}