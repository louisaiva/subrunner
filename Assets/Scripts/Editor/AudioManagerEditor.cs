using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioManager))]
[CanEditMultipleObjects]
public class AudioManagerEditor: Editor {


    public override void OnInspectorGUI() {

        AudioManager manager = (AudioManager)target;
        if (GUILayout.Button("Add Sounds from Path"))
        {
            manager.AddSoundsFromDefaultPath();
        }

        DrawDefaultInspector();
    }
}