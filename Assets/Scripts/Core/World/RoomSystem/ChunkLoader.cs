using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[Serializable] public class ChunkLoader
{
    private HashSet<ChunkData> loaded_chunks = new HashSet<ChunkData>(); // all the chunks that are currently loaded
    private HashSet<string> loaded_chunks_ids = new HashSet<string>();
    public string[] LoadedChunkIDs { get => loaded_chunks_ids.ToArray(); }

    private HashSet<string> chunks_waiting_for_load = new HashSet<string>(); // only real ones that need loading
    private HashSet<string> chunks_waiting_for_unload = new HashSet<string>(); // only those that need unloading
    [SerializeField] private List<ChunkLoadTask> load_tasks = new List<ChunkLoadTask>();
    [SerializeField] private List<ChunkUnloadTask> unload_tasks = new List<ChunkUnloadTask>();

    // LOGS
    [Header("Logs")]
    [SerializeField] private bool log_ask = false;
    [SerializeField] private bool log_loading = false;
    [SerializeField] private bool log_run = false;


    // MAIN ENTRY POINTS
    public ChunkLoadTask AskLoadChunks(string[] chunks_ids)
    {
        if (chunks_ids.Length == 0) { return null; }
        ChunkLoadTask task = new ChunkLoadTask(chunks_ids);
        load_tasks.Add(task);

        if (log_ask) { Debug.Log("(ChunkLoader) Asked to load chunks: " + string.Join(", ", chunks_ids)); }

        // remove the chunks that are waiting for unload and add the ones that are not already waiting for load
        foreach (string chunk_id in chunks_ids)
        {
            chunks_waiting_for_unload.Remove(chunk_id);
            chunks_waiting_for_load.Add(chunk_id);
        }

        return task;
    }
    public ChunkUnloadTask AskUnloadChunks(string[] chunks_ids)
    {
        if (chunks_ids.Length == 0) { return null; }
        ChunkUnloadTask task = new ChunkUnloadTask(chunks_ids);
        unload_tasks.Add(task);

        if (log_ask) { Debug.Log("(ChunkLoader) Asked to unload chunks: " + string.Join(", ", chunks_ids)); }

        // remove the chunks that are waiting for load and add the ones that are not already waiting for unload
        foreach (string chunk_id in chunks_ids)
        {
            chunks_waiting_for_load.Remove(chunk_id);
            chunks_waiting_for_unload.Add(chunk_id);
        }

        return task;
    }


    // TASKS RUNNING
    public void RunTask()
    {
        if (log_run) { Debug.Log("(ChunkLoader) Running tasks. (" + load_tasks.Count + " loading, " + unload_tasks.Count + " unloading)"); }

        // we do ONE loading task & ONE unloading task
        ChunkLoadTask load_task = get_first_waiting_loading_task();
        // here if we have no waiting loading task we could handle a delayed one
        if (load_task != null) { load_task.Run(this); }

        ChunkUnloadTask unload_task = get_first_waiting_unloading_task();
        // here if we have no waiting unloading task we could handle a delayed one
        if (unload_task != null) { unload_task.Run(this); }


        // we remove the done tasks
        // todo when we will wait for capable loading we can wait for .IsCapableDone or something like this
        load_tasks.RemoveAll(t => t.IsDone);
        unload_tasks.RemoveAll(t => t.IsDone);
    }
    private ChunkLoadTask get_first_waiting_loading_task()
    {
        for (int i = 0; i < load_tasks.Count; i++)
        {
            if (load_tasks[i].State == ChunkLoaderTaskState.Waiting) { return load_tasks[i]; }
        }
        return null;
    }
    private ChunkUnloadTask get_first_waiting_unloading_task()
    {
        for (int i = 0; i < unload_tasks.Count; i++)
        {
            if (unload_tasks[i].State == ChunkLoaderTaskState.Waiting) { return unload_tasks[i]; }
        }
        return null;
    }


    // CHUNK LOADING / UNLOADING
    public bool LoadChunk(string chunk_id /*, out CapableLoadTask task*/) // for later
    {
        if (loaded_chunks_ids.Contains(chunk_id)) { return true; }

        // we get the chunk data
        ChunkData data = ChunkEngine.Instance.GetChunkDataFromID(chunk_id);
        if (data == null) { return false; }

        // load the chunk from bank
        ChunkBank.Instance.Load(data);
        loaded_chunks.Add(data);
        loaded_chunks_ids.Add(chunk_id);
        if (log_loading) { Debug.Log("(ChunkLoader) Loaded " + chunk_id); }

        // hide the room (and so the just loaded chunk's capables) if the room is not visible, otherwise show it
        if (!RoomEngine.Instance.DoorEngine.IsRoomVisible(data.room_id)) { RoomEngine.Instance.DoorEngine.HideRoom(data.room_id); }
        else { RoomEngine.Instance.DoorEngine.ShowRoom(data.room_id); }
        return true;
    }
    public bool UnloadChunk(string chunk_id /*, out CapableUnloadTask task*/) // for later
    {
        if (!loaded_chunks_ids.Contains(chunk_id)) { return true; }

        // we get the chunk data
        ChunkData data = ChunkEngine.Instance.GetChunkDataFromID(chunk_id);
        if (data == null) { return false; }

        // unload the chunk from bank
        ChunkBank.Instance.Unload(data);
        loaded_chunks.Remove(data);
        loaded_chunks_ids.Remove(chunk_id);
        if (log_loading) { Debug.Log("(ChunkLoader) Unloaded " + chunk_id); }

        // no need to check door engine
        return true;
    }



    // GETTERS
    public bool IsChunkLoaded(ChunkData data) { return loaded_chunks.Contains(data); }
    public bool IsChunkLoaded(string chunk_id) { return loaded_chunks_ids.Contains(chunk_id); }
    public bool WillChunkBeLoaded(string chunk_id) { return chunks_waiting_for_load.Contains(chunk_id) || (loaded_chunks_ids.Contains(chunk_id) && !chunks_waiting_for_unload.Contains(chunk_id)); }
    public bool WillChunkBeUnloaded(string chunk_id) { return chunks_waiting_for_unload.Contains(chunk_id) || (!loaded_chunks_ids.Contains(chunk_id) && !chunks_waiting_for_load.Contains(chunk_id)); }
    public bool is_chunk_waiting_for_load(string chunk_id) { return chunks_waiting_for_load.Contains(chunk_id); }
    public bool is_chunk_waiting_for_unload(string chunk_id) { return chunks_waiting_for_unload.Contains(chunk_id); }


    // CLEAR
    public void Clear()
    {
        loaded_chunks.Clear();
        loaded_chunks_ids.Clear();
        chunks_waiting_for_load.Clear();
        chunks_waiting_for_unload.Clear();
        foreach (ChunkLoadTask task in load_tasks) { task.Clear(); }
        foreach (ChunkUnloadTask task in unload_tasks) { task.Clear(); }
        load_tasks.Clear();
        unload_tasks.Clear();
    }
}

