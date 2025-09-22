using System.Collections.Generic;
using System.Linq;
// using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// HackCapacity is a capacity that allows a being to hack things
/// in their range. need to have a laptop for this to work
/// </summary>

public class HackCapacity : Capacity
{
    [Header("Exploit selection")]
    public Exploit selected_exploit;
    public event System.Action OnDeselected = delegate { };

    [Header("Hacks")]
    public List<Hack> running_hacks = new List<Hack>();
    public event System.Action<Hack> OnExploitRun = delegate { };
    public event System.Action<Exploit> OnScanned = delegate { };


    [Header("Hackrays")]
    public GameObject hackray_prefab;
    [SerializeField] private Color hackray_color = Color.white;
    protected Dictionary<Hack, Hackray> hackrays = new Dictionary<Hack, Hackray>();


    [Header("Components")]
    [SerializeField] private Laptop laptop;
    [SerializeField] private ConnectCapacity connector;
    
    // START
    private void Start()
    {
        laptop = capable.GetComponent<Laptop>();
    }

    // EXPLOIT SELECTION
    public void OverrideExploit(Exploit exploit, Vulnerable Target)
    {
        if (exploit == null) { return; }

        // we check if the exploit is TypePassword then we need to assign a password
        if (exploit == Exploit.TypePassword && Target != null && Target.capable is Lockable lockable)
        {
            Key key = laptop.GetKeyFor(lockable);
            if (key != null) { exploit = new FileExploit(exploit, key); }
        }

        // we set the selected exploit
        selected_exploit = exploit;
        if (debug) { Debug.Log($"(HackCapacity) {capable.name} selected exploit {exploit.name}."); }
    }
    public void DeselectExploit()
    {
        selected_exploit = null;
        OnDeselected?.Invoke();
    }

    // UPDATE
    protected override void Update()
    {
        // we cycle through all the running exploits and we check few things
        for (int i = running_hacks.Count - 1; i >= 0; --i)
        {
            Hack hack = running_hacks[i];

            // if the hack is completed and it was a nmap, we scan the target
            if (hack.state == ProcessusState.Completed && hack.name == "nmap") { Scan(hack.target); }

            // we check if the hack is done
            if (hack.state == ProcessusState.Completed || hack.state == ProcessusState.Failed)
            {
                // we donwload files that the hack found if there are any
                foreach (File file in hack.downloads)
                {
                    bool wrote_file = laptop.WriteFile(file);
                    if (debug && wrote_file) { Debug.Log($"(HackCapacity) {capable.name} downloaded file {file.name} from {hack.target.name} and saved it to its laptop."); }
                    else if (debug && !wrote_file) { Debug.LogWarning($"(HackCapacity) {capable.name} downloaded file {file.name} from {hack.target.name} but could not save it to its laptop (maybe full storage)."); }
                }

                // we remove the hack
                if (debug) { Debug.Log($"(HackCapacity) {capable.name} finished exploit {hack.name}."); }
                remove_hack(i);
                continue;
            }

            /* if (hack.state == ProcessusState.Freeing)
            {
                laptop.Processor.FreeCores(hack);
                hack.state = ProcessusState.Waiting;
                continue;
            } */

            if (hack.state == ProcessusState.Waiting)
            {
                hack.Wait();
                continue;
            }

            hack.Process();
        }
    }
    private void remove_hack(int hack_index)
    {
        Hack hack = running_hacks[hack_index];

        // we remove the hackray
        Destroy(hackrays[hack].gameObject);
        hackrays.Remove(hack);

        // we remove the hack from the running hacks
        running_hacks.RemoveAt(hack_index);
    }

    // USE
    public override void Use(Capable capable)
    {
        // checks if we have a connector
        // ConnectCapacity connector = laptop.GetCapacity<ConnectCapacity>();
        if (connector == null)
        {
            if (debug) { Debug.LogWarning($"(HackCapacity) {capable.name} tried to hack but no connector is available."); }
            return;
        }

        // checks if we have a connected target
        Vulnerable target = connector.Target;
        if (target == null)
        {
            if (debug) { Debug.LogWarning($"(HackCapacity) {capable.name} tried to hack but no target is connected."); }
            return;
        }

        // we check if we are not already hacking this target
        if (running_hacks.Any(h => h.target == target))
        {
            if (debug) { Debug.LogWarning($"(HackCapacity) {capable.name} is already hacking {target.name}."); }
            return;
        }

        Exploit exploit = selected_exploit;
        if (exploit == null)
        {
            // todo : bug quelques fois on a pas d'exploit selectionné, est-ce qu'il faut re nmap ?
            if (debug) { Debug.LogWarning($"(HackCapacity) {capable.name} tried to hack {target.name} but has no exploit selected."); }
            return;
        }

        // we check if our laptop has enough cores for this exploit
        if (!laptop.Processor.HasFreeCores(exploit.cores_cost))
        {
            if (debug) { Debug.LogWarning($"(HackCapacity) {capable.name} tried to hack {target.name} but has no free cores for exploit {exploit.name}."); }
            return;
        }

        // we create a Hack for this target
        Hack hack = new Hack(connector.connection, exploit);

        // we run the exploit
        RunExploit(hack);
    }
    public void RunExploit(Hack hack)
    {
        // we get some free cores from the laptop
        List<Core> free_cores = laptop.Processor.GetFreeCores(hack.program.cores_cost);

        // we run the hack
        hack.Run(free_cores);

        // we add the hack to the running hacks
        running_hacks.Add(hack);
        hack.target.OnHackStarted(hack);

        // we notify that the exploit is run
        OnExploitRun?.Invoke(hack);

        // we create a hackray for this hack
        hackrays[hack] = create_hackray(hack.target);
    }

    // SCANNING
    public void Scan(Vulnerable target)
    {
        // we list all the exploits we have
        List<Exploit> exploits = laptop.GetExploits();

        // we associate the key file to type_password if it's a lockable and if we have the key
        if (target.capable is Lockable lockable)
        {
            Key key = laptop.GetKeyFor(lockable);
            if (key != null)
            {
                exploits.Remove(Exploit.TypePassword); // we remove the empty type password exploit
                exploits.Add(new FileExploit(Exploit.TypePassword, key));
            }
        }

        // we find the better suited exploit & select it
        selected_exploit = target.GetHighestVulnerability(exploits);

        // we invoke the event
        OnScanned?.Invoke(selected_exploit);
    }
    public void SetConnector(ConnectCapacity connect)
    {
        connector = connect;
    }

    // HACKRAY MANAGEMENT
    private Hackray create_hackray(Vulnerable target)
    {
        // we create a hackray for this hack
        Hackray hackray = Instantiate(hackray_prefab, transform).GetComponent<Hackray>();
        hackray.name = "hackray_" + target.name;
        // hackray.SetLaptopAndTarget(laptop, target.transform);
        hackray.SetConnectors(connector, target.Connector);

        // apply color & material
        hackray.SetColor(hackray_color);
        hackray.SetMaterial(GetComponent<HackrayMaterialVariation>().hackray_material);

        return hackray;
    }

    // GETTERS
    public bool IsHacking(Vulnerable vulnerable)
    {
        // checks if we are hacking this target
        return running_hacks.Any(h => h.target == vulnerable);
    }
    
    // ON DESTROY
    private void OnDestroy()
    {
        for (int i = running_hacks.Count - 1; i >= 0; --i)
        {
            Hack hack = running_hacks[i];
            hack.Fail();
            remove_hack(i); // we remove the hack
        }
    }
}


