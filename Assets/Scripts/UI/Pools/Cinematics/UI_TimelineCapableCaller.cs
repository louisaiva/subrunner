using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_TimelineCapableCaller : MonoBehaviour
{
    [SerializeField] private string capable_id;
    [SerializeField] private Capable capable;
    public Capable Capable { get { return capable; } }

    public void ConnectCapable()
    {
        // check if we have a capable id
        if (string.IsNullOrEmpty(capable_id)) { return; }

        if (capable_id == "controller")
        {
            if (Controller.Capable == null)
            {
                Debug.LogError("UI_TimelineCapableCaller: Controller has no capable");
                return;
            }
            capable = Controller.Capable;
            return;
        }

        if (!CapableBank.Instance.TryGetLoadedCapable(capable_id, out Capable found_capable))
        {
            Debug.LogWarning("UI_TimelineCapableCaller: No capable found with id " + capable_id);

            // we try to get the first one with the prefix
            if (!CapableBank.Instance.TryGeFirstCapableWithPrefix(World.Instance.GetPrefix(capable_id), out found_capable))
            {
                Debug.LogError("UI_TimelineCapableCaller: No capable found with prefix " + capable_id);
                return;
            }
        }

        capable = found_capable;

        // todo : we switch the brain mode of the capable to CinematicMode
    }

    // MOVING

    [Header("Positions for moving the capable")]
    [SerializeField] private List<Transform> position_transforms = new List<Transform>();
    public void MoveCapableToNextPosition()
    {
        if (capable == null) { Debug.LogError("UI_TimelineCapableCaller: No capable connected"); return; }
        if (position_transforms.Count == 0) { Debug.LogWarning("UI_TimelineCapableCaller: No position transforms set"); return; }

        // for now we simply override the position
        capable.transform.position = position_transforms[0].position;

        // then we move the used transform to the end of the list
        Transform used_transform = position_transforms[0];
        position_transforms.RemoveAt(0);
        position_transforms.Add(used_transform);
    }
    public void MoveCapableToPosition(int position_index)
    {
        if (capable == null) { Debug.LogError("UI_TimelineCapableCaller: No capable connected"); return; }

        // todo : here we call a specific capacity like walk capacity if we want to update our self ?
        // todo : or a small GOTO capacity that will handle the walk for us

        // for now we simply override the position
        if (position_index < 0 || position_index >= position_transforms.Count)
        {
            Debug.LogError("UI_TimelineCapableCaller: Position index out of range");
            return;
        }

        capable.transform.position = position_transforms[position_index].position;
    }
    public void SetCapableSpeed(float speed)
    {
        if (capable == null) { Debug.LogError("UI_TimelineCapableCaller: No capable connected"); return; }

        // todo : same shit here either we call the goto small handler or the walk capacity directly
    }


    // ANIMATIONS
    public void PlayAnim(string capacity)
    {
        if (capable == null) { Debug.LogError("UI_TimelineCapableCaller: No capable connected"); return; }
        capable.AnimPlayer.Play(capacity);
    }
    public void StopPlaying(string capacity)
    {
        if (capable == null) { Debug.LogError("UI_TimelineCapableCaller: No capable connected"); return; }
        capable.AnimPlayer.StopPlaying(capacity);
    }
    public void SetOrientation(string orientation)
    {
        if (capable == null) { Debug.LogError("UI_TimelineCapableCaller: No capable connected"); return; }
        switch (orientation)
        {
            case "U":
                capable.Orientation = Vector2.up;
                break;
            case "UL":
                capable.Orientation = new Vector2(-1, 1);
                break;
            case "UR":
                capable.Orientation = new Vector2(1, 1);
                break;
            case "D":
                capable.Orientation = Vector2.down;
                break;
            case "DL":
                capable.Orientation = new Vector2(-1, -1);
                break;
            case "DR":
                capable.Orientation = new Vector2(1, -1);
                break;
            case "L":
                capable.Orientation = Vector2.left;
                break;
            case "R":
                capable.Orientation = Vector2.right;
                break;
            default:
                Debug.LogWarning("UI_TimelineCapableCaller: Unknown orientation " + orientation);
                break;
        }
    }


    // SIT
    public void SitOnSofa(GameObject sofa_caller_go)
    {
        // we check if the sofa has a UI_TimelineCapableCaller component,
        if (!sofa_caller_go.TryGetComponent<UI_TimelineCapableCaller>(out UI_TimelineCapableCaller sofa_caller))
        {
            Debug.LogError("(UI_TimelineCapableCaller - SitOnSofa) Sofa does not have a UI_TimelineCapableCaller component");
            return;
        }
        if (sofa_caller.Capable == null)
        {
            Debug.LogError("(UI_TimelineCapableCaller - SitOnSofa) Sofa's UI_TimelineCapableCaller does not have a capable connected");
            return;
        }

        if (sofa_caller.Capable is not Sofa sofa)
        {
            Debug.LogError("(UI_TimelineCapableCaller - SitOnSofa) Sofa's UI_TimelineCapableCaller does not have a valid Sofa capable connected");
            return;
        }

        if (!capable.TryGetCapacity(out SitCapacity sit_capacity))
        {
            Debug.LogError("(UI_TimelineCapableCaller - SitOnSofa) Capable does not have a SitCapacity");
            return;
        }

        sit_capacity.Sit(sofa);
    }


    // TALK
    [Header("Dialogs")]
    [SerializeField] private List<string> dialog_lines_ids = new List<string>();
    public void SayNextLine()
    {
        if (dialog_lines_ids.Count == 0) { Debug.LogWarning("UI_TimelineCapableCaller: No dialog lines available"); return; }
        Say(dialog_lines_ids[0]);
        dialog_lines_ids.RemoveAt(0);
    }
    public void Say(string msg_id)
    {
        if (capable == null) { Debug.LogError("UI_TimelineCapableCaller: No capable connected"); return; }

        if (!capable.TryGetCapacity(out TalkCapacity talk_capacity))
        {
            Debug.LogError("UI_TimelineCapableCaller: Capable does not have a TalkCapacity");
            return;
        }

        talk_capacity.Say(msg_id);
    }
}