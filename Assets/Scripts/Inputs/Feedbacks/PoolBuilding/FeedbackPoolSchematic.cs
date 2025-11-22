using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "FeedbackPoolSchematic", menuName = "IF/PoolSchematic", order = 1)]
public class FeedbackPoolSchematic : ScriptableObject
{
    public List<FeedbackRowSchematic> kb_L_plan;
    public List<FeedbackRowSchematic> kb_R_plan;
    public List<FeedbackRowSchematic> gmpd_L_plan;
    public List<FeedbackRowSchematic> gmpd_R_plan;
}

[Serializable] public class FeedbackRowSchematic
{
    public string label_text;
    public Color base_color = Color.white;
    public Color inputed_color = Color.green;
    public List<GameObject> prefabs;
}


