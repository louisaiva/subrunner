using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// this script is the main script for handling camera movement.
/// It can work with any capable, or with a SimpleCameraController
/// </summary>
public class CameraFollow : Singleton<CameraFollow>
{
    /// <summary>
    /// Simple Target is a simple controller using ZQSD (WASD) inputs.
    /// It is not singleton so that's why we cache it here.
    /// </summary>
    private SimpleCameraController _simple_target;
    public SimpleCameraController SimpleTarget
    {
        get
        {
            if (_simple_target == null)
            {
                _simple_target = FindFirstObjectByType<SimpleCameraController>(FindObjectsInactive.Include);
            }
            return _simple_target;
        }
    }


    [Header("Blended Targets")]
    private List<CameraTarget> targets = new List<CameraTarget>();
    [SerializeField] private List<Transform> debug_targets = new List<Transform>(); // only for showing targets in the inspector
    private Rigidbody2D rb;

    [Header("Lerp parameters")]
    public float timeOffset;
    private Vector3 velocity;

    [Header("Size")]
    [SerializeField] private float default_size = 3f;
    private float target_size = 3f;

    // these parameters are used for the dynamic camera mode,
    // which is basically never used but adapt camera movement
    // based on rigidbody velocity vector
    [Header("Dynamic camera parameters")]
    [SerializeField] private bool dynamic_cam = false;
    public float Y_OFF;
    [SerializeField] private float min_velocity = 0.5f;
    [SerializeField] private float Y_OFF_MAX = 1.5f;
    [SerializeField] private float Y_OFF_SPEED = 0.5f;

    [Header("Logs")]
    [SerializeField] private bool log_target = false;

