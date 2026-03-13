using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a ConnectionTree is associated to a HackCapacity
/// and handles every connection the hacker currently has
/// under a tree form in order to fix some bugs
/// when a connection is lost in the middle but we can still
/// hack things from far away
/// </summary>
public class ConnectionTree : MonoBehaviour
{
    [Header("Root & Nodes")]
    public ConnectCapacity root;
    public List<Connection> nodes = new List<Connection>();

    [Header("Parent caches")]
    private Dictionary<Connection, Connection> parent_cache = new Dictionary<Connection, Connection>();

    [Header("Logs")]
    public bool log = false;

    // ADD / REMOVE
    public void AddNode(Connection connection)
    {
        if (log) { Debug.Log($"(ConnectionTree) Adding node from {connection.start.Capable.name} to {connection.destination.Capable.name}"); }

        // we find the connection's parent to add their child
        Connection parent = get_parent(connection);
        if (parent != null)
        {
            // we add the child to the parent
            parent.AddChild(connection);
        }

        // add the node to the graph
        nodes.Add(connection);
    }
    public void RemoveNode(Connection node)
    {
        if (log) { Debug.Log($"(ConnectionTree) Removing node from {node.start.Capable.name} to {node.destination.Capable.name}"); }

        // we remove the node from the graph
        nodes.Remove(node);

        // we remove the node from its parent's children list
        Connection parent = get_parent(node);
        if (parent != null)
        {
            parent.RemoveChild(node);
            if (parent_cache.ContainsKey(node)) { parent_cache.Remove(node); }
        }
    }

    // GET PARENT
    private Connection get_parent(Connection node)
    {
        if (node.start == root) { return null; }

        // we check in cache first
        if (parent_cache.ContainsKey(node)) { return parent_cache[node]; }

        // we search for the parent
        foreach (Connection parent in nodes)
        {
            // only takes nodes where destination is the start of our
            if (parent.destination == node.start)
            {
                parent_cache[node] = parent;
                return parent;
            }
        }
        return null;
    }
}