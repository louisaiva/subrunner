using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

public class UI_VirtualSkillTreeHandler : MonoBehaviour, I_UI_Slottable
{
    
    // perso
    private SkillTree tree;

    // uis
    private GameObject max_bits_lvl_label;
    private GameObject regen_bits_lvl_label;
    private GameObject portee_hack_lvl_label;

    private GameObject points_to_spend_label;

    private GameObject perso_level_label;

    // CALLBACK (we don't need it)
    public System.Action<InputAction.CallbackContext> CancelCallback { get { return ctx => { }; } }
    public void define_callback() { }

    // unity functions
    void Start()
    {
        // on récupère le skill tree
        tree = transform.parent.GetComponent<SkillTree>();

        // on récupère les labels
        max_bits_lvl_label = transform.Find("max_bits/lvl_label").gameObject;
        regen_bits_lvl_label = transform.Find("regen_bits/lvl_label").gameObject;
        portee_hack_lvl_label = transform.Find("portee_hack/lvl_label").gameObject;

        points_to_spend_label = transform.Find("level/tuto/points_to_spend").gameObject;

        perso_level_label = transform.Find("level").gameObject;
    }

    void Update()
    {
        // on update seulement si le skill tree est actif
        if (!tree.is_virtual_tree_open) { return; }

        // on met à jour les labels
        max_bits_lvl_label.GetComponent<TextMeshProUGUI>().text = "level " + tree.max_bits_level.ToString();
        regen_bits_lvl_label.GetComponent<TextMeshProUGUI>().text = "level " + tree.regen_bits_level.ToString();
        portee_hack_lvl_label.GetComponent<TextMeshProUGUI>().text = "level " + tree.portee_hack_level.ToString();

        points_to_spend_label.GetComponent<TextMeshProUGUI>().text = tree.virtual_points.ToString();

        perso_level_label.GetComponent<TextMeshProUGUI>().text = "COMPUTER LEVEL " + (tree.computer.niveau -1).ToString();
    }       


    // interface functions
    public List<GameObject> GetSlots(ref Vector2 base_position, ref float angle_threshold, ref float angle_multiplicator)
    {
        // on récupère les slots
        List<GameObject> slots = new List<GameObject>();
        slots.Add(transform.Find("max_bits").gameObject);
        slots.Add(transform.Find("regen_bits").gameObject);
        slots.Add(transform.Find("portee_hack").gameObject);

        // on récupère les variables
        base_position = transform.parent.position;
        angle_threshold = 90f;
        angle_multiplicator = 0f;

        return slots;
    }


}