using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// laptop's capacity. holds all the cpu_modules' cores in one place
/// allow a computer to run program by using cores
/// </summary>
public class ProcessCapacity : Capacity
{

    [Header("Process parameters")]
    [SerializeField] private List<Core> cores = new List<Core>();
    public int MaxCores => cores.Count;
    public int FreeCoresCount => cores.Count(c => c.isFree);
    public int UsedCoresCount => cores.Count(c => !c.isFree);
    public event System.Action<int> OnCoresFreedOrUsed = delegate { };

    [Header("Running processes")]
    private Dictionary<Processus, List<Core>> running_processes = new Dictionary<Processus, List<Core>>(); // store the nb of cores used per processus

    // CORES MANAGEMENTS
    public bool HasFreeCores(int amount = 1)
    {
        return FreeCoresCount >= amount;
    }
    public void UseCores(Processus processus)
    {
        // we select the cores that will process the processus
        List<Core> selected_cores = new List<Core>();
        for (int i = 0; i < processus.cost; i++)
        {
            Core core = get_random_free_core();
            if (core == null)
            {
                Debug.LogWarning($"(ProcessCapacity) {capable.name} has a core overflow !!!");
                return;
            }
            core.Use(processus);
            selected_cores.Add(core);
        }

        // we add the processus to the running processes
        running_processes[processus] = selected_cores;
        if (debug) { Debug.Log($"(ProcessCapacity) {capable.name} using {processus.cost} cores"); }

        OnCoresFreedOrUsed?.Invoke(processus.cost);
    }
    public void FreeCores(Processus processus)
    {
        if (!running_processes.ContainsKey(processus))
        {
            if (debug) { Debug.LogWarning($"(ProcessCapacity) {capable.name} tried to free cores for a processus that is not running: {processus.name}"); }
            return;
        }

        // we free the cores used by the processus
        foreach (Core core in running_processes[processus]) { core.Free(); }
        if (debug) { Debug.Log($"(ProcessCapacity) {capable.name} freed {processus.cost} cores"); }

        // remove the processus
        running_processes.Remove(processus);
        OnCoresFreedOrUsed?.Invoke(-processus.cost);
    }

    // low level core management
    private Core get_random_free_core()
    {
        List<Core> free_cores = cores.Where(c => c.isFree).ToList();
        if (free_cores.Count == 0) { return null; }
        return free_cores[UnityEngine.Random.Range(0, free_cores.Count)];
    }


    // GRABBING PROCESSOR MODULE
    // todo : modify this so each Module_CPU has its own cores and we simply adds them to the cores list
    public void OnCPU_Changed()
    {
        // we check how many cpu modules we have in our inventory
        List<Item> cpus = capable.Inventory.GetItemsByRule("module:cpu");
        int new_max_cores = cpus.Count * 2; // each cpu provides 2 cores

        if (debug) { Debug.Log($"(ProcessCapacity) {capable.name} CPU changed. New max cores: {new_max_cores} / old cores: {cores.Count}"); }

        // check the difference between current and next cores.Count
        if (new_max_cores >= cores.Count) { set_new_max_cores(new_max_cores); return; }

        // if we have less cores, it's ok if we have have enough free cores left
        if (FreeCoresCount >= cores.Count - new_max_cores) { set_new_max_cores(new_max_cores); return; }

        // otherwise we need to free some used cores
        while (FreeCoresCount < cores.Count - new_max_cores)
        {
            // we free the first process in the list
            Processus first_process = running_processes.Keys.First();
            first_process.Overflow();
            FreeCores(first_process);
        }

        // finally we set the new max_cores
        set_new_max_cores(new_max_cores);
    }
    private void set_new_max_cores(int new_max_cores)
    {
        cores = new List<Core>();
        for (int i = 0; i < new_max_cores; i++) { cores.Add(new Core()); }
        // OnCoresChange?.Invoke(new_max_cores);
    }

}

[System.Serializable]
public class Core
{
    public bool isFree = true;
    public Processus RunningProcess = null;
    public Core() { }

    // USE / FREE
    public void Use(Processus processus)
    {
        isFree = false;
        RunningProcess = processus;
    }
    public void Free()
    {
        isFree = true;
        RunningProcess = null;
    }
}


[System.Serializable] public class Processus
{

    // PROGRAM
    public Program program;
    public string name => program.name;
    public int cost => program.cores_cost;

    // CONSTRUCTOR
    public Processus(Program program)
    {
        this.program = program;
    }


    // RUN , PROCESS & FINISH
    public virtual void Run(float duration) { }
    public virtual void Process() {}
    public virtual void Finish() {}
    public virtual void Overflow() {}
}