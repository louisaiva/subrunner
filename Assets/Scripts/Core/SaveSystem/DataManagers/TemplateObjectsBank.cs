
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


    public List<Capable> GetAllCapablesTemplates()
    {
        List<Capable> all_templates = new List<Capable>();
        all_templates.AddRange(doors_templates);
        all_templates.AddRange(chests_templates);
        all_templates.AddRange(sofas_templates);
        all_templates.AddRange(spawners_templates);
        all_templates.AddRange(others_templates);
        return all_templates;
    }
}