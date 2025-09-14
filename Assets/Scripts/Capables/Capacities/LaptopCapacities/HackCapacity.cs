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

    [Header("Scanner")]
    public Dictionary<Hackable, List<Exploit>> vulnerabilities = new Dictionary<Hackable, List<Exploit>>();
    // this stores the list of all the hackables we scanned & their vulnerabilites found !
    // todo : improve this by saving the last scan time and remove those when time > x
    // and maybe move it to a List<ScanResult> ???
    public event System.Action<Exploit> OnScanned = delegate { };


    [Header("Hackrays")]
    public GameObject hackray_prefab;
    [SerializeField] private Color hackray_color = Color.white;
    protected Dictionary<Hack, Hackray> hackrays = new Dictionary<Hack, Hackray>();


    [Header("Components")]
    [SerializeField] private Laptop laptop;
    private Hackable Target => laptop.GetCapacity<ConnectCapacity>()?.Target;

    // START
    private void Start()
    {
        laptop = capable.GetComponent<Laptop>();
    }

    // EXPLOIT SELECTION
    public void SelectExploit(Exploit exploit)
    {
        if (exploit == null) { return; }

        // we check if the exploit is TypePassword then we need to assign a password
        if (exploit == Exploit.TypePassword && Target != null && Target is Lockable lockable)
        {
            Key key = laptop.GetKeyFor(lockable);
            if (key != null)
            {
                exploit = new FileExploit(exploit, key);
            }
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
            if (hack.state == HackState.Completed && hack.name == "nmap") { Scan(hack.target); }

            // we check if the hack is done
            if (hack.state == HackState.Completed || hack.state == HackState.Failed || hack.state == HackState.Overflowed)
            {
                if (hack.target is Lockable lockable && hack.state == HackState.Completed)
                {
                    // we save the found key in our laptop
                    Key key = lockable.Key;
                    if (key != null && !laptop.HasKeyFor(lockable))
                    {
                        bool wrote_key = laptop.WriteFile(key);
                        if (debug && wrote_key) { Debug.Log($"(HackCapacity) {capable.name} found key {key} for {lockable.name} and saved it to its laptop."); }
                        else if (debug && !wrote_key) { Debug.LogWarning($"(HackCapacity) {capable.name} found key {key} for {lockable.name} but could not save it to its laptop (maybe full storage)."); }
                    }
                }

                if (debug) { Debug.Log($"(HackCapacity) {capable.name} finished hacking {hack.target.name} with exploit {hack.name}."); }
                remove_hack(i);
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

        // we free the cores used by the hack
        if (hack.state != HackState.Overflowed) { laptop.Processor.FreeCores(hack); }

        // we remove the hack from the running hacks
        running_hacks.RemoveAt(hack_index);
    }

    // USE
    public override void Use(Capable capable)
    {
        // checks if we have a connector
        ConnectCapacity connector = laptop.GetCapacity<ConnectCapacity>();
        if (connector == null)
        {
            if (debug) { Debug.LogWarning($"(HackCapacity) {capable.name} tried to hack but no connector is available."); }
            return;
        }

        // checks if we have a connected target
        Hackable target = connector.Target;
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

        // we check if our laptop has enough cores for this exploit
        if (debug && exploit == null) { Debug.LogWarning($"(HackCapacity) {capable.name} tried to hack {target.name} but has no exploit selected."); }
        if (debug && laptop.Processor == null) { Debug.LogWarning($"(HackCapacity) {capable.name} tried to hack {target.name} but has no processor."); }
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
        // we occupy some cores for the hack duration
        // will also calculate average cores speed based on the chosen cores
        laptop.Processor.UseCores(hack);

        // we run the hack
        hack.Run();

        // we add the hack to the running hacks
        running_hacks.Add(hack);
        hack.target.OnHackStarted(hack);

        // we notify that the exploit is run
        OnExploitRun?.Invoke(hack);

        // we create a hackray for this hack
        hackrays[hack] = create_hackray(hack.target);
    }

    // SCANNING
    public void Scan(Hackable target)
    {
        // todo we launch a nmap hack on the target ???
        // but for now we just scan vulnerabilities
        scan_vulnerabilities(target);
        OnScanned?.Invoke(selected_exploit);
    }
    private List<Exploit> scan_vulnerabilities(Hackable target)
    {
        // if we have some vulnerabilities found for this hackable we clear them
        vulnerabilities[target] = new List<Exploit>();

        // we check if we already have the key for this target (instant hack)
        string log_exploits = "";
        /* if (target is Lockable lockable && laptop.HasKeyFor(lockable))
        {
            vulnerabilities[target].Add(Exploit.TypePassword);
            log_exploits += $"- {Exploit.TypePassword.name} (instant hack)\n";
        } */

        List<Exploit> exploits = laptop.GetExploits();

        // we scan the other vulnerabilities
        for (int i = 0; i < exploits.Count; i++)
        {
            Exploit exploit = exploits[i];

            // we check if the exploit is TypePassword then we need to assign a password
            if (target is Lockable lockable && exploit == Exploit.TypePassword)
            {
                Key key = laptop.GetKeyFor(lockable);
                if (key != null)
                {
                    exploit = new FileExploit(exploit, key);
                }
            }

            // we check if the target is vulnerable to this exploit
            if (target.IsVulnerableTo(exploit))
            {
                vulnerabilities[target].Add(exploit);
                log_exploits += $"- {exploit.name}\n";
            }
        }

        // we select the first exploit
        selected_exploit = vulnerabilities[target][0];

        if (debug) { Debug.Log($"(HackCapacity) {capable.name} scanned {target.name} : {vulnerabilities[target].Count} vulnerabilities found\n{log_exploits}"); }
        return vulnerabilities[target];
    }

    // HACKRAY MANAGEMENT
    private Hackray create_hackray(Hackable target)
    {
        // we create a hackray for this hack
        Hackray hackray = Instantiate(hackray_prefab, transform).GetComponent<Hackray>();
        hackray.name = "hackray_" + target.name;
        hackray.SetLaptopAndTarget(laptop, target.transform);

        // apply color & material
        hackray.SetColor(hackray_color);
        hackray.SetMaterial(GetComponent<HackrayMaterialVariation>().hackray_material);

        return hackray;
    }

    // GETTERS
    public bool IsHacking(Hackable target)
    {
        // checks if we are hacking this target
        return running_hacks.Any(h => h.target == target);
    }
    public Exploit GetExploitVulnerabilities(Hackable target)
    {
        if (!vulnerabilities.ContainsKey(target)) { return Exploit.Nmap; } // if we never scanned the target we can't know if its vulnerable or not
        if (vulnerabilities[target].Count == 0) { return null; } // if we never found any vulnerabilities we return null
        return vulnerabilities[target][0]; // returns the first exploit found
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


