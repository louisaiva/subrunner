using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MotherboardBuilder))]
public class MotherboardBuilderEditor : Editor
{
    public override void OnInspectorGUI() {
        MotherboardBuilder builder = (MotherboardBuilder)target;
        if (GUILayout.Button("Build Motherboard"))
        {
            builder.BuildMotherboard(builder.Size.x, builder.Size.y);
        }

        DrawDefaultInspector();
    }
}