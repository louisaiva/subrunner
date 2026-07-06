using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingDmgProvider : MonoBehaviour
{
    
    // cette classe permet de générer des dégats flottants au dessus d'un objet
    // elle est utilisée par les beings pour afficher les dégats qu'ils reçoivent

    [Header("Damage Counter parameters")]
    [SerializeField] private float dmg_size = 20f;
    [SerializeField] private float random_position_range = 0.1f;
    [SerializeField] private float offset_y = 0.5f;

    // show damages once a while if there are too many
    private float show_dmg_delay = 0.1f;
    private float free_dmg_delay = 10f; // on supprime les dégats si on a pas fait de dégats depuis ce temps
    private Dictionary<Capable, FloatingDmgState> capables_dmgs = new Dictionary<Capable, FloatingDmgState>();

    // AWAKE & INSTANCE
    private TextManager _text_manager;
    public TextManager TextManager
    {
        get
        {
            if (_text_manager == null) { _text_manager = GetComponent<TextManager>(); }
            return _text_manager;
        }
    }
    public static FloatingDmgProvider Instance { get; private set; }
    protected void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this.gameObject); }
        else { Instance = this; }
    }

    // UPDATE
    private readonly Stack<Capable> outdated_dmg = new Stack<Capable>();
    private readonly List<Capable> reset_dmg = new List<Capable>();
    protected void Update()
    {
        // on prepare la suppression des objets qui n'ont pas recu de dégats depuis un moment
        outdated_dmg.Clear();
        reset_dmg.Clear();

        // on parcourt les objets
        foreach (KeyValuePair<Capable, FloatingDmgState> kvp in capables_dmgs)
        {
            // on regarde si on doit afficher les dégats
            if (Time.time - kvp.Value.last_time_shown < show_dmg_delay) { continue; }

            // on vérifie si on doit supprimer l'objet
            if (kvp.Value.pending_dmg == 0f)
            {
                outdated_dmg.Push(kvp.Key);
                continue;
            }

            // on vérfie si on fait au moins 1 dégat
            if (Mathf.Abs(kvp.Value.pending_dmg) < 1f)
            {
                // * il se peut que l'objet ait reçu des dégats inférieurs à 1
                // * mais pas depuis hyper longtemps -> on stocke le dégat pour "rien"
                // * donc on le supprime si on a pas fait de dégats depuis un moment
                if (kvp.Value.last_time_shown != -1
                    && Time.time - kvp.Value.last_time_shown > free_dmg_delay)
                { outdated_dmg.Push(kvp.Key); }
                continue;
            }

            // on affiche les dégats
            create_floating_dmg((int)kvp.Value.pending_dmg, kvp.Key.transform.position);

            reset_dmg.Add(kvp.Key); // on reset le compteur de dégats pour cet objet

        }

        // on supprime les objets
        foreach (Capable capa in outdated_dmg) { capables_dmgs.Remove(capa); }

        // on reset les dégats
        foreach (Capable capa in reset_dmg)
        {
            FloatingDmgState state = capables_dmgs[capa];
            state.pending_dmg = 0f;
            state.last_time_shown = Time.time;
            capables_dmgs[capa] = state; // write back
        }
    }

    // DMG PROVIDER
    public void AddFloatingDmg(Capable capable, float dmg)
    {
        // if we already have a counter for this capable, we update it
        if (capables_dmgs.TryGetValue(capable, out FloatingDmgState state))
        {
            state.pending_dmg += dmg;
            capables_dmgs[capable] = state; // write back
            return;
        }
        
        // else we initiate the damage counter for this capable
        capables_dmgs[capable] = new FloatingDmgState
        {
            pending_dmg = dmg,
            last_time_shown = -1
        };
    }
    public void AddMissed(Vector3 position)
    {
        // on met un peu d'aléatoire dans la position
        position.y += offset_y;
        position.x += Random.Range(-random_position_range, random_position_range);

        // on génère un floating dmg
        FloatingTextPooler.Instance.LoadText("missed", "yellow", position, dmg_size);
    }
    
    // LOW LEVEL DMG
    private void create_floating_dmg(int dmg, Vector3 position)
    {
        if (dmg == 0) { return; }

        // on met un peu d'aléatoire dans la position
        position.y += offset_y;
        position.x += Random.Range(-random_position_range, random_position_range);

        // on ajuste la couleur en fonction des dégats
        string color;
        if (dmg > 0) { color = "green"; }
        else { color = "red"; }

        // on génère un floating dmg
        FloatingTextPooler.Instance.LoadText(dmg.ToString(), color, position, dmg_size);
    }
}

public struct FloatingDmgState
{
    public float pending_dmg;
    public float last_time_shown;
}