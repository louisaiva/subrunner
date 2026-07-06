using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// this class handles multiple descriptions FOR FILEs
/// and allow the exploit wheel to show exploit description
/// with core cost & help for example
/// </summary>
public class UI_FileDescriptor : UI_Descriptor
{
    [Header("Program Details")]
    public UI_Writer core_cost_desc;
    public UI_Writer base_duration_desc;
    public Color core_cost_base_color = new Color(0.5f, 1f, 0.5f, 1f);
    public Color not_enough_cores_color = new Color(1f, 0.5f, 0.5f, 1f);

    [Header("Exploit Details")]
    public UI_Writer secu_level_desc;
    public UI_Writer targets_desc;

    // DESCRIPTION
    public override void Describe(Descriptable descriptable)
    {
        base.Describe(descriptable);
        if (descriptable == null || descriptable is not Program program)
        {
            core_cost_desc.gameObject.SetActive(false);
            base_duration_desc.gameObject.SetActive(false);
            secu_level_desc.gameObject.SetActive(false);
            targets_desc.gameObject.SetActive(false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            return;
        }

        // set colors
        set_colors();

        core_cost_desc.gameObject.SetActive(true);
        core_cost_desc.Write($"required cores : {program.cores_cost}");

        // we check if the current Controller.Perso.device has enough free cores for this program
        if (Controller.Perso != null && Controller.Perso.Device != null)
        {
            if (Controller.Perso.Device.Processor.FreeCoresCount < program.cores_cost) { core_cost_desc.SetColor(not_enough_cores_color); }
        }

        base_duration_desc.gameObject.SetActive(true);
        base_duration_desc.Write($"base duration: {program.base_duration.ToString("F1")} s");

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        return;

        /* // checks if file is not an exploit
        if (program is not Exploit exploit)
        {
            // secu_level_desc.gameObject.SetActive(false);
            // targets_desc.gameObject.SetActive(false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            return;
        }

        // we set the exploit details
        // secu_level_desc.gameObject.SetActive(true);
        // secu_level_desc.Write($"security level: {exploit.security_level}");

        // targets_desc.gameObject.SetActive(true);
        // targets_desc.Write($"targets: {exploit.targets}");
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>()); */
    }
    private void set_colors()
    {
        core_cost_desc.SetColor(core_cost_base_color);
        base_duration_desc.SetColor(core_cost_base_color);
        secu_level_desc.SetColor(core_cost_base_color);
        targets_desc.SetColor(core_cost_base_color);
    }
}