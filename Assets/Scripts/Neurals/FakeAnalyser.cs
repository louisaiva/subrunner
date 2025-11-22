using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FakeAnalyser : MonoBehaviour
{
    [SerializeField] private NeuralBrain brain;
    public void Reinforce()
    {
        if (!Application.isPlaying) { return; }
        if (brain.instant_memory == null) { Debug.LogWarning("[FakeAnalyser] No instant memory to reinforce."); return; }
        brain.instant_memory.Reinforce();
        Debug.Log("[FakeAnalyser] Reinforced memory that led to decision: " + brain.instant_memory.decision?.name);
    }
    public void Fragilise()
    {
        if (!Application.isPlaying) { return; }
        if (brain.instant_memory == null) { Debug.LogWarning("[FakeAnalyser] No instant memory to fragilise."); return; }
        brain.instant_memory.Fragilise();
        Debug.Log("[FakeAnalyser] Fragilised memory that led to decision: " + brain.instant_memory.decision?.name);
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(FakeAnalyser))]
public class FakeAnalyserEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Reinforce"))
        {
            FakeAnalyser myScript = (FakeAnalyser)target;
            myScript.Reinforce();
        }
        if (GUILayout.Button("Fragilise"))
        {
            FakeAnalyser myScript = (FakeAnalyser)target;
            myScript.Fragilise();
        }
    }
}

#endif