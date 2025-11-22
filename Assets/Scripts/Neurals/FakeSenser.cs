using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FakeSenser : MonoBehaviour
{
    [SerializeField] private NeuralBrain brain;

    [Header("Sensation")]
    [SerializeField] protected string type;
    [SerializeField] protected float intensity;
    [SerializeField] protected string initiator;
    [SerializeField] protected string receiver;

    public void SetProperties(string type, float intensity, string initiator, string receiver)
    {
        this.type = type;
        this.intensity = intensity;
        this.initiator = initiator;
        this.receiver = receiver;
    }

    public void Sense()
    {
        if (!Application.isPlaying) { return; }
        brain.Sense(type, intensity, initiator, receiver);
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(FakeSenser))]
public class FakeSenserEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Sense"))
        {
            FakeSenser myScript = (FakeSenser)target;
            myScript.Sense();
        }
    }
}

#endif