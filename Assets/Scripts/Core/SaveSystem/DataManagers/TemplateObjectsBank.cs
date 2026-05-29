
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;


public class TemplateObjectsBank : MonoBehaviour
{
    [Header("Objects templates bank")]

    [Header("Doors")]
    public List<Capable> doors_templates = new List<Capable>();

    [Header("Chests")]
    public List<Capable> chests_templates = new List<Capable>();

    [Header("Sofas")]
    public List<Capable> sofas_templates = new List<Capable>();

    [Header("Spawners")]
    public List<Capable> spawners_templates = new List<Capable>();

    [Header("Others")]
    public List<Capable> others_templates = new List<Capable>();

    [Header("Specific folders to find templates")]
    public List<string> template_paths = new List<string>();


    public List<Capable> GetAllCapablesTemplates()
    {
        List<Capable> all_templates = new List<Capable>();
        all_templates.AddRange(doors_templates);
        all_templates.AddRange(chests_templates);
        all_templates.AddRange(sofas_templates);
        all_templates.AddRange(spawners_templates);
        all_templates.AddRange(others_templates);
        all_templates.AddRange(get_all_aditional_templates_from_paths());
        return all_templates;
    }

    private List<Capable> get_all_aditional_templates_from_paths()
    {
        List<Capable> templates = new List<Capable>();
        foreach (string path in template_paths)
        {
            templates.AddRange(get_templates_from_path(path));
        }
        return templates;
    }
    private List<Capable> get_templates_from_path(string path)
    {
        Capable[] templates = Resources.LoadAll<Capable>(path);
        // Debug.Log($"[TemplateObjectsBank] Found {templates.Length} templates in path : {path}");
        return new List<Capable>(templates);
    }
}