using System.Collections;
using UnityEngine;

public class UI_CapablePlacerPool : UI_SlottablePool
{

    [Header("References")]
    [SerializeField] private UI_CycleButton magnetism_cycle_btn;

    private void Start()
    {
        SettingsManager.Instance.RegisterCallback("object_placer_magnetism", WorldPlacer.LazyInstance.SetGridSetting);

        // also make sure the UI_Button is updated with the current value of the setting
        Setting magnetism_setting = SettingsManager.Instance.GetSetting("object_placer_magnetism");
        if (magnetism_setting != null)
        {
            magnetism_cycle_btn.start_cycle_index = magnetism_setting.Value > 0.5f ? 1 : 0;
            // we the set the start cycle index so when the button initializes it will be on the good cycle !
        }
    }


    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        yield return base.enable_coroutine();

        // Debug.Log($"[UI_CapablePlacerPool] enabling, we also enable worldplacer");
        // we enable worldplacer
        WorldPlacer.LazyInstance.Enable();
    }
    protected override IEnumerator disable_coroutine()
    {
        // Debug.Log($"[UI_CapablePlacerPool] disabling, we also disable worldplacer");
        
        // we disable worldplacer
        WorldPlacer.LazyInstance.Disable();

        yield return base.disable_coroutine();
    }
}