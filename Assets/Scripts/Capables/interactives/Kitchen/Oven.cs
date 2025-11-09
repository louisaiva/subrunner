using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Oven : Chest, Onnable
{

    // CHEST
    public override bool is_open { get => true; }
    public override bool is_moving { get => false; }


    // INTERACTABLE
    public override InteractType InteractionType { get => InteractType.Kitchen; }
    public bool IsMoving { get; set; } = false;
    public bool IsOn { get; set; } = false;

    [Header("Oven Settings")]
    public float heat_percentage = 0.0f;
    public float total_heat_time = 10.0f;
    public float max_heat_temperature = 100.0f; // corresponds to 1f heat percentage

    private Coroutine current_heating_coroutine = null;

    // START
    protected override void Start()
    {
        base.Start();

        // suscribe to Inventory Grab
        Inventory.OnItemGrabbed += (item) => Invoke(nameof(try_to_put_food_in_pot), 0.1f);

        // and to buttons / toggles
        UI_Toggle onoff_toggle = Inventory.MainUI.GetToggleByName("on_off_toggle");
        onoff_toggle.OnOn += PowerOn;
        onoff_toggle.OnOff += PowerOff;
        UI_Button exit_btn = Inventory.MainUI.GetButtonByName("exit_button");
        exit_btn.OnClick += ExitHover;
    }

    // INTERACTION
    public override void OnInteract(Capable interactor)
    {
        base.OnInteract(interactor);

        // we check if it is a pot
        if (interactor is Pot pot) { Inventory.Grab(pot); return; }
    }
    private void try_to_put_food_in_pot()
    {
        // we get the pot in the hob
        Pot pot = Inventory.GetItemsByType<Pot>().FirstOrDefault();
        if (pot == null) { return; }

        // we get the food in the food pool
        List<Food> food = Inventory.GetItemsByType<Food>();
        if (food.Count == 0) { return; }

        // we try to put them all in the pot
        for (int i = food.Count - 1; i >= 0; i--)
        {
            food[i].OnInteract(pot);
        }
    }


    // UPDATE
    protected override void Update()
    {
        base.Update();


        // todo

        // réfléchir à un systeme de heat plus complet avec plusieurs choses :
        // tous les capables ont une temperature
        // ces capables transmettent cette chaleur à leur parent & à leurs enfants (inventory-parlant)
        // si on a des enfants on peut donc dissiper cette chaleur et ensuite on crame pas
        // mais si on a pas d'enfant bah ça surchauffe et après on peut prendre feu
        // ce qui fait potentiellement prendre feu nos parents & enfants ?

        // plusieurs parametres
        // -> temperature
        // -> temperature de base
        // -> temperature de gel
        // -> temperature de fusion (prend feu)
        // -> facteur de transmission vers le haut (le parent, dissipe aussi la chaleur)
        // -> facteur de transmission vers le bas (les enfants, dissipe aussi)

        // quand on recoit de la chaleur on en transmet dans le bon sens (si ça vient du bas on transmet en haut, et inv.)
        // et ensuite on stack le reste de la chaleur, ce qui fait augmenter ou baisser notre temperature


        // we transmit the heat to things that are in the inventory
        List<Item> items = Inventory.Items;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] is not Pot pot) { continue; }
            pot.TransmitHeat(heat_percentage * max_heat_temperature * Time.deltaTime);
        }
    }

    // ONNIN / ONNOFF
    public void PowerOn()
    {
        if (current_heating_coroutine != null) { StopCoroutine(current_heating_coroutine); }
        current_heating_coroutine = StartCoroutine(heat_up());
    }
    public void PowerOff()
    {
        if (current_heating_coroutine != null) { StopCoroutine(current_heating_coroutine); }
        current_heating_coroutine = StartCoroutine(heat_down());
    }
    private IEnumerator heat_up()
    {
        IsOn = true;
        IsMoving = true;

        // we get the transition duration
        float heat_up_duration = total_heat_time * (1.0f - heat_percentage);

        // we play the heating up animation
        anim_player.Play("heating_up", duration_override: heat_up_duration);
        anim_player.AddToPile("heating");

        // we wait for the animation to end
        while (anim_player.IsPlaying("heating_up"))
        {
            heat_percentage += Time.deltaTime / total_heat_time;
            yield return null;
        }

        // we set the oven as on
        IsMoving = false;
        heat_percentage = 1.0f;
    }
    private IEnumerator heat_down()
    {
        IsOn = false;
        IsMoving = true;

        // we get the transition duration
        float heat_down_duration = total_heat_time * heat_percentage;

        // we play the heating down animation
        anim_player.Play("heating_down", duration_override: heat_down_duration);
        anim_player.StopPlaying("heating");

        // we wait for the animation to end
        while (anim_player.IsPlaying("heating_down"))
        {
            heat_percentage -= Time.deltaTime / total_heat_time;
            yield return null;
        }

        // we set the oven as off
        IsMoving = false;
        heat_percentage = 0.0f;
    }


    // INTERACT KEY FEEDBACK
    protected override Vector2 calculate_best_kf_position()
    {
        // we calculate the position we need to give the kf's canvas

        // 1 - we get the inventory's canvas
        Transform ui_canvas = Inventory.ui.transform.parent;
        Vector2 kf_position = ui_canvas.transform.localPosition;
        kf_position.y += 150.0f; // we move it a bit up
        return kf_position;
    }

}