using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(LightsDataExporter))]
public class LightsDataExporterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LightsDataExporter exporter = (LightsDataExporter)target;

        if (GUILayout.Button("Export Lights Data"))
        {
            exporter.ExportAndSave();
        }
    }
}

#endif