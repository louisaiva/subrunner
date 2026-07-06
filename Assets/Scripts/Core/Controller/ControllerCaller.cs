using UnityEngine;
/// <summary>
/// this class is a helper class to call some things
/// for the controller
/// </summary>
public class ControllerCaller : MonoBehaviour
{
    // INPUTS
    public void EnableInputs()
    {
        Controller.LazyInstance?.PIC.EnableInputs();
        InputManager.Instance.EnablePersoInputs();

        // we also reset the camera to the controller
        CameraFollow.Instance?.ResetCameraToController();
    }
    public void DisableInputs() => Controller.LazyInstance?.PIC.DisableInputs();
}
