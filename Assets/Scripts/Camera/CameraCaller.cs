using UnityEngine;
public class CameraCaller : MonoBehaviour
{

    // CAMERA HANDLING
    [Header("Camera targeting")]
    [SerializeField] private bool tp_on_target = false;
    [SerializeField] private float camera_target_size = 3f;
    [SerializeField] private float camera_target_weight = 1f;

    // CAMERA
    public void TargetCamera() => CameraFollow.Instance?.AddTarget(transform, tp:tp_on_target, camera_target_weight, camera_target_size);
    public void MakeCameraTargetThisOnly() => CameraFollow.Instance?.SetSingleTarget(transform, tp:tp_on_target, camera_target_weight, camera_target_size);
    public void StopTargetingCamera() => CameraFollow.Instance?.RemoveTarget(transform);
    public void ResetCameraToController() => CameraFollow.Instance?.ResetCameraToController(tp: tp_on_target);
}