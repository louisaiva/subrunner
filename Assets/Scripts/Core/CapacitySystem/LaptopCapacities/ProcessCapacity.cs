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
    public List<Core> Cores { get { return cores; } }
    public int MaxCores => cores.Count;
    public int FreeCoresCount => cores.Count(c => c.isFree);
    public int UsedCoresCount => cores.Count(c => !c.isFree);
    public event Action<int> OnCoresNumberChanged = delegate { };

    // CORES MANAGEMENTS
    public bool HasFreeCores(int amount = 1)
    {
        return FreeCoresCount >= amount;
    }
    public List<Core> GetFreeCores(int amount)
    {
        List<Core> free_cores = get_random_free_cores(amount);
        return free_cores;
    }

    // low level core management
    private List<Core> get_random_free_cores(int amount = 1)
    {
        List<Core> free_cores = cores.Where(c => c.isFree).ToList();
        if (free_cores.Count < amount) { return null; }
        else if (free_cores.Count == amount) { return free_cores; }
        return free_cores.OrderBy(c => UnityEngine.Random.value).Take(amount).ToList();
    }

    // GRABBING / DROPPING PROCESSOR MODULE
    public void OnProcessorGrabbed(Module_CPU cpu)
    {
        if (log) { Debug.Log($"(ProcessCapacity) {Capable.name} grabbed a CPU module: {cpu.name}"); }

        bool cores_changed = false;

        // we go through all the cores we have and check if we need to add them to the list
        foreach (Core core in cpu.Cores)
        {
            if (cores.Contains(core)) { continue; }
            cores.Add(core);
            cores_changed = true;
        }

        if (cores_changed) { OnCoresNumberChanged?.Invoke(cores.Count); }
    }
    public void OnProcessorDropped(Module_CPU cpu)
    {
        if (log) { Debug.Log($"(ProcessCapacity) {Capable.name} dropped a CPU module: {cpu.name}"); }

        bool cores_changed = false;

        // we go through all the cores we have and check if we need to remove them from the list
        foreach (Core core in cpu.Cores)
        {
            if (!cores.Contains(core)) { continue; }

            // we overflow the processus if the core is not free
            if (!core.isFree)
            {
                if (log) { Debug.Log($"(ProcessCapacity) {Capable.name} lost a core that was used by {core.RunningProcess.name}. Overflowing process."); }
                core.RunningProcess.Fail(); // we overflow the processus -> will free the used cores
                // FreeCores(core.RunningProcess); // we don't only free this one core, but we free all the cores used by this processus
                // if (debug) { Debug.Log($"(ProcessCapacity) running process is ");}
            }

            cores.Remove(core);
            cores_changed = true;
        }

        if (cores_changed) { OnCoresNumberChanged?.Invoke(cores.Count); }
    }
}

[Serializable] public class Core
{
    public bool isFree = true;
    public float Speed = 1.0f; // percentage speed (1 == 100%)
    [SerializeReference] public Processus RunningProcess = null;
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


[Serializable] public class Processus
{
    // public static Processus Null = new Processus(FileBank.Instance.GetFile<Program>("null_program"));

    // PROGRAM
    public Program program;
    public string name => program.name;
    public ProcessusState state = ProcessusState.NotStarted;


    // CORES MANAGEMENT
    public int cost => program.cores_cost;
    public List<Core> provided_cores = new List<Core>();
    public int provided_cores_count => provided_cores.Count;



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
    protected void register_core_speed(Core core)
    {
        // speed is at zero so we can add the core speed / cost and NORMALLY it should be okay
        average_cores_speed += core.Speed / cost;
    }
    protected void free_all_cores()
    {
        if (Logger.Instance.LOG_CORES) { Debug.Log($"---> (Processus) {name} is freeing {provided_cores.Count} cores (all of them)"); }

        foreach (Core core in provided_cores) { core.Free(); }
        provided_cores.Clear();
    }
    protected void free_cores(int amount)
    {
        if (Logger.Instance.LOG_CORES) { Debug.Log($"---> (Processus) {name} is freeing {amount} cores"); }

        for (int i = 0; i < amount; ++i)
        {
            if (provided_cores.Count == 0) { break; }
            Core core = provided_cores[0];
            core.Free();
            provided_cores.RemoveAt(0);
        }
    }
    protected void use_all_cores()
    {
        foreach (Core core in provided_cores)
        {
            core.Use(this);
            register_core_speed(core); // calculates the speed
        }

        if (Logger.Instance.LOG_CORES) { Debug.Log($"---> (Processus) {name} is using {provided_cores.Count} cores with an average speed of {average_cores_speed}"); }
    }

    // RUN , PROCESS & FINISH
    public virtual void Run(List<Core> cores)
    {
        // we use the cores
        provided_cores = cores;
        use_all_cores();

        // we set the state to running
        state = ProcessusState.Running;
    }
    public virtual void Process() { }
    public virtual void Finish() { }
    public virtual void Fail() { }
}

public enum ProcessusState
{
    NotStarted,
    Running,
    Completed,
    Failed,
    Overflowed,
    Waiting, // for WaitEnd & Timer exploits
}