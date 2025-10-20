using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// this class handles multiple descriptions FOR FILEs
/// and allow the exploit wheel to show exploit description
/// with core cost & help for example
/// </summary>
public class FileDescriptor : MonoBehaviour
{
    [Header("Descriptions")]
    public Description name_desc;
    public Description data_desc;

    [Header("Program Details")]
    public Description core_cost_desc;
    public Description base_duration_desc;
    public Color core_cost_base_color = new Color(0.5f, 1f, 0.5f, 1f);
    public Color not_enough_cores_color = new Color(1f, 0.5f, 0.5f, 1f);

    [Header("Exploit Details")]
    public Description secu_level_desc;
    public Description targets_desc;

    [Header("Logs")]
    public bool log = false;

    // DESCRIPTION
    public void SetDescription(File file)
    {
        // we set the name & data description
        name_desc.SetDescription(file == null ? "empty file" : file.name + file.extension);
        data_desc.SetDescription(file == null ? "" : file.data);

        // set colors
        set_colors();

        // we set the core cost description
        if (file == null || file is not Program program)
        {
            core_cost_desc.gameObject.SetActive(false);
            base_duration_desc.gameObject.SetActive(false);
            secu_level_desc.gameObject.SetActive(false);
            targets_desc.gameObject.SetActive(false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            return;
        }

        core_cost_desc.gameObject.SetActive(true);
        core_cost_desc.SetDescription($"required cores : {program.cores_cost}");

        // we check if the current perso.instance.device has enough free cores for this program
        if (Perso.Instance.Device?.Processor.FreeCoresCount < program.cores_cost) { core_cost_desc.SetColor(not_enough_cores_color); }

        base_duration_desc.gameObject.SetActive(true);
        base_duration_desc.SetDescription($"base duration: {program.base_duration.ToString("F1")} s");


        // checks if file is not an exploit
        if (file is not Exploit exploit)
        {
            secu_level_desc.gameObject.SetActive(false);
            targets_desc.gameObject.SetActive(false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            return;
        }

        // we set the exploit details
        secu_level_desc.gameObject.SetActive(true);
        secu_level_desc.SetDescription($"security level: {exploit.security_level}");

        targets_desc.gameObject.SetActive(true);
        targets_desc.SetDescription($"targets: {exploit.targets}");
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    private void set_colors()
    {
        core_cost_desc.SetColor(core_cost_base_color);
        base_duration_desc.SetColor(core_cost_base_color);
        secu_level_desc.SetColor(core_cost_base_color);
        targets_desc.SetColor(core_cost_base_color);
    }
}