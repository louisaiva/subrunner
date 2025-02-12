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
    public Transform hidden_ceiling;
    public Transform room_hider;
    public Transform walls_hider;
    public Transform ground_hider;

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
        hidden_ceiling = transform.Find("hidden_ceiling");
        room_hider = transform.Find("room_hider");
        walls_hider = transform.Find("walls_hider");
        ground_hider = transform.Find("ground_hider");

        Hide();
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

        if (hidden_ceiling != null)
        {
            hidden_ceiling.gameObject.SetActive(true);
        }
        else if (room_hider != null)
        {
            room_hider.gameObject.SetActive(true);
            walls_hider?.gameObject.SetActive(true);
            ground_hider?.gameObject.SetActive(true);
        }
        else
        {
            foreach (Light2D light in lights)
            {
                light.enabled = false;
            }
        }

        Alight = false;
    }
    public void Show()
    {
        if (debug) { Debug.Log("(Room) Showing room " + gameObject.name); }

        if (hidden_ceiling != null)
        {
            hidden_ceiling.gameObject.SetActive(false);
        }
        else if (room_hider != null)
        {
            room_hider.gameObject.SetActive(false);
            walls_hider?.gameObject.SetActive(false);
            ground_hider?.gameObject.SetActive(false);
        }
        else
        {
            foreach (Light2D light in lights)
            {
                light.enabled = true;
            }
        }
        Alight = true;
    }
}