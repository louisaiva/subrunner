using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oven : Capable, Interactable, Onnable
{
    // INTERACTABLE
    public InteractCapacity Interactor { get; set; }
    public bool IsMoving { get; set; } = false;
    public bool IsOn { get; set; } = false;

    [Header("Oven Settings")]
    public float heat_percentage = 0.0f;
    public float total_heat_time = 10.0f;
    public float max_heat_temperature = 100.0f; // corresponds to 1f heat percentage

    [Header("Cooking")]
    [SerializeField] private Vector2 pot_local_position = new Vector2(0.0f, 0.0f);

    private Coroutine current_heating_coroutine = null;

    // INTERACTION
    public void OnInteract(Capable interactor)
    {
        // we set the interactor
        Interactor = interactor.GetCapacity<InteractCapacity>();

        // we check if a being interact with the oven
        if (interactor is Being)
        {
            // if we have a burned pot and we are not on, we drop the pot
            Pot burned_pot = Inventory.GetItemsByType<Pot>().Find(p => p.Reference == "pot:burned");
            if (burned_pot != null && !IsOn)
            {
                interactor.Inventory.Grab(burned_pot);
                return;
            }

            // we toggle the oven
            if (current_heating_coroutine != null) { StopCoroutine(current_heating_coroutine); }
            if (IsOn) { current_heating_coroutine = StartCoroutine(heat_down()); }
            else { current_heating_coroutine = StartCoroutine(heat_up()); }
            return;
        }

        // we check if it is a pot
        if (interactor is Pot pot) { interact_with_pot(pot); return; }
    }
    private void interact_with_pot(Pot pot)
    {
        // we heat the pot
        Debug.Log("(Oven) Heating pot " + pot.name);

        // we grab the pot
        Inventory.Grab(pot);

        // we place the pot on the hob
        place_pot(pot);
    }
    private void place_pot(Pot pot)
    {
        // we place the pot
        pot.Place();

        // then we move the pot to our capable + position it
        pot.transform.localPosition = pot_local_position;
        /* Transform hover = pot.GetCapacity<HoverCapacity>()?.transform;
        if (hover != null)
        {
            // we put the pot hover y at 0 so we can interact with it (otherwise it will never be reachable because
            // the oven hover is always in front of it, and we can't get closer bcz of the collider)
            hover.localPosition = new Vector3(hover.localPosition.x, -pot_local_position.y, hover.localPosition.z);
        } */
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
}