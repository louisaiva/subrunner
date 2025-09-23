using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Pathfinding;
using UnityEngine;

/// <summary>
/// ConnectCapacity is a capacity that allows a being to connect
/// to other hardware things. before hacking anything we need to
/// connect to it, this script handles this part
/// </summary>

public class ConnectCapacity : Capacity
{
    [Header("Connection Target")]
    public Connection connection = null;
    public Vulnerable Target {
        get {
            if (connection == null) { return null; }
            if (connection.destination == null) { return null; }
            return connection.destination.Vulnerable;
        }
    }
    public Vulnerable Vulnerable => GetComponent<Vulnerable>();

    [Header("Opened Connections")]
    public List<Connection> connections = new List<Connection>();

    [Header("Connection Parameters")]
    public float Radius = 0.5f;

    [Header("Connection Tree")]
    public ConnectionTree Tree;

    [Header("Debug")]
    public bool log_connection = false;
    public bool log_connection_details = false;

    // AWAKE
    private void Awake()
    {
        Tree = GetComponent<ConnectionTree>();
        if (Tree != null) { Tree.root = this; }
    }


    // CONNECT
    public void Connect(ConnectCapacity target, HackCapacity scanner = null)
    {
        // checks if we already have a connection to it we don't open a new one
        if (IsConnectedTo(target))
        {
            connection = get_connection(target);
            if (log_connection) { Debug.LogWarning($"(ConnectCapacity) {capable.name} is already connected to {target.capable.name}"); }
            return;
        }

        // checks if target is unlocked already (can't connect)
        if (target.capable is Lockable lockable && !lockable.Locked)
        {
            if (log_connection) { Debug.LogWarning($"(ConnectCapacity) {capable.name} tried to connect to {target.capable.name} but it is already unlocked."); }
            return;
        }

        // we connect to the target
        connection = new Connection(this, target, Controller.Instance.HackableNavigator.Tree);
        connections.Add(connection);

        if (!is_in_range(target))
        {
            if (debug) { Debug.LogWarning($"(ConnectCapacity) {capable.name} try to connect to {target.capable.name} but is out of range."); }
            connection.Close();
        }
        else if (debug) { Debug.LogWarning($"(ConnectCapacity) {capable.name} connected to {target.capable.name}."); }

        // and now we scan the target
        if (scanner == null)
        {
            if (debug) { Debug.LogWarning($"(ConnectCapacity) {capable.name} tried to scan {target.capable.name} but no hack capacity is available."); }
            return;
        }

        if (log_connection) { Debug.Log($"(ConnectCapacity) {capable.name} launching scan on {target.capable.name}."); }
        scanner.Scan(target.Vulnerable);
    }
    public void Disconnect()
    {
        if (connection == null) { return; }
        if (debug) { Debug.LogWarning($"(ConnectCapacity) {capable.name} disconnected current connection."); }
        if (connection.state != ConnectionState.Opened)
        {
            connection.Close();
            connections.Remove(connection);
        }
        connection = null;
    }
    public bool IsConnectedTo(ConnectCapacity target)
    {
        if (Target != null && Target == target)
        {
            return connection.state == ConnectionState.Connected || connection.state == ConnectionState.Opened;
        }
        return connections.Any(c => c.destination == target && (c.state == ConnectionState.Connected || c.state == ConnectionState.Opened));
    }
    private Connection get_connection(ConnectCapacity target)
    {
        return connections.FirstOrDefault(c => c.destination == target);
    }

    // UPDATE
    protected override void Update()
    {
        // on parcourt les connections pour voir si on a des connections à fermer
        for (int i = connections.Count - 1; i >= 0; i--)
        {
            Connection tunnel = connections[i];
            if (tunnel.state == ConnectionState.Closed && tunnel != connection) { connections.RemoveAt(i); }

            // sinon on vérifie si on doit fermer la connection
            if (!is_in_range(tunnel.destination))
            {
                if (debug) { Debug.LogWarning($"(ConnectCapacity) {capable.name} closing connection because it is out of range."); }
                tunnel.Close();
                continue;
            }

            // on vérifie si on est connection et qu'on est à nouveau dans le range on rebascule en connected
            if (tunnel.state == ConnectionState.Closed && tunnel == connection)
            {
                tunnel.state = ConnectionState.Connected;
            }
        }
    }
    private bool is_in_range(ConnectCapacity target)
    {
        if (target == null) { return false; }
        try { return Vector3.Distance(transform.position, target.transform.position) <= Radius; }
        catch { return false; }
    }

    // ON DESTROY
    private void OnDestroy()
    {
        for (int i = connections.Count - 1; i >= 0; --i)
        {
            Connection connection = connections[i];
            connection.Close();
            connections.RemoveAt(i);
        }
    }
}

[Serializable]
public class Connection
{
    [NonSerialized] public ConnectCapacity start;
    [NonSerialized] public ConnectCapacity destination;
    public string to;
    public string from;

    [Header("Connection State")]
    public ConnectionType type = ConnectionType.None;
    public ConnectionState state = ConnectionState.None;

    [Header("Tree Structure")]
    [SerializeReference] public ConnectionTree tree;
    [SerializeReference] public List<Connection> children = new List<Connection>();


    // CONSTRUCTOR
    public Connection(ConnectCapacity start, ConnectCapacity destination, ConnectionTree tree, ConnectionType type = ConnectionType.None)
    {
        this.start = start;
        this.destination = destination;
        this.state = ConnectionState.Connected;
        this.type = type;
        this.tree = tree;

        // we set the names
        this.from = start.capable.name;
        this.to = destination.capable.name;
    }

    // OPEN / CLOSE
    public void Open()
    {
        this.state = ConnectionState.Opened;

        // we add the node to the tree
        tree.AddNode(this);
    }
    public void Close()
    {
        this.state = ConnectionState.Closed;

        // we make the tree delete ourselves
        tree.RemoveNode(this);

        // we close all children too
        while (children.Count > 0) { children[0].Close(); }
    }

    // CHILDREN MANAGEMENT
    public void AddChild(Connection child)
    {
        if (children.Contains(child)) { return; }

        // we add the children
        children.Add(child);
    }
    public void RemoveChild(Connection child)
    {
        if (!children.Contains(child)) { return; }
        children.Remove(child);
    }
}

public enum ConnectionState
{
    None,
    Connected,
    Opened,
    Closed
}

public enum ConnectionType
{
    None,
    Cable,
    Bluetooth,
    Wifi
}