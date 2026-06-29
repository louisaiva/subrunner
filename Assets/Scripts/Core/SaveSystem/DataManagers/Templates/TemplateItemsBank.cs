
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;


public class TemplateItemsBank : MonoBehaviour
{

    [Header("Items templates folders")]
    public List<string> template_paths = new List<string>();
    public List<Capable> GetAllCapablesTemplates()
    {
        List<Capable> all_templates = new List<Capable>();
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