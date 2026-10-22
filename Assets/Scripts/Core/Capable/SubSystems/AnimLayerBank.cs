using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimLayerBank : MonoBehaviour
{

    // ANIM PLAYER POOLING
    [Header("AnimLayer pooling")]
    [SerializeField] protected AnimLayer anim_layer_prefab;
    [SerializeField] protected Stack<AnimLayer> pooled_anim_layers;


    [Header("Logs")]
    public bool log_anim_player = false;
    public bool log_anim_layers = false;


    ///
    //
    ///  AWAKE & CLEAR CACHE
    //
    ///
    private void Awake()
    {
        // we initialize the pool of anim layers with the prefab one
        pooled_anim_layers = new Stack<AnimLayer>();
    }
    public void ClearCache(bool log)
    {
        // we clear all the pools
        pooled_anim_layers.Clear();

        if (log) { Debug.Log($"(AnimLayerBank) Cache cleared"); }
    }


    ///
    //
    ///  LOAD / UNLOAD  &  INSERT/EXTRACT
    //
    ///

    // INSERT/EXTRACT IN/OUT OF POOL
    private AnimLayer extractAnimLayerFromPool(Transform layer_parent)
    {
        // we first try to extract an anim layer from the pool
        AnimLayer anim_layer = null;
        if (pooled_anim_layers != null && pooled_anim_layers.Count > 0)
        {
            anim_layer = pooled_anim_layers.Pop();
            anim_layer.gameObject.SetActive(true);
            anim_layer.transform.SetParent(layer_parent);
        }
        else
        {
            // if we have no pooled anim layer we need to instantiate one
            anim_layer = Instantiate(anim_layer_prefab, layer_parent);
        }
        return anim_layer;
    }
    private void insertAnimLayerInPool(AnimLayer anim_layer)
    {
        anim_layer.gameObject.SetActive(false);
        anim_layer.transform.SetParent(transform);
        pooled_anim_layers.Push(anim_layer);
    }


    // LOAD / UNLOAD
    /// <summary>
    /// Feed the AnimData into the AnimPlayer.
    /// Ensure we exactly have the required AnimLayers + Load their data.
    /// AND THEN also Load the data of the main AnimPlayer
    /// </summary>
    /// <param name="player">The main AnimPlayer that will receive the data</param>
    /// <param name="anim_data">The AnimPlayerData to feed. This also includes a List<AnimLayerData> for the layers</param>
    public void LoadAnimData(AnimPlayer player, AnimPlayerData anim_data)
    {
        if (player == null) { return; }
        if (anim_data == null || anim_data.skin == "empty" || string.IsNullOrEmpty(anim_data.skin))
        {
            if (log_anim_player) { Debug.Log($"(AnimLayerBank) No anim data (or skin) to load for capable {player.Capable.ID}, destroying player"); }
            Destroy(player.gameObject);
            return;
        }

        // we get the layers parent
        Transform layer_parent = player.transform;

        // check that we do have some layers / layer_parent
        if (layer_parent == null) { return; }

        // remove existing anim layers
        List<AnimLayer> anim_layers = player.GetStaticAnimLayers();
        if (log_anim_layers) { Debug.Log($"(AnimLayerBank) Removing layers of {player.Capable?.ID ?? player.Skin}, unloading {anim_layers.Count} anim layers" + string.Join("\n - ", anim_layers.Select(al => al.name))); }
        while (anim_layers.Count > 0)
        {
            anim_layers[0].UnassignLeader();
            insertAnimLayerInPool(anim_layers[0]);
            anim_layers.RemoveAt(0);
        }
        if (log_anim_layers) { Debug.Log($"(AnimLayerBank) Loading anim data for {anim_data.skin}, loading {anim_data.layers.Count} anim layers"); }

        // we go through all the layers inside anim_data and we load a layer for each
        if (anim_data.layers != null)
        {
            for (int i = 0; i < anim_data.layers.Count; i++)
            {
                // we extract an anim layer from pooled ones
                AnimLayer anim_layer = extractAnimLayerFromPool(layer_parent);

                // then we load the data in the anim layer
                AnimLayerData layer_data = anim_data.layers[i];
                anim_layer.LoadData(layer_data);
                anim_layer.AssignLeader(player);
            }
        }

        // we load the main anim data in the player
        if (log_anim_player) { player.log = true; }
        player.LoadPlayerData(anim_data); // will play the last anim by default
    }
    public void UnloadAnimData(AnimPlayer player)
    {
        if (player == null) { return; }

        // unload anim layers
        List<AnimLayer> anim_layers = player.GetStaticAnimLayers(); // test with .layers
        if (log_anim_layers) { Debug.Log($"(AnimLayerBank) Unloading layers of {player.Capable?.ID ?? player.Skin}, unloading {anim_layers.Count} anim layers" + string.Join("\n - ", anim_layers.Select(al => al.name))); }

        while (anim_layers.Count > 0)
        {
            anim_layers[0].UnassignLeader();
            insertAnimLayerInPool(anim_layers[0]);
            anim_layers.RemoveAt(0);
        }
    }
}