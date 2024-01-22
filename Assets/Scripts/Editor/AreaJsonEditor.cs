using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AreaJsonHandler))]
public class AreaJsonEditor: Editor {
    public override void OnInspectorGUI() {

        AreaJsonHandler areaBuilder = (AreaJsonHandler)target;
        if (GUILayout.Button("Save Areas to Json"))
        {
            areaBuilder.SaveAreasToJson();
        }
        else if (GUILayout.Button("Save Areas to Prefabs"))
        {
            areaBuilder.SaveAreasToPrefab();
        }
        else if (GUILayout.Button("Select all Areas"))
        {
            areaBuilder.SelectAllAreas();
        }
        else if (GUILayout.Button("Select all Sas areas"))
        {
            areaBuilder.SelectAllSasAreas();
        }
        else if (GUILayout.Button("Load Area"))
        {
            areaBuilder.LoadArea();
        }

        DrawDefaultInspector();
    }
}