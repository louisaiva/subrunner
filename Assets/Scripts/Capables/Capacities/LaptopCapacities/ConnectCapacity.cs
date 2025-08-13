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
    public Hackable Target => connection != null ? connection.target : null;

    [Header("Opened Connections")]
    public List<Connection> connections = new List<Connection>();

    [Header("Connection Parameters")]
    public float Radius = 0.5f;


    [Header("Components")]
    private Laptop laptop;
    private HackCapacity hacker;


    // START
    private void Start()
    {
        laptop = capable.GetComponent<Laptop>();
        hacker = capable.GetCapacity<HackCapacity>();
    }

    // CONNECT
    public void Connect(Hackable target)
    {
        // checks if we already have a connection to it we don't open a new one
        if (IsConnectedTo(target)) { return; }

        // checks if target is unlocked already (can't connect)
        if (target is Lockable lockable && !lockable.Locked) { return; }

        // we connect to the target
        connection = new Connection(target);
        connections.Add(connection);

        if (!is_in_range(target))
        {
            if (debug) { Debug.LogWarning($"(ConnectCapacity) {capable.name} try to connect to {target.name} but is out of range."); }
            connection.Close();
            return;
        }

        if (debug) { Debug.LogWarning($"(ConnectCapacity) {capable.name} connected to {target.name}."); }

        // and now we scan the target
        hacker.Scan(target);
    }
    public void Disconnect()
    {
        if (connection == null) { return; }
        if (debug) { Debug.LogWarning($"(ConnectCapacity) {capable.name} disconnected from {Target.name}."); }
        if (connection.state != ConnectionState.Opened) { connections.Remove(connection); }
        connection = null;
    }
    public bool IsConnectedTo(Hackable target)
    {
        if (Target != null && Target == target)
        {
            return connection.state == ConnectionState.Connected || connection.state == ConnectionState.Opened;
        }
        return connections.Any(c => c.target == target && (c.state == ConnectionState.Connected || c.state == ConnectionState.Opened));
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
            if (!is_in_range(tunnel.target))
            {
                if (debug) { Debug.LogWarning($"(ConnectCapacity) {capable.name} closing connection to {tunnel.target.name} because it is out of range."); }
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

    private bool is_in_range(Hackable target)
    {
        if (target == null) { return false; }
        return Vector3.Distance(transform.position, target.transform.position) <= Radius;
    }
}

[Serializable] public class Connection
{
    public ConnectionType type = ConnectionType.None;
    public ConnectionState state = ConnectionState.None;
    public Hackable target;

    public Connection(Hackable target, ConnectionType type = ConnectionType.None)
    {
        this.target = target;
        this.state = ConnectionState.Connected;
        this.type = type;
    }
    public void Open()
    {
        this.state = ConnectionState.Opened;
    }
    public void Close()
    {
        this.state = ConnectionState.Closed;
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