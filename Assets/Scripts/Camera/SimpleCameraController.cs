using UnityEngine;

/// <summary>
/// this is a simple camera controller that allows zqsd (wasd) movement
/// on the transform. Used by CameraFollow to apply dynamic camera movement
/// i.e. when in LevelBuilder
/// </summary>
public class SimpleCameraController : MonoBehaviour
{
    public float move_speed = 5f;

    private void Update()
    {
        if (!gameObject.activeSelf) { return; }
        if (InputManager.Instance == null) { return; }
        if (CameraFollow.Instance == null) { return; }

        // read raw inputs
        Vector2 raw_inputs = InputManager.Instance.CameraMovementInputs;

        // we check if the raw inputs are below the deadzone
        raw_inputs.x = Mathf.Abs(raw_inputs.x) < InputManager.Instance.JOYSTICK_MIN_THRESHOLD ? 0f : raw_inputs.x;
        raw_inputs.y = Mathf.Abs(raw_inputs.y) < InputManager.Instance.JOYSTICK_MIN_THRESHOLD ? 0f : raw_inputs.y;

        // we normalize the inputs
        Vector2 direction = raw_inputs.normalized;

        // we change our position
        transform.position += new Vector3(direction.x, direction.y, 0f) * move_speed * Time.deltaTime * CameraFollow.Instance.GetSizeRelativeToDefault();
    }
}