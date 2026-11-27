using Unity.VisualScripting;
using UnityEngine;

public class UI_CmdLine : MonoBehaviour
{
    [SerializeField] protected UI_Writer lefter;
    [SerializeField] protected UI_Writer righter;

    [Header("Colors")]
    [SerializeField] protected Color left_writing_color = Color.white;
    [SerializeField] protected Color left_working_color = Color.white;
    [SerializeField] protected Color left_done_color = Color.white;
    [SerializeField] protected Color right_writing_color = Color.white;
    [SerializeField] protected Color right_working_color = Color.white;
    [SerializeField] protected Color right_done_color = Color.white;

    public bool IsDone => !IsWriting && !IsWorking;
    public virtual bool IsWriting => lefter.IsWriting || righter.IsWriting;
    public virtual bool IsWorking => false;

    protected virtual void Update()
    {
        if (IsWorking)
        {
            lefter.SetColor(left_working_color);
            righter.SetColor(right_working_color);
            return;
        }
        if (IsWriting)
        {
            lefter.SetColor(left_writing_color);
            righter.SetColor(right_writing_color);
            return;
        }

        lefter.SetColor(left_done_color);
        righter.SetColor(right_done_color);
    }

}