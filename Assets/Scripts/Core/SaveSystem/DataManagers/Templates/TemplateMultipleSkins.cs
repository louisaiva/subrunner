using System.Collections.Generic;
using UnityEngine;

public class TemplateMultipleSkins : MonoBehaviour
{
    public List<string> Skins => GetSkins();

    public List<string> specific_skins = new List<string>();
    public List<BurstSkinTemplate> burst_skin_templates = new List<BurstSkinTemplate>();

    public List<string> GetSkins()
    {
        List<string> skins = new List<string>();

        // we add the specific skins
        foreach (string skin in specific_skins)
        {
            if (string.IsNullOrEmpty(skin)) { continue; }
            if (!skins.Contains(skin)) { skins.Add(skin); }
        }

        // we add the burst skins
        foreach (BurstSkinTemplate burst_skin in burst_skin_templates)
        {
            if (string.IsNullOrEmpty(burst_skin.skin)) { continue; }
            for (int i = 0; i < burst_skin.skin_count; i++)
            {
                string skin = burst_skin.skin + "_" + i;
                if (!skins.Contains(skin)) { skins.Add(skin); }
            }
        }

        return skins;
    }
}

[System.Serializable] public class BurstSkinTemplate
{
    public string skin = "";
    public int skin_count = 0;
}