    // TARGET ADD / REMOVE
    public void AddTarget(MonoBehaviour mono, bool tp = false, float weight = 1f, float size = 3f)
    {
        AddTarget(mono.transform, tp, weight, size);
    }
    public void AddTarget(Transform new_target, bool tp = false, float weight = 1f, float size = 3f)
    {
        if (new_target == SimpleTarget.transform) { add_target(SimpleTarget.Target, tp); return; }

        // else we create a new CameraTarget and we add it
        add_target(new CameraTarget(new_target, weight, size), tp);
    }
    private void add_target(CameraTarget new_target, bool tp = false)
    {
        if (new_target == null) { return; }
        if (has_target(new_target)) { return; }
        if (log_target) { Debug.Log($"(CameraFollow) Adding target '{new_target.ID}' with weight {new_target.Weight} and size {new_target.Size}"); }
        targets.Add(new_target);
        debug_targets.Add(new_target.Target);

        if (tp) { transform.position = new Vector3(new_target.Target.position.x, new_target.Target.position.y, transform.position.z); }

        // we also check if we have no rigidbody and this new target has one we set it
        if (rb != null) { return; }
        rb = new_target.Target.GetComponent<Rigidbody2D>();
    }
    public void RemoveTarget(MonoBehaviour mono)
    {
        RemoveTarget(mono.transform);
    }
    public void RemoveTarget(Transform target_to_remove)
    {
        if (target_to_remove == SimpleTarget.transform) { remove_target(SimpleTarget.Target); return; }

        // we check if we have one
        foreach (CameraTarget target in targets)
        {
            if (target.Target == target_to_remove)
            {
                remove_target(target);
                return;
            }
        }
    }
    private void remove_target(CameraTarget target_to_remove)
    {
        if (target_to_remove == null) { return; }
        if (!has_target(target_to_remove)) { return; }
        targets.Remove(target_to_remove);
        debug_targets.Remove(target_to_remove.Target);
        
        if (log_target) { Debug.Log($"(CameraFollow) Removed camera target '{target_to_remove.ID}'  "); }
        
        // we also check if the removed target is the one we are following with a rigidbody, if yes we remove it
        if (rb != null && target_to_remove.Target.GetComponent<Rigidbody2D>() == rb)
        {
            rb = null;
            if (log_target) { Debug.Log($"(CameraFollow) Camera target was the rigidbody, so we removed it as well and disabled dynamic camera"); }
        }
    }
    public void SetSingleTarget(MonoBehaviour new_target, bool tp = false, float weight = 1f, float size = 3f)
    {
        SetSingleTarget(new_target.transform, tp, weight, size);
    }
    public void SetSingleTarget(Transform new_target, bool tp = false, float weight = 1f, float size = 3f)
    {
        // we remove all other targets
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            if (targets[i].Target != new_target) { remove_target(targets[i]); }
        }
        AddTarget(new_target, tp);
    }
    public void ResetCameraToController(bool tp = false)
    {
        if (Controller.Capable == null) { return; }
        SetSingleTarget(Controller.Capable, tp: tp);
    }
    // UPDATE
    private void Update()
    {
        if (targets.Count == 0) { return; }

        // lerp zoom
        target_size = compute_target_size();
        if (Camera.main.orthographicSize != target_size)
        {
            float next_size = Mathf.Lerp(Camera.main.orthographicSize, target_size, Time.deltaTime * 5f);
            if (Mathf.Abs(next_size - target_size) < 0.01f) { next_size = target_size; }
            Camera.main.orthographicSize = next_size;
        }

        // lerp position
        Vector3 final_position = compute_target_position();
        transform.position = Vector3.SmoothDamp(transform.position, final_position, ref velocity, timeOffset);
    }
    private Vector3 compute_dynamic_position(Vector3 target_position, Rigidbody2D rb)
    {
        /* if (Controller.Capable == null || Controller.LazyInstance.PIC.InputsDisabled)
        {
            RemoveCapableTarget();
            return;
        }
        else if (target == null)
        {
            ChangeCapableTarget(Controller.Capable, tp: false);
        } */

        // calcule le mouvement de la cam en X
        float final_x = target_position.x;
        float x_movement = final_x - transform.position.x;


        // on ajuste l'offset en fonction de la vitesse du joueur en Y
        if (dynamic_cam && rb != null)
        {
            Y_OFF = 0;
            if (Mathf.Abs(rb.linearVelocity.y) > min_velocity)
            {
                Y_OFF = (rb.linearVelocity.y - Mathf.Sign(rb.linearVelocity.y) * min_velocity) * Y_OFF_SPEED;
                Y_OFF = Mathf.Clamp(Y_OFF, -Y_OFF_MAX, Y_OFF_MAX);
            }
        }


        // calcule le mouvement de la cam en Y
        float final_y = target_position.y + Y_OFF;
        float y_movement = final_y - transform.position.y;

        // on calcule la position finale de la cam, puis on la déplace
        Vector3 final_position = new Vector3(final_x, final_y, transform.position.z);
        return final_position;
        // transform.position = Vector3.SmoothDamp(transform.position, final_position, ref velocity, timeOffset);
    }
    private float compute_target_size()
    {
        float blended_size = 0f;
        float total_weight = 0f;
        foreach (CameraTarget target in targets)
        {
            blended_size += target.Size * target.Weight;
            total_weight += target.Weight;
        }
        if (total_weight == 0f) { return default_size; }
        return blended_size / total_weight;
    }
    private Vector3 compute_target_position()
    {
        Vector3 blended_position = Vector3.zero;
        float total_weight = 0f;
        foreach (CameraTarget target in targets)
        {
            total_weight += target.Weight;
            if (dynamic_cam && target.Target.TryGetComponent(out Rigidbody2D rb) && rb == this.rb)
            {
                blended_position += compute_dynamic_position(target.Target.position, rb);
                continue;
            }
            
            // else it is basic blend mode so we just blend the positions based on the weights
            // this is the default mode and is used in 99% of the cases
            // (subrunner 0.0.1 was maybe the last time the other mode was used lol. i still keep it tho bcz i'm nostalgic)
            blended_position += (Vector3)target.Target.position * target.Weight;
        }
        if (total_weight == 0f) { return transform.position; }
        blended_position /= total_weight;
        blended_position.z = transform.position.z; // we keep the original z position of the camera
        blended_position.y += Y_OFF; // we add the Y offset to the blended position
        return blended_position;
    }

    // SIZE SETTER
    public float GetSize() { return target_size; }
    public float GetSizeRelativeToDefault() { return target_size / default_size; }


    // GETTER
    private bool has_target(MonoBehaviour mono)
    {
        return has_target(mono.transform);
    }
    private bool has_target(CameraTarget target)
    {
        return has_target(target.Target);
    }
    private bool has_target(Transform target)
    {
        foreach (CameraTarget ct in targets)
        {
            if (ct.Target == target) { return true; }
        }
        return false;
    }
}

[Serializable] public class CameraTarget
{
    public Transform Target;
    public GameObject gameObject { get; private set; }
    public string ID; // only for debug purposes, not used by the camera
    public float Weight; // weight for blending multiple targets
    public float Size; // desired camera size when focusing on this target. if multiple targets, size will be blended based on weight

    public CameraTarget(Transform target, float weight = 1f, float size = 3f)
    {
        Target = target;
        gameObject = target.gameObject;
        Weight = weight;
        Size = size;

        if (target.TryGetComponent(out Capable capable))
        {
            ID = capable.ID;
        }
        else if (target.TryGetComponent(out SimpleCameraController scc))
        {
            ID = "simple camera controller";
        }
        else
        {
            ID = target.name;
        }
    }
}