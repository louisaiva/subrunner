using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Controller : Singleton<Controller>
{
    [Header("Current capable")]
    public Capable Capable
    {
        get
        {
            if (_capable == null)
                _capable = transform.parent.GetComponent<Capable>();
            return _capable;
        }
    }
    [SerializeField] private Capable _capable;

    [Header("Capable stack")]
    public List<Capable> stack = new List<Capable>();

    [Header("Components")]
    public PersoInputsController PIC;
    public UI_InputsController UIC;
    public HackableNavigator HackableNavigator;
    public ExploitNavigator ExploitNavigator;
    [SerializeField] private SeeThroughHandler see_through;
    public Room current_room { get; set; }

    [Header("UI Statics elements")]
    [SerializeField] private UI_Inventory perso_quick_inventory;


    // START
    private void Start()
    {
        // on récupère les composants
        perso_quick_inventory = UI_Manager.Instance.GetPool("hud").transform.Find("perso_quick_inventory").GetComponent<UI_Inventory>();

        ResetCapableTarget();
    }


    // CHANGE CAPABLE TARGET HIGH LEVEL
    public void ChangeCapableTarget(Capable new_target, float duration = -888f,bool add_to_stack=true)
    {
        // on décontrole l'ancienne target
        uncontrol_capable(Capable);

        // on déplace le script sur le gameobject capable
        transform.parent = new_target.transform;
        transform.localPosition = Vector3.zero;

        // on ajoute la target à la stack
        if (add_to_stack) { stack.Add(new_target); }
        _capable = new_target;

        // on controle la nouvelle target
        control(new_target, duration);
    }
    public void BreakCapableTarget(Capable target)
    {
        if (!stack.Contains(target) || stack.Count <= 1) { return; }

        // on parcourt toute la stack depuis la fin pour voir jusqu'ou on remonte dans la stack
        for (int i = stack.Count - 1; i >= 0; --i)
        {
            Capable capa = stack[i];
            stack.RemoveAt(i);
            if (capa == target) { break; }
        }

        // on change de target
        ChangeCapableTarget(stack.Last(), add_to_stack: false);
    }
    public void ResetCapableTarget()
    {
        CancelInvoke("ResetCapableTarget");

        // on clear la stack
        stack.Clear();

        // on change la target pour le perso (ajoute automatiquement à la stack)
        ChangeCapableTarget(Perso.Instance, add_to_stack: true);
    }


    // CHANGE CAPABLE low level
    private void uncontrol_capable(Capable capa)
    {
        // reset les inputs de l'ancien capable
        capa.ClearInputs();
        if (capa.GetCapacity<WalkCapacity>() != null)
        {
            capa.GetCapacity<WalkCapacity>().walk_percentage_target = 0f;
        }

        // reset le behaviour
        if (capa is IA old_ia)
        {
            // on réactive l'ancien Brain si le capable actuel est une ia
            old_ia.Brain?.gameObject.SetActive(true);

            // on remet le tag
            old_ia.gameObject.tag = old_ia.BaseTag;

            // reset les tags d'attaques si on a
            if (old_ia.HasCapacity<AttackCapacity>())
            {
                old_ia.GetCapacity<AttackCapacity>().ResetTags();
            }
        }

        // reset l'inventory
        capa?.Inventory?.RemoveUI(perso_quick_inventory);
        perso_quick_inventory.Inventory = null;
    }
    private void control(Capable capa, float duration = -888f)
    {
        // on refresh la cam
        CameraFollow.Instance.RefreshTarget(capa);

        // si on a une durée, on reviens au perso après la durée
        CancelInvoke("ResetCapableTarget");
        if (duration != -888f) { Invoke("ResetCapableTarget", duration); }

        // refresh le see through pour remettre la tete bien centrée
        see_through.Refresh(capa);

        // on désactive le Brain si le nouveau capable est un IA
        if (capa is IA ia)
        {
            // désactive le cerveau
            ia.Brain?.gameObject.SetActive(false);

            // on remet le tag
            ia.gameObject.tag = "Controlled";

            // on clear les tags d'attaque pour pouvoir attaquer des gens
            if (ia.HasCapacity<AttackCapacity>())
            {
                ia.GetCapacity<AttackCapacity>().ClearTags();
            }
        }

        // on met le perso_quick_inventory sur la target si elle a un inventaire
        capa?.Inventory?.AddUI(perso_quick_inventory);
        perso_quick_inventory.Refresh();

        // on refresh le hackable navigator pour qu'il ait une nouvelle ConnectCapacity si jamais le capable a un device
        ConnectCapacity connector = capa.Connector;
        if (connector != null)
        {
            // HackableNavigator.SetConnector(connector);
            HackableNavigator.transform.localPosition = connector.transform.localPosition;
        }
    }
}