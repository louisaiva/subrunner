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

    // WIRED SETTING
    private Setting wired_setting;

    private void Start() { register_settings(); }
    private void OnDestroy() { unregister_settings(); }



    // SETTINGS REGISTERING
    private void register_settings()
    {
        wired_setting = SettingsManager.Instance.GetSetting("wired");
        if (wired_setting != null) { wired_setting.OnValueChanged += handle_wired_switched; handle_wired_switched(wired_setting.Value); }
    }
    private void unregister_settings()
    {
        // enleve les callbacks des settings
        if (wired_setting != null) { wired_setting.OnValueChanged -= handle_wired_switched; }
    }

    
    // CALLBACKS
    private void handle_wired_switched(float setting_value)
    {
        bool is_wired = setting_value > 0.5f;
        if (is_wired) { SwitchRendererData("wired"); }
        else { SwitchRendererData("default"); }
    }

    // SWITCH RENDERER DATA
    public void SwitchRendererData(string rendererDataName) => SwitchRendererData(rendererDataIndices[rendererDataName]);
    public void SwitchRendererData(int index)
    {
        Debug.Log("(CameraRendererDataSwitcher) SwitchRendererData : " + index);

        UniversalAdditionalCameraData data = cam.GetUniversalAdditionalCameraData();
        if (data == null) { return; }

        data.SetRenderer(index);
    }
}