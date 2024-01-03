using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AreaJsonHandler))]
public class AreaJsonEditor: Editor {
    public override void OnInspectorGUI() {

        AreaJsonHandler areaBuilder = (AreaJsonHandler)target;
        if (GUILayout.Button("Save Json"))
        {
            areaBuilder.SaveTileAreas();
        }
        else if (GUILayout.Button("Select all Areas"))
        {
            areaBuilder.SelectAllAreas();
        }
        else if (GUILayout.Button("Load Area"))
        {
            areaBuilder.LoadArea();
        }
        else if (GUILayout.Button("Save all Areas"))
        {
            areaBuilder.SaveAllAreas();
        }

        DrawDefaultInspector();
    }
}