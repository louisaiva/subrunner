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
    public Vulnerable Target => connection != null ? connection.target : null;

    [Header("Opened Connections")]
    public List<Connection> connections = new List<Connection>();

    [Header("Connection Parameters")]
    public float Radius = 0.5f;

    [Header("Debug")]
    public bool log_connection = false;

    // CONNECT
    public void Connect(Vulnerable target, HackCapacity scanner = null)
    {
        // checks if we already have a connection to it we don't open a new one
        if (IsConnectedTo(target))
        {
            connection = get_connection(target);
            if (log_connection) { Debug.LogWarning($"(ConnectCapacity) {capable.name} is already connected to {target.name}"); }
            return;
        }

        // checks if target is unlocked already (can't connect)
        if (target is Lockable lockable && !lockable.Locked)
        {
            if (log_connection) { Debug.LogWarning($"(ConnectCapacity) {capable.name} tried to connect to {target.name} but it is already unlocked."); }
            return;
        }

        // we connect to the target
        connection = new Connection(target);
        connections.Add(connection);

        if (!is_in_range(target))
        {
            if (debug) { Debug.LogWarning($"(ConnectCapacity) {capable.name} try to connect to {target.name} but is out of range."); }
            connection.Close();
        }
        else if (debug) { Debug.LogWarning($"(ConnectCapacity) {capable.name} connected to {target.name}."); }

        // and now we scan the target
        // HackCapacity hacker = capable.GetCapacity<HackCapacity>();
        if (scanner == null)
        {
            if (debug) { Debug.LogWarning($"(ConnectCapacity) {capable.name} tried to scan {target.name} but no hack capacity is available."); }
            return;
        }

        if (log_connection) { Debug.Log($"(ConnectCapacity) {capable.name} launching scan on {target.name}."); }
        scanner.Scan(target);
    }
    public void Disconnect()
    {
        if (connection == null) { return; }
        if (debug) { Debug.LogWarning($"(ConnectCapacity) {capable.name} disconnected current connection."); }
        if (connection.state != ConnectionState.Opened) { connections.Remove(connection); }
        connection = null;
    }
    public bool IsConnectedTo(Vulnerable target)
    {
        if (Target != null && Target == target)
        {
            return connection.state == ConnectionState.Connected || connection.state == ConnectionState.Opened;
        }
        return connections.Any(c => c.target == target && (c.state == ConnectionState.Connected || c.state == ConnectionState.Opened));
    }
    private Connection get_connection(Vulnerable target)
    {
        return connections.FirstOrDefault(c => c.target == target);
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
    private bool is_in_range(Vulnerable target)
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

[Serializable] public class Connection
{
    public ConnectionType type = ConnectionType.None;
    public ConnectionState state = ConnectionState.None;
    public Vulnerable target;

    public Connection(Vulnerable target, ConnectionType type = ConnectionType.None)
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