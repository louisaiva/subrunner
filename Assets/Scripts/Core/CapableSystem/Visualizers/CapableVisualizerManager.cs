using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// this class draws icon for each capable in the world, based on their kind and skin.
/// For this we go through all CapableSystem.world_capables_data.
/// </summary>
public class CapableVisualizerManager : MonoBehaviour
{

    [Header("Capable Visu Prefab")]
    [SerializeField] private GameObject capable_visu_prefab;
    private HashSet<UI_CapableVisualizer> capable_renderers = new();
    private HashSet<Capable> outsider_capables = new();

    [Header("Sprites")]
    [SerializeField] private List<CapableSpriteIcon> capable_sprites;

    // START
    public void CreateVisuals()
    {
        // todo should take only capables in the current level

        // grab all the capable data in the CapableSystem and build a visual for each one
        List<CapableData> insiders = CapableSystem.Instance.GetInsidersWorldCapablesData();
        foreach (CapableData cap in insiders) { create_visu_for_capable(cap); }

        // we also create visuals for enabled outsiders, and we keep track of them so we can update their position manually later
        Dictionary<CapableData, Capable> outsiders = CapableSystem.Instance.GetOutsidersWorldCapablesData();
        foreach (KeyValuePair<CapableData, Capable> entry in outsiders)
        {
            create_visu_for_capable(entry.Key);
            outsider_capables.Add(entry.Value);
        }
    }

    // VISU CREATION
    private void create_visu_for_capable(CapableData cdata)
    {
        // 1. instanciate a visu
        GameObject go = Instantiate(capable_visu_prefab, transform);
        go.name = cdata.id;

        // 2. get the sprite icon based on the capable kind
        CapableSpriteIcon icon = capable_sprites.Find(s => s.kind == cdata.kind);
        if (icon == null) { icon = get_best_matching_icon(cdata); }

        // 4. get the UI_CapableVisualizer component and init it with the capable data & the sprite icon
        UI_CapableVisualizer visu = go.GetComponent<UI_CapableVisualizer>();
        visu.Init(cdata, icon);

        // 5. add the renderer to our hashset
        capable_renderers.Add(visu);
    }
    private CapableSpriteIcon get_best_matching_icon(CapableData cdata)
    {
        int best_inheritance_distance = int.MaxValue;
        bool best_skin_match = false;
        CapableSpriteIcon best_icon = null;

        // we go through all the icons and we try to find the icon that matches the kind the best, based on these criteria (in this order):
        // 1. generic kind match (i.e. Chest does not match IA but matches Capable)
        // 2. inheritance distance (i.e. if we have an icon for Chest and an icon for Capable, Chest is better for a Chest kind)
        // 3. skin match (i.e. if we have a Corpse with bones skin, it can match with both no skin & bones skin, but bones skin is better)

        foreach (CapableSpriteIcon icon in capable_sprites)
        {
            bool is_kind_match = GameManager.IsKind(cdata.kind, icon.kind, out int icon_inheritance_distance);
            if (!is_kind_match) { continue; }

            // check if we have an inheritance match that is better than the current best
            if (icon_inheritance_distance > best_inheritance_distance) { continue; }

            // if we have a better inheritance match, we take it even if we don't have a skin match
            if (icon_inheritance_distance < best_inheritance_distance)
            {
                best_inheritance_distance = icon_inheritance_distance;
                best_skin_match = does_icon_skin_match(icon, cdata);
                best_icon = icon;
                continue;
            }

            // if we have the same inheritance distance, we check for a skin match
            if (best_skin_match) { continue; } // if we already have a skin match, we don't care about other icons with the same inheritance distance

            if (does_icon_skin_match(icon, cdata))
            {
                best_skin_match = true;
                best_icon = icon;
            }
        }

        // if we found a matching icon, we return it
        if (best_icon != null) { return best_icon; }

        // otherwise we return the base default icon (which is a ?), which is the 19th one
        return capable_sprites[19];
    }
    private bool does_icon_skin_match(CapableSpriteIcon icon, CapableData cdata)
    {
        // if the icon has no skins, it does never matches
        if (icon.skins == null || icon.skins.Count == 0) { return false; }

        // if the capable has no skin, same
        if (cdata.anim_data == null || string.IsNullOrEmpty(cdata.anim_data.skin)) { return false; }

        // otherwise, we check if we have a skin match
        return icon.skins.Contains(cdata.anim_data.skin);
    }


    // MANUALLY UPDATE POSITION OF OUTSIDER CAPABLES
    private float time_since_last_outsiders_update = 0f;
    private float outsiders_update_interval = 1f;
    private void LateUpdate()
    {
        time_since_last_outsiders_update += Time.deltaTime;
        if (time_since_last_outsiders_update < outsiders_update_interval) { return; }
        time_since_last_outsiders_update = 0f;

        // manually update the position of outsiders, will call the events that will update the visu
        foreach (Capable capable in outsider_capables)
        {
            capable.data.SetPosition(capable.transform.position);
        }
    }
}


[Serializable] public class CapableSpriteIcon
{
    public string kind;
    public List<string> skins;
    public Sprite sprite;
}