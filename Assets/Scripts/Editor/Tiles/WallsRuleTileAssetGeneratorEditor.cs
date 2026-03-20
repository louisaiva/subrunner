#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WallsRuleTileAssetGenerator))]
public class WallsRuleTileAssetGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("templateAsset"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("targetSpriteSheet"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("assetSuffix"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mappingMode"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("overwriteIfExists"));

        var generator = (WallsRuleTileAssetGenerator)target;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "This generator copies the template YAML, remaps sprite fileIDs using deterministic mapping (Position or Name), replaces the spritesheet GUID, and writes a new WallRuleTile asset in the same folder as this generator asset.",
            MessageType.Info);

        EditorGUILayout.LabelField("Output Preview", generator.GetOutputPreviewPath());

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(Application.isPlaying))
        {
            if (GUILayout.Button("Generate Wall Rule Tile Asset"))
            {
                serializedObject.ApplyModifiedProperties();

                if (generator.TryGenerate(out var outputPath, out var error))
                {
                    var outputAsset = AssetDatabase.LoadAssetAtPath<Object>(outputPath);
                    Selection.activeObject = outputAsset;
                    if (outputAsset != null)
                        EditorGUIUtility.PingObject(outputAsset);

                    Debug.Log($"Generated WallRuleTile asset: {outputPath}");
                }
                else
                {
                    EditorUtility.DisplayDialog("Generation Failed", error, "OK");
                    Debug.LogError(error);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
