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
    public event Action<int> OnCoresFreedOrUsed = delegate { };

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


    // GRABBING / DROPPING PROCESSOR MODULE
    public void OnProcessorGrabbed(Module_CPU cpu)
    {
        if (debug) { Debug.Log($"(ProcessCapacity) {capable.name} grabbed a CPU module: {cpu.name}"); }

        // we go through all the cores we have and check if we need to add them to the list
        foreach (Core core in cpu.Cores)
        {
            if (cores.Contains(core)) { continue; }
            cores.Add(core);
        }
    }
    public void OnProcessorDropped(Module_CPU cpu)
    {
        if (debug) { Debug.Log($"(ProcessCapacity) {capable.name} dropped a CPU module: {cpu.name}"); }

        // we go through all the cores we have and check if we need to remove them from the list
        foreach (Core core in cpu.Cores)
        {
            if (!cores.Contains(core)) { continue; }

            // we overflow the processus if the core is not free
            if (!core.isFree)
            {
                if (debug) { Debug.Log($"(ProcessCapacity) {capable.name} lost a core that was used by {core.RunningProcess.name}. Overflowing process."); }
                FreeCores(core.RunningProcess); // we don't only free this one core, but we free all the cores used by this processus
                core.RunningProcess.Overflow(); // we overflow the processus
            }

            cores.Remove(core);
        }

        
    }
    /* public void OnCPU_Changed()
    {
        List<Module_CPU> cpus = capable.Inventory.GetItemsByRule("module:cpu")
                                                .Select(item => item as Module_CPU)
                                                .Where(cpu => cpu != null)
                                                .ToList();

        List<Core> new_cores = cpus.SelectMany(cpu => cpu.Cores).ToList();
        if (debug) { Debug.Log($"(ProcessCapacity) {capable.name} CPU changed. New max cores: {new_cores.Count} / old cores: {cores.Count}"); }


        // we go through all the cores we had and check if they aren't in the list
        List<Core> cores_to_remove = new List<Core>();
        foreach (Core core in cores)
        {
            if (new_cores.Contains(core)) { continue; }
            cores_to_remove.Add(core);
        }

        // we go through all the cores we have and check if we need to add them to the list
        foreach (Core core in new_cores)
        {
            if (cores.Contains(core)) { continue; }
            cores.Add(core);
        }

        // finally we remove the old cores and overflow their processus if they were used
        foreach (Core core in cores_to_remove)
        {
            if (!core.isFree)
            {
                if (debug) { Debug.Log($"(ProcessCapacity) {capable.name} lost a core that was used by {core.RunningProcess.name}. Overflowing process."); }
                FreeCores(core.RunningProcess); // we don't only free this one core, but we free all the cores used by this processus
                core.RunningProcess.Overflow(); // we overflow the processus
            }
            cores.Remove(core);
        }
    } */
}

[Serializable] public class Core
{
    public bool isFree = true;
    public float Speed = 1.0f; // percentage speed (1 == 100%)
    public Processus RunningProcess = Processus.Null;
    public Core() { }

    // USE / FREE
    public void Use(Processus processus)
    {
        isFree = false;
        RunningProcess = processus;

        // we register the core in the processus
        processus.RegisterCore(this);
    }
    public void Free()
    {
        isFree = true;
        RunningProcess = Processus.Null;
    }
}


[Serializable] public class Processus
{
    public static Processus Null = new Processus(new Program("null", 0, 0));

    // PROGRAM
    public Program program;
    public string name => program.name;
    public int cost => program.cores_cost;

    // SPEED CALCULATIONS
    public float average_cores_speed = 0.0f; // average speed of the cores used by the processus
    public float os_speed = 1.0f; // bonus speed for if the os speed has particular speed for this kind of processus

    // CONSTRUCTOR
    public Processus(Program program)
    {
        this.program = program;

        // reset speeds
        average_cores_speed = 0.0f;
        os_speed = 1.0f;
    }

    // Cores management
    public void RegisterCore(Core core)
    {
        // speed is at zero so we can add the core speed / cost and NORMALLY it should be okay
        average_cores_speed += core.Speed / cost;
    }


    // RUN , PROCESS & FINISH
    public virtual void Run() { }
    public virtual void Process() {}
    public virtual void Finish() {}
    public virtual void Overflow() {}
}