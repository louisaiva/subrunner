using System.Collections.Generic;
using UnityEngine;

public class ChunkBank : MonoBehaviour
{

    // AWAKE & SINGLETON LOGIC
    public static ChunkBank Instance { get; private set; }
    public void Awake()
    {
        // singleton logic
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        // initialize the pool of rooms
        pooled_chunks = new Stack<Chunk>();
    }

    // CHUNK LOADING
    [Header("Loaded chunks")]
    [SerializeField] protected List<Chunk> loaded_chunks;
    [SerializeField] protected Chunk chunk_prefab; // rename to chunk_prefab
    private Transform chunk_parent => World.LazyInstance.ChunkParent;

    [Header("Sleeping chunks")]
    [SerializeField] protected Stack<Chunk> pooled_chunks;

    // LOAD UNLOAD CHUNKS
    public Chunk Load(ChunkData data)
    {
        // if we have no pooled room we need to instantiate one
        if (pooled_chunks.Count == 0)
        {
            Chunk new_chunk = Instantiate(chunk_prefab, chunk_parent);
            new_chunk.LoadData(data);
            loaded_chunks.Add(new_chunk);
            return new_chunk;
        }

        // extract a room from the pooled ones and load its data
        Chunk chunk = pooled_chunks.Pop();
        chunk.LoadData(data);
        chunk.enabled = true;
        loaded_chunks.Add(chunk);
        return chunk;
    }
    public void Unload(ChunkData data)
    {
        // get room
        Chunk chunk = GetLoadedChunk(data);
        if (chunk == null) { return; }
        Unload(chunk);
    }
    public void Unload(Chunk chunk)
    {
        // unload the room's data and put it back in the pool
        chunk.UnloadData();
        pooled_chunks.Push(chunk);

        // remove the room from the loaded rooms list
        loaded_chunks.Remove(chunk);

        // disable room component
        chunk.enabled = false;
    }


    // DESTROY CHUNKS
    public void DestroyAllChunksInstantly()
    {
        destroy_all_loaded_chunks();
        destroy_all_pooled_chunks();
    }
    private void destroy_all_loaded_chunks()
    {
        while (loaded_chunks.Count > 0)
        {
            Chunk chunk = loaded_chunks[0];
            if (!Application.isPlaying) { DestroyImmediate(chunk.gameObject); }
            else { Destroy(chunk.gameObject); }
            loaded_chunks.RemoveAt(0);
        }
        loaded_chunks.Clear();
    }
    private void destroy_all_pooled_chunks()
    {
        while (pooled_chunks.Count > 0)
        {
            Chunk chunk = pooled_chunks.Pop();
            if (!Application.isPlaying) { DestroyImmediate(chunk.gameObject); }
            else { Destroy(chunk.gameObject); }
        }
        pooled_chunks.Clear();
    }


    // CHUNK GETTING
    public Chunk GetLoadedChunk(string id)
    {
        // we look for the room with the given id in the pool of loaded rooms
        for (int i = 0; i < loaded_chunks.Count; i++)
        {
            Chunk chunk = loaded_chunks[i];
            if (chunk.data != null && chunk.data.id == id)
            {
                return chunk;
            }
        }
        return null;
    }
    public Chunk GetLoadedChunk(ChunkData data)
    {
        return GetLoadedChunk(data.id);
    }
    public List<Chunk> GetLoadedChunks(List<ChunkData> data)
    {
        List<Chunk> chunks = new List<Chunk>();
        foreach (ChunkData d in data)
        {
            Chunk chunk = GetLoadedChunk(d);
            if (chunk != null) { chunks.Add(chunk); }
        }
        return chunks;
    }
    public List<Chunk> GetAllLoadedChunks()
    {
        return new List<Chunk>(loaded_chunks);
    }
    public bool IsChunkLoaded(ChunkData data)
    {
        for (int i = 0; i < loaded_chunks.Count; i++)
        {
            Chunk chunk = loaded_chunks[i];
            if (chunk.data != null && chunk.data == data) { return true; }
        }
        return false;
    }
}