[Serializable] public class ChunkLoadTask
{
    [SerializeField] private ChunkLoaderTaskState state = ChunkLoaderTaskState.Waiting;
    public ChunkLoaderTaskState State { get => state; }
    public bool IsDone { get => state == ChunkLoaderTaskState.Done; }
    [SerializeField] private List<string> chunks_ids_to_load = new List<string>();
    public System.Action<ChunkLoadTask> OnDone;

    public ChunkLoadTask(string[] chunks_ids)
    {
        chunks_ids_to_load.AddRange(chunks_ids);
    }

    public void Run(ChunkLoader loader)
    {
        state = ChunkLoaderTaskState.Running;

        // we get the intersection of the chunks to load with the ones that are still waiting for load
        for (int i = chunks_ids_to_load.Count - 1; i >= 0; i--)
        {
            string chunk_id = chunks_ids_to_load[i];
            if (!loader.is_chunk_waiting_for_load(chunk_id)) { chunks_ids_to_load.RemoveAt(i); continue; } // no need to load it after all, maybe it went back to unload waiting
            if (!loader.LoadChunk(chunk_id)) { continue; }

            // we successfully loaded the chunk, we remove it from our list
            chunks_ids_to_load.RemoveAt(i);
        }

        // if we have no more chunk to load, we are done
        if (chunks_ids_to_load.Count == 0) { Done(); }
        else { state = ChunkLoaderTaskState.Delayed; }
    }
    public void Done()
    {
        state = ChunkLoaderTaskState.Done;
        OnDone?.Invoke(this);
    }

    public void Clear()
    {
        chunks_ids_to_load.Clear();
        state = ChunkLoaderTaskState.Waiting;
    }
}
[Serializable] public class ChunkUnloadTask
{
    [SerializeField] private ChunkLoaderTaskState state = ChunkLoaderTaskState.Waiting;
    public ChunkLoaderTaskState State { get => state; }
    public bool IsDone { get => state == ChunkLoaderTaskState.Done; }
    [SerializeField] private List<string> chunks_ids_to_unload = new List<string>();
    public System.Action<ChunkUnloadTask> OnDone;

    public ChunkUnloadTask(string[] chunks_ids)
    {
        chunks_ids_to_unload.AddRange(chunks_ids);
    }

    public void Run(ChunkLoader loader)
    {
        state = ChunkLoaderTaskState.Running;

        // we get the intersection of the chunks to unload with the ones that are still waiting for unload
        for (int i = chunks_ids_to_unload.Count - 1; i >= 0; i--)
        {
            string chunk_id = chunks_ids_to_unload[i];
            if (!loader.is_chunk_waiting_for_unload(chunk_id)) { chunks_ids_to_unload.RemoveAt(i); continue; } // no need to unload it after all, maybe it went back to load waiting
            if (!loader.UnloadChunk(chunk_id)) { continue; }

            // we successfully unloaded the chunk, we remove it from our list
            chunks_ids_to_unload.RemoveAt(i);
        }

        // if we have no more chunk to unload, we are done
        if (chunks_ids_to_unload.Count == 0) { Done(); }
        else { state = ChunkLoaderTaskState.Delayed; }
    }
    public void Done()
    {
        state = ChunkLoaderTaskState.Done;
        OnDone?.Invoke(this);
    }
    public void Clear()
    {
        chunks_ids_to_unload.Clear();
        state = ChunkLoaderTaskState.Waiting;
    }
}
public enum ChunkLoaderTaskState
{
    Waiting,
    Running,
    Delayed,
    Done
}