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
    [SerializeField] private GameObject capable_visu_prefab; // an ui image basically
    private HashSet<Image> capable_renderers = new();

    [Header("Sprites")]
    [SerializeField] private List<CapableSpriteIcon> capable_sprites;

    // START
    public void CreateVisuals(RectTransform mapRoot, Vector2 globalOffset)
    {
        // grab all the capable data in the CapableSystem and build a visual for each one
        // todo should take only capables in the current level
        List<CapableData> capables = CapableSystem.Instance.world_capables_data.Values.ToList();
        foreach (CapableData cap in capables)
        {
            create_visu_for_capable(mapRoot, cap, globalOffset);
        }
    }

    // VISU CREATION
    private void create_visu_for_capable(RectTransform mapRoot, CapableData cdata, Vector2 globalOffset)
    {
        // 1. instanciate a visu
        GameObject go = Instantiate(capable_visu_prefab, transform);
        go.name = cdata.id;


        // 2. set the sprite based on the capable kind
        Image sr = go.GetComponent<Image>();
        CapableSpriteIcon icon = capable_sprites.Find(s => s.kind == cdata.kind);
        if (icon == null) { icon = get_best_matching_icon(cdata); }
        sr.sprite = icon.sprite;

        // 3. set the position
        Vector2 world_position = cdata.position;
        Vector2 canvas_position = UI_Manager.WorldToCanvasLocal(world_position, mapRoot, sr.canvas, Camera.main);
        sr.rectTransform.anchoredPosition = canvas_position + globalOffset;


        // 4. add the renderer to our hashset
        capable_renderers.Add(sr);
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
}


[Serializable] public class CapableSpriteIcon
{
    public string kind;
    public List<string> skins;
    public Sprite sprite;
}