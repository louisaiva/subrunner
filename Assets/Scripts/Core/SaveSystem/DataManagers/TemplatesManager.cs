
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class TemplatesManager : MonoBehaviour
{
    private string templates_data_path = "Assets/Resources/data/templates/";


    // TEMPLATES BANKS
    private TemplateObjectsBank _object_bank;
    public TemplateObjectsBank ObjectBank
    {
        get
        {
            if (_object_bank != null) { return _object_bank; }
            _object_bank = GetComponentInChildren<TemplateObjectsBank>(includeInactive: true);
            if (_object_bank == null) { Debug.LogError("(TemplatesManager) no TemplateObjectsBank in children !!"); }
            return _object_bank;
        }
    }
    private TemplateItemsBank _item_bank;
    public TemplateItemsBank ItemBank
    {
        get
        {
            if (_item_bank != null) { return _item_bank; }
            _item_bank = GetComponentInChildren<TemplateItemsBank>(includeInactive: true);
            if (_item_bank == null) { Debug.LogError("(TemplatesManager) no TemplateItemsBank in children !!"); }
            return _item_bank;
        }
    }



    [Header("Capables Templates")]
    public List<Capable> capables_templates = new List<Capable>(); // these capables are in the prefabs, not loaded in the scene
    
    [Header("Capacities Templates")]
    public List<Capacity> capacities_templates = new List<Capacity>(); // these capacities are in the prefabs, not loaded in the scene

    [Header("Extended parameters")]
    public bool save_capables_capacities_as_templates = false; // if true, when saving capable templates,
    // it will also save the capacities of these capables as templates (if they are not already templates)
    // public bool save_capables_in_inventory = false;

    [Header("Logs")]
    public bool log = false;
    public bool log_extended = false;
    public Loggable<TemplatesManager> log_banks;


    // CAPABLES TEMPLATES SAVING
    public void SaveCapableTemplates()
    {
        // we first need to ensure that MaterialBank is loaded bcz we need to get the paths
        foreach (Capable capable in capables_templates)
        {
            saveCapableTemplate(capable);
        }

        // we also get all the objects templates from the TemplateObjectsBank and save them as capable templates
        List<Capable> objects = ObjectBank.GetAllCapablesTemplates();
        foreach (Capable obj in objects)
        {
            saveCapableTemplate(obj);
        }
        log_banks.Log($"Saved {objects.Count} OBJECTS templates from TemplateObjectsBank");

        // and the items templates
        List<Capable> items = ItemBank.GetAllCapablesTemplates();
        foreach (Capable obj in items)
        {
            saveCapableTemplate(obj);
        }
        log_banks.Log($"Saved {items.Count} ITEMS templates from TemplateItemsBank");

        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
    }
    private void saveCapableTemplate(Capable capable)
    {
        CapableData data = (CapableData) capable.GetStaticData();

        // we apply some modifs to the data, i.e. adding some specific capacities templates
        // this is mainly for simplifying the work flow inside unity, bcz with data-driven approach
        // we don't have a lot of control on modifying data in scene view
        // We do most heavy job with templates, AND SO we must be fast
        filter_capable_data(capable, ref data);

        // save the current data to a json file
        string json = JsonUtility.ToJson(data, true);
        string path = templates_data_path + "capables/" + data.id + ".json";
        System.IO.File.WriteAllText(path, json, System.Text.Encoding.UTF8);

        if (log) { Debug.Log($"(TemplatesManager - Save Capable) Updated & Saved CapableData : {capable.name} (to {path})\n\n{data.GetDetails()}\n\n{json}"); }

        // we also save the capacities of this capable if we want to
        if (!save_capables_capacities_as_templates) { return; }

        // we get all the capacities (ONLY DIRECT CHILDREN - we don't want to get the capa of the items we store :)
        List<Capacity> capacities = capable.GetStaticCapacities();

        // then we save all the data of these capacities
        foreach (Capacity capacity in capacities)
        {
            // get the id of this capacity template (if it has one)
            CapacityData capa_data = capacity.GetStaticData();
            if (capa_data == null) { continue; }
            if (capa_data.id == null || capa_data.id == "") { continue; }
            if (capa_data.id.Contains("-")) { continue; } // if the id contains a "-", we consider that it's not a template (because templates id can't contain "-" in their id, but spawned capacities have an id with "-" followed by random chars to be unique), so we don't save it as a template

            // we save this capacity as a template
            saveCapacityTemplate(capacity);
        }
    }
    private void filter_capable_data(Capable capable, ref CapableData data)
    {
        // we check if the capable has some TemplateCapacityReference components, and we save the id of the ref
        TemplateCapacityReference[] template_capacity_refs = capable.GetComponentsInChildren<TemplateCapacityReference>(includeInactive: false);
        for (int i = 0; i < template_capacity_refs.Length; i++)
        {
            TemplateCapacityReference template_capacity_ref = template_capacity_refs[i];
            if (template_capacity_ref == null) { continue; }

            // add the rest of the templates
            if (template_capacity_ref.capacity_template_ids != null && template_capacity_ref.capacity_template_ids.Count > 0)
            {
                foreach (string capacity in template_capacity_ref.capacity_template_ids)
                {
                    if (string.IsNullOrEmpty(capacity)) { continue; }
                    data.capacities_ids.Add(capacity);
                }
            }

            // add the main template
            string template = template_capacity_ref.capacity_template_id;
            if (template == null) { continue; }
            if (string.IsNullOrEmpty(template)) { continue; }
            data.capacities_ids.Add(template);
        }

        // we check if the capable has some TemplateInventoryReference components, and we save the id of the ref
        TemplateInventoryReference inv_temp = capable.GetComponent<TemplateInventoryReference>();
        if (inv_temp != null)
        {
            data.inventory = inv_temp.GetInventoryData();
        }

        // we also make sure all the items have is_grabbed to true by default
        if (data is ItemData item_data) { item_data.is_grabbed = true; }
    }


    // CAPACITIES TEMPLATES SAVING
    private void SaveCapacitiesTemplates()
    {

        foreach (Capacity capacity in capacities_templates)
        {
            saveCapacityTemplate(capacity);
        }

        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
    }
    private void saveCapacityTemplate(Capacity capacity)
    {
        CapacityData capacity_data = capacity.GetStaticData();

        // save the current data to a json file
        string capacity_json = JsonUtility.ToJson(capacity_data, true);
        string path = templates_data_path + "capacities/" + capacity_data.id + ".json";
        System.IO.File.WriteAllText(path, capacity_json, System.Text.Encoding.UTF8);

        if (log) { Debug.Log($"(TemplatesManager - Save Capacity) Updated & Saved CapacityData : {capacity.ID} (to {path})\n\n{capacity_data.GetDetails()}\n\n{capacity_json}"); }
    }



#if UNITY_EDITOR
    [CustomEditor(typeof(TemplatesManager))]
    public class TemplatesManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            TemplatesManager manager = (TemplatesManager)target;

            DrawDefaultInspector();
            if (GUILayout.Button("Save Capable Templates")) { manager.SaveCapableTemplates(); }
            if (GUILayout.Button("Save Capacity Templates")) { manager.SaveCapacitiesTemplates(); }
        }
    }
#endif
}