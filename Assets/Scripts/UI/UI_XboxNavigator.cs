using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class UI_XboxNavigator : MonoBehaviour
{
    
    // this class handles how the UI reacts to the xbox controller

    [Header("Slots")]
    [SerializeField] private I_UI_Slottable slottable;
    [SerializeField] private List<GameObject> slots;
    [SerializeField] private int current_slot_index;

    [Header("Navigation")]
    [SerializeField] private Vector2 base_position = Vector2.zero;
    [SerializeField] private float angle_threshold = 45f;
    [SerializeField] private float angle_multiplicator = 100f;

    [Header("Fast Navigation")]
    [SerializeField] private float fast_navigation_time_first_threshold = 0.5f; // temps avant activation du fast navigation
    [SerializeField] private float fast_navigation_time_cooldown = 0.2f; // temps entre chaque activation du fast navigation
    [SerializeField] private float fast_navigation_time = -1f; // temps depuis le dernier mouvement
    [SerializeField] private bool fast_navigation = false; // est-ce que l'on est en fast navigation


    [Header("Inputs")]
    [SerializeField] private PlayerInputActions inputs;
    [SerializeField] private bool first_navigation = true;

    // unity functions
    protected void Start()
    {
        // on récupère les inputs
        inputs = GameObject.Find("/perso").GetComponent<Perso>().playerInputs;
    }

 
    // enable/disable
    public void enable(I_UI_Slottable slottable)
    {
        // on récupère les inputs
        inputs.UI.Enable();
        inputs.UI.navigate.performed += ctx => navigate(ctx.ReadValue<Vector2>());
        inputs.UI.activate.performed += ctx => activate();

        // on désactive les perso inputs
        inputs.enhanced_perso.Disable();

        // on récupère les slots & tresholds
        this.slottable = slottable;
        slots = slottable.GetSlots(ref base_position, ref angle_threshold, ref angle_multiplicator);

        // on active le premier slot
        current_slot_index = -1;
    }

    public void disable()
    {
        if (slottable == null) {return;}

        // on récupère les inputs
        inputs.UI.Disable();
        inputs.UI.navigate.performed -= ctx => navigate(ctx.ReadValue<Vector2>());
        inputs.UI.activate.performed -= ctx => activate();

        // on active les perso inputs
        inputs.enhanced_perso.Enable();

        // on désactive le slot
        if (current_slot_index != -1)
        {
            slots[current_slot_index].GetComponent<I_UI_Slot>().OnPointerExit(null);
        }

        // on reset les variables
        slottable = null;
        slots = null;
        current_slot_index = -1;
    }


    // main functions
    public void navigate(Vector2 direction)
    {
        // on vérifie que l'on peut naviguer
        if (direction == Vector2.zero || slottable == null)
        {
            // on reset la navigation
            first_navigation = true;
            fast_navigation = false;
            fast_navigation_time = -1f;
            return;
        }
        else if (!first_navigation)
        {
            if (fast_navigation)
            {
                // on vérifie que l'on peut naviguer
                if (Time.time - fast_navigation_time < fast_navigation_time_cooldown) {return;}
                fast_navigation_time = Time.time;
            }
            else
            {
                // on vérifie que l'on peut naviguer
                if (Time.time - fast_navigation_time < fast_navigation_time_first_threshold) {return;}
                fast_navigation_time = Time.time;
                fast_navigation = true;
            }
        }
        else if (first_navigation)
        {
            first_navigation = false;
            fast_navigation_time = Time.time;
        }

        // on récupère les slots
        slots = slottable.GetSlots(ref base_position, ref angle_threshold, ref angle_multiplicator);

        // on récupère la position du slot actuel
        Vector2 current_slot_position = base_position;
        if (current_slot_index != -1)
        {
            GameObject slot = slots[current_slot_index];
            current_slot_position = slot.GetComponent<RectTransform>().TransformPoint(slot.GetComponent<RectTransform>().rect.center);
        }

        // on récupère les slots dans le bon angle
        List<GameObject> slots_in_angle = new List<GameObject>();
        // float angle_threshold = 45;
        foreach (GameObject slot in slots)
        {
            // on vérifie que ce n'est pas le slot actuel
            if (slots.IndexOf(slot) == current_slot_index) {continue;}

            // on récupère la position du slot
            Vector2 slot_position = slot.GetComponent<RectTransform>().TransformPoint(slot.GetComponent<RectTransform>().rect.center);
            Vector2 direction_to_slot = (slot_position - current_slot_position).normalized;
            float angle = Vector2.Angle(direction, direction_to_slot);
            if (angle < angle_threshold)
            {
                slots_in_angle.Add(slot);
            }
        }
        if (slots_in_angle.Count == 0) {return;}

        // string s = "SLOTS: \n\n";

        // on récupère le slot le plus proche
        int next_index = -1;
        float closest_distance = float.MaxValue;
        // float angle_multiplicator = 100f;
        foreach (GameObject slot in slots_in_angle)
        {
            // on récupère la position du slot
            Vector2 slot_position = slot.GetComponent<RectTransform>().TransformPoint(slot.GetComponent<RectTransform>().rect.center);
            float angle = Vector2.Angle(direction, (slot_position - current_slot_position).normalized);
            float distance = Vector2.Distance(current_slot_position, slot_position - direction * angle_multiplicator);

            // s += slot.GetComponent<I_UI_Slot>().item.item_name + " : " + slot_position + " / angle : " + angle + " /  distance : " + distance + "\n";

            if (distance < closest_distance)
            {
                closest_distance = distance;
                next_index = slots.IndexOf(slot);
            }
        }
        if (next_index == -1) {return;}

        // print(s);


        // on met à jour l'affichage
        if (current_slot_index != -1)
        {
            slots[current_slot_index].GetComponent<I_UI_Slot>().OnPointerExit(null);
        }
        slots[next_index].GetComponent<I_UI_Slot>().OnPointerEnter(null);
        

        
        // on met à jour l'index
        current_slot_index = next_index;
    }

    public void activate()
    {
        if (slottable == null || current_slot_index == -1) {return;}

        // on récupère le slot
        GameObject slot = slots[current_slot_index];

        // on clique sur le slot
        slot.GetComponent<I_UI_Slot>().OnPointerClick(null);

        // on update les slots
        current_slot_index = -1;
    }
}