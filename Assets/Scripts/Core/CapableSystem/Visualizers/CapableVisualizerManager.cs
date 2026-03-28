using System;
using System.Collections.Generic;
using UnityEngine;

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
    public void ClearVisuals()
    {
        foreach (UI_CapableVisualizer visu in capable_renderers) { if (visu != null) { Destroy(visu.gameObject); } }
        capable_renderers.Clear();
        outsider_capables.Clear();
    }
    public void CreateVisuals(string level_id = null)
    {
        // if level id is null, we get the current level id from the LevelEngine
        if (level_id == null) { level_id = LevelEngine.Instance.CurrentLevelID; }
        if (level_id == null) { Debug.LogWarning($"(CapableVisualizerManager) Can't find the current level ID"); return; }

        // grab all the capable data in the CapableSystem and build a visual for each one
        // List<CapableData> insiders = CapableSystem.Instance.GetInsidersWorldCapablesData();
        List<CapableData> cdata = LevelEngine.Instance.GetCapablesDataOfLevel(level_id);
        foreach (CapableData cap in cdata)
        {
            // we create a visu for this capable
            create_visu_for_capable(cap);

            // we check if insider or not
            if (CapableSystem.Instance.TryGetOutsider(cap.id, out Capable outsider))
            {
                outsider_capables.Add(outsider);
            }
        }

        // we also create visuals for enabled outsiders, and we keep track of them so we can update their position manually later
        Dictionary<CapableData, Capable> outsiders = CapableSystem.Instance.GetOutsidersWorldCapablesData();
        foreach (KeyValuePair<CapableData, Capable> entry in outsiders)
        {
            // check that we don't already have a visu for this capable, just in case
            if (cdata.Contains(entry.Key)) { continue; }
            create_visu_for_capable(entry.Key);
            outsider_capables.Add(entry.Value);
        }

        // finally we register to CapableSystem events to create/destroy visuals when needed
        CapableSystem.Instance.OnCapableSpawned += handle_capable_spawned;
        CapableSystem.Instance.OnCapableDespawned += handle_capable_despawned;
    }
    
    // ON DESTROY
    private void OnDestroy()
    {
        if (CapableSystem.Instance == null) { return; }
        CapableSystem.Instance.OnCapableSpawned -= handle_capable_spawned;
        CapableSystem.Instance.OnCapableDespawned -= handle_capable_despawned;
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
        ResizeIcon(visu);

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

    // EVENT HANDLERS
    private void handle_capable_spawned(CapableData data) { create_visu_for_capable(data); }
    private void handle_capable_despawned(CapableData data)
    {
        // check if we have a visu for this capable
        foreach (UI_CapableVisualizer visu in capable_renderers)
        {
            if (visu.capable_data != data) { continue; }

            // if we found the visu, we destroy it and remove it from the hashset
            Destroy(visu.gameObject);
            capable_renderers.Remove(visu);
            break;
        }
    }

    // ON ENABLE
    private void OnEnable() { manually_update_outsiders_position(); }

    // MANUALLY UPDATE POSITION OF OUTSIDER CAPABLES
    private float time_since_last_outsiders_update = 0f;
    private float outsiders_update_interval = 0.25f;
    private void LateUpdate()
    {
        time_since_last_outsiders_update += Time.deltaTime;
        if (time_since_last_outsiders_update < outsiders_update_interval) { return; }

        manually_update_outsiders_position();
    }
    private void manually_update_outsiders_position()
    {
        time_since_last_outsiders_update = 0f;

        // manually update the position of outsiders, will call the events that will update the visu
        foreach (Capable capable in outsider_capables)
        {
            capable.data.SetPosition(capable.transform.position);
        }
    }


    // RESIZE ICONS
    private float min_icon_scale = 0.5f;
    private float max_icon_scale = 4f;
    public void ResizeAllIcons()
    {
        float percentage_zoom = (UI_DevMap.global_zoom - UI_DevMap.MinZoom) / (UI_DevMap.MaxZoom - UI_DevMap.MinZoom);

        // when percentage_zoom is 0, we want the max_icon_scale so the icons are bigs and visible
        // when percentage_zoom is 1, we want the min_icon_scale so the icons are small to avoid cluttering the map
        float icon_scale = Mathf.Lerp(max_icon_scale, min_icon_scale, percentage_zoom);

        foreach (UI_CapableVisualizer visu in capable_renderers)
        {
            visu.GetComponent<RectTransform>().localScale = Vector3.one * icon_scale;
        }
    }
    private void ResizeIcon(UI_CapableVisualizer visu)
    {
        float percentage_zoom = (UI_DevMap.global_zoom - UI_DevMap.MinZoom) / (UI_DevMap.MaxZoom - UI_DevMap.MinZoom);
        float icon_scale = Mathf.Lerp(max_icon_scale, min_icon_scale, percentage_zoom);
        visu.GetComponent<RectTransform>().localScale = Vector3.one * icon_scale;
    }
}


[Serializable] public class CapableSpriteIcon
{
    public string kind;
    public List<string> skins;
    public Sprite sprite;
}