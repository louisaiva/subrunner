using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraRendererDataSwitcher : MonoBehaviour
{
    private Camera _cam;
    private Camera cam
    {
        get
        {
            if (_cam == null) { _cam = GetComponent<Camera>(); }
            return _cam;
        }
    }

    private Dictionary<string, int> rendererDataIndices = new Dictionary<string, int>() {
        { "default", 0 },
        { "wired", 1 }
    };

    public void SwitchRendererData(string rendererDataName) => SwitchRendererData(rendererDataIndices[rendererDataName]);
    public void SwitchRendererData(int index)
    {
        UniversalAdditionalCameraData data = cam.GetUniversalAdditionalCameraData();
        if (data == null) { return; }

        data.SetRenderer(index);
    }
}