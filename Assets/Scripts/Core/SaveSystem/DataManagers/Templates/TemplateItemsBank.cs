
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;


public class TemplateItemsBank : MonoBehaviour
{
    [Header("Items templates bank")]

    [Header("Basics")]
    public List<Capable> items_templates = new List<Capable>();

    [Header("Food")]
    public List<Capable> food_templates = new List<Capable>();

    [Header("Modules")]
    public List<Capable> modules_templates = new List<Capable>();

    [Header("Files")]
    public List<Capable> files_templates = new List<Capable>();

    [Header("Exploits")]
    public List<Capable> exploits_templates = new List<Capable>();

    [Header("Keys")]
    public List<Capable> keys_templates = new List<Capable>();

    [Header("Others")]
    public List<Capable> others_templates = new List<Capable>();

    public List<Capable> GetAllCapablesTemplates()
    {
        List<Capable> all_templates = new List<Capable>();
        all_templates.AddRange(files_templates);
        all_templates.AddRange(items_templates);
        all_templates.AddRange(exploits_templates);
        all_templates.AddRange(keys_templates);
        all_templates.AddRange(others_templates);
        all_templates.AddRange(food_templates);
        all_templates.AddRange(modules_templates);
        return all_templates;
    }
}