using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

public class WorldRoomVisualizer : MonoBehaviour
{
    private List<WorldCellVisualizer> cells = new List<WorldCellVisualizer>();
    private List<WorldLinkVisualizer> links = new List<WorldLinkVisualizer>();
    public Color Color
    {
        get { return Color.white; }
        set { }
    }

    // CREATE ROOM
    public void CreateRoom(List<WorldCellVisualizer> cells, List<WorldLinkVisualizer> links)
    {
        // unregister from previous cells if there is any
        unregister_callbacks();
        reset_color();

        this.cells = cells;
        this.links = links;

        // set color of cells and links
        set_color();

        // register to new cells
        register_callbacks();
    }

    // colors
    private void set_color()
    {
        foreach (var c in cells) { c.Color = WorldBuilder.Instance.LinkedColor; }
        foreach (var l in links) { l.Color = WorldBuilder.Instance.LinkedColor; }
    }
    private void reset_color()
    {
        foreach (var c in cells)
        {
            if (c.IsPartOfRoom()) { c.Color = WorldBuilder.Instance.LinkedColor; }
            else { c.Color = WorldBuilder.Instance.WaitingColor; }
        }
        foreach (var l in links)
        {
            if (l.CellA.IsPartOfRoom() && l.CellB.IsPartOfRoom()) { l.Color = WorldBuilder.Instance.LinkedColor; }
            else { l.Color = WorldBuilder.Instance.WaitingColor; }
        }
    }

    // callbacks
    private void register_callback(WorldCellVisualizer cell)
    {
        if (cell == null) { return; }
        cell.OnRemoved += remove_ourself;
    }
    private void register_callbacks()
    {
        foreach (var c in cells) { register_callback(c); }
    }
    private void unregister_callback(WorldCellVisualizer cell)
    {
        if (cell == null) { return; }
        cell.OnRemoved -= remove_ourself;
    }
    private void unregister_callbacks()
    {
        foreach (var c in cells) { unregister_callback(c); }
    }
    private void remove_ourself(WorldCellVisualizer cell_visu)
    {
        going_to_be_destroyed = true;
        reset_color();
        Destroy(gameObject);
    }
    private bool going_to_be_destroyed = false;
    private void OnDestroy()
    {
        unregister_callbacks();
    }

    // GETTERS
    public bool HasCell(WorldCellVisualizer cell)
    {
        if (going_to_be_destroyed) { return false; }
        return cells.Contains(cell);
    }
    public bool IsEqualTo(List<WorldCellVisualizer> cells, List<WorldLinkVisualizer> links)
    {
        return this.cells.Intersect(cells).Count() == this.cells.Count
            && this.links.Intersect(links).Count() == this.links.Count;
    }
    public List<Vector3Int> GetLoopCells()
    {
        return cells.Select(c => c.CurrentCell).ToList();
    }
}