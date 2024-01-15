using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class UI_XboxManager : MonoBehaviour
{
    
    // this class handles how the UI reacts to the xbox controller


    [Header("Slots")]
    [SerializeField] private I_UI_Slottable slottable;
    [SerializeField] private List<GameObject> slots;
    [SerializeField] private int current_slot_index;

    [Header("Navigation")]
    [SerializeField] private float angle_threshold = 45f;
    [SerializeField] private float angle_multiplicator = 100f;

    [Header("Inputs")]
    [SerializeField] private PlayerInputActions inputs;
    [SerializeField] private bool is_enabled = false;
    [SerializeField] private bool can_navigate = true;

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

        // on récupère les slots & tresholds
        this.slottable = slottable;
        slots = slottable.GetSlots(ref angle_threshold, ref angle_multiplicator);

        // on active le premier slot
        current_slot_index = -1;
        // slots[current_slot_index].GetComponent<UI_Item>().OnPointerEnter(null);

        is_enabled = true;
    }

    public void disable()
    {
        if (slottable == null) {return;}

        // on récupère les inputs
        inputs.UI.Disable();
        inputs.UI.navigate.performed -= ctx => navigate(ctx.ReadValue<Vector2>());
        inputs.UI.activate.performed -= ctx => activate();

        // on désactive le slot
        if (current_slot_index != -1)
        {
            slots[current_slot_index].GetComponent<UI_Item>().OnPointerExit(null);
        }

        // on reset les variables
        slottable = null;
        slots = null;
        current_slot_index = -1;

        is_enabled = false;
    }


    // main functions
    public void navigate(Vector2 direction)
    {
        // on vérifie que l'on peut naviguer
        if (direction == Vector2.zero || slottable == null)
        {
            can_navigate = true;
            return;
        }
        else if (can_navigate)
        {
            can_navigate = false;
        }
        else
        {
            return;
        }

        // on récupère les slots
        slots = slottable.GetSlots(ref angle_threshold, ref angle_multiplicator);

        // on récupère la position du slot actuel
        Vector2 current_slot_position = Vector2.zero;
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

        string s = "SLOTS: \n\n";

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

            s += slot.GetComponent<UI_Item>().item.item_name + " : " + slot_position + " / angle : " + angle + " /  distance : " + distance + "\n";

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
            slots[current_slot_index].GetComponent<UI_Item>().OnPointerExit(null);
        }
        slots[next_index].GetComponent<UI_Item>().OnPointerEnter(null);
        

        
        // on met à jour l'index
        current_slot_index = next_index;
    }

    public void activate()
    {
        if (slottable == null || current_slot_index == -1) {return;}

        // on récupère le slot
        GameObject slot = slots[current_slot_index];

        // on clique sur le slot
        slottable.clickOnItem(slot.GetComponent<UI_Item>().item);

        // on update les slots
        slots = slottable.GetSlots(ref angle_threshold, ref angle_multiplicator);
        current_slot_index = -1;
    }

    /* // GIZMOS
    private void OnDrawGizmos()
    {
        if (slots == null) {return;}

        // on dessine les slots
        foreach (GameObject slot in slots)
        {
            Vector3 world_pos = slot.GetComponent<RectTransform>().TransformPoint(slot.GetComponent<RectTransform>().rect.center);
            Vector3 slot_position = Camera.main.WorldToScreenPoint(world_pos);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(slot_position, 10);

            if (slots.IndexOf(slot) == current_slot_index)
            {
                // on dessine la direction avec une ligne
                Vector2 direction = inputs.UI.navigate.ReadValue<Vector2>();
                Gizmos.color = Color.red;
                Gizmos.DrawLine(slot_position, slot_position + (Vector3) direction * 100);
            }
        }

    } */


}