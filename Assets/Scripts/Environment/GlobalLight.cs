using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class GlobalLight : MonoBehaviour {
    
    // MODE
    public string[] mode = {"on","off","flickering","almost_stable"};
    public string mode_current = "on";

    // GLOBAL parameters
    public float base_intensity = 0.1f;
    private float intensity = 1f;
    private Light2D global_light;
    public float almost_stable_rate = 0.1f; // clignote 0.1 fois par seconde

    // unity functions
    void Start()
    {
        // on récupère le global_light
        global_light = gameObject.GetComponent<Light2D>();

        // on met à jour la couleur
        // global_light.color = new Color(0.5f, 0.5f, 0.5f, 1f);
    }

    void Update()
    {
        // on met à jour le mode
        update_mode();

        // on met à jour l'intensité
        global_light.intensity = intensity * base_intensity;
    }

    // functions
    private void update_mode(){
        // on met à jour le mode
        switch (mode_current)
        {
            case "on":
                on();
                break;
            case "off":
                off();
                break;
            case "flickering":
                flicker();
                break;
            case "almost_stable":
                almost_stable();
                break;
            default:
                on();
                break;
        }
    }

    private void flicker()
    {
        // clignote h24

        // on met à jour le mode
        intensity = 0.5f + Mathf.Sin(Time.time * 10f) * 0.5f;
    }

    private void almost_stable()
    {
        // clignote une fois de temps en temps
        
        if (Time.time % 10f < almost_stable_rate)
        {
            // on met à jour le mode
            intensity = 0.9f + Mathf.Sin(Time.time * 10f) * 0.1f;
        }
        else
        {
            // on met à jour le mode
            intensity = 1f;
        }        
    }

    private void on()
    {
        // on met à jour le mode
        intensity = 1f;
    }

    private void off()
    {
        // on met à jour le mode
        intensity = 0f;
    }

    // SETTERS

    public void setMode(string mode)
    {
        // on met à jour le mode
        mode_current = mode;
        update_mode();
    }

    public void roll(){

        // roll through modes
        int index = System.Array.IndexOf(mode, mode_current);
        index += 1;
        if (index >= mode.Length) { index = 0; }

        // on met à jour le mode
        mode_current = mode[index];
        update_mode();
    }
}