using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// HackCapacity is a capacity that allows a being to hack things
/// in their range. need to have a laptop for this to work
/// </summary>

public class HackCapacity : Capacity
{
    [Header("Exploit selection")]
    public Exploit selected_exploit;

    [Header("Hacks")]
    public List<Hack> running_hacks = new List<Hack>();
    public event System.Action<Hack> OnExploitRun = delegate { };

    [Header("Exploits")]
    public List<Exploit> exploits = new List<Exploit>();

    [Header("Vulnerabilities found")]
    public Dictionary<Hackable, List<Exploit>> vulnerabilities = new Dictionary<Hackable, List<Exploit>>();
    // this stores the list of all the hackables we scanned & their vulnerabilites found !
    // todo : improve this by saving the last scan time and remove those when time > x
    // and maybe move it to a List<ScanResult> ???

    [Header("Hackrays")]
    public GameObject hackray_prefab;
    [SerializeField] private Color hackray_color = Color.white;
    protected Dictionary<Hack, Hackray> hackrays = new Dictionary<Hack, Hackray>();


    [Header("Components")]
    [SerializeField] private Laptop laptop;

    // START
    private void Start()
    {
        laptop = capable.GetComponent<Laptop>();
    }

    // UPDATE
    protected override void Update()
    {
        // we cycle through all the running exploits and we check few things
        for (int i = running_hacks.Count - 1; i >= 0; --i)
        {
            Hack hack = running_hacks[i];

            // if the hack is completed and it was a nmap, we scan the target
            if (hack.state == HackState.Completed && hack.name == "nmap") { scan_vulnerabilities(hack.target); }

            // we check if the hack is done
            if (hack.state == HackState.Completed || hack.state == HackState.Failed || hack.state == HackState.Overflowed)
            {
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
        if (hack.state != HackState.Overflowed) { laptop.FreeCores(hack); }

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
        /* if (exploit == Exploit.Nmap)
        {
            // we scan the target
            List<Exploit> vulnerabilities = Scan(target);
            if (vulnerabilities.Count == 0)
            {
                if (debug) { Debug.LogWarning($"(HackCapacity) {capable.name} found no vulnerabilities on {target.name}."); }
                return;
            }
            exploit = vulnerabilities[0]; // if no exploit is selected, we take the first one found
        } */

        // we check if our laptop has enough cores for this exploit
        if (!laptop.HasFreeCores(exploit.cores_cost))
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
        // we run the hack
        float duration = hack.CalculateDuration();
        hack.Run(duration);

        // we occupy some cores for the hack duration
        laptop.UseCores(hack);

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
    }
    private List<Exploit> scan_vulnerabilities(Hackable target)
    {
        // if we have some vulnerabilities found for this hackable we clear them
        vulnerabilities[target] = new List<Exploit>();

        // we check if we already have the key for this target (instant hack)
        string log_exploits = "";
        if (target is Lockable lockable && laptop.HasKeyFor(lockable))
        {
            vulnerabilities[target].Add(Exploit.InsertPassword);
            log_exploits += $"- {Exploit.InsertPassword.name} (instant hack)\n";
        }

        // we scan the other vulnerabilities
        for (int i = 0; i < exploits.Count; i++)
        {
            Exploit exploit = exploits[i];
            if (target.IsVulnerableTo(exploit))
            {
                vulnerabilities[target].Add(exploit);
                log_exploits += $"- {exploit.name}\n";
            }
        }

        // we always add Nmap as last vulnerability
        vulnerabilities[target].Add(Exploit.Nmap);
        selected_exploit = vulnerabilities[target][0]; // we select the first exploit

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




[System.Serializable]
public class Hack
{
    [Header("Hack Details")]
    public float progress;
    public Connection tunnel;
    public Exploit exploit;
    public float duration;
    public HackState state = HackState.NotStarted;
    public Hackable target => tunnel.target;
    public string name => exploit.name;

    // CONSTRUCTOR
    public Hack(Connection tunnel, Exploit exploit)
    {
        this.tunnel = tunnel;
        this.exploit = exploit;
        this.progress = 0f;
    }

    // RUN , PROCESS & FINISH
    public void Run(float duration)
    {
        // open the tunnel
        tunnel.Open();

        // run the hack
        this.duration = duration;
        state = HackState.Running;
        Debug.Log($"Starting hack on {target.name} with exploit {exploit.name}");
    }
    public void Process()
    {
        // checks if the tunnel is still open
        if (tunnel.state != ConnectionState.Opened)
        {
            Debug.LogWarning($"Hack on {target.name} with exploit {exploit.name} was interrupted because the tunnel was closed");
            Fail();
            return;
        }

        // we update the progress of the hack
        progress += Time.deltaTime / duration * 100f;

        // we check if the hack is done
        if (progress >= 100f) { Finish(); }
    }
    public void Finish()
    {
        // we check if the hackable is vulnerable to the exploit
        if (name == "nmap" || target.IsVulnerableTo(exploit)) { Complete(); }
        else { Fail(); }
    }

    // FAIL & COMPLETE
    public void Fail()
    {
        // the hack has failed :///
        Debug.Log($"Hack on {target.name} with exploit {exploit.name} was quit.");
        this.state = HackState.Failed;

        // we close the connection
        tunnel.Close();

        // Notify the target that the hack is failed
        target.OnHackFailed(this);
    }
    public void Complete()
    {
        // the hack is successful !!
        Debug.Log($"Hack on {target.name} with exploit {exploit.name} completed successfully.");
        this.progress = 100f;
        this.state = HackState.Completed;

        // we close the connection
        tunnel.Close();

        // Notify the target that the hack is completed
        target.OnHackCompleted(this);
    }
    public void Overflow()
    {
        // the hack has overflowed :///
        Debug.LogWarning($"Hack on {target.name} with exploit {exploit.name} has overflowed. Freeing cores.");
        this.state = HackState.Overflowed;

        // we close the connection
        tunnel.Close();

        // Notify the target that the hack is failed
        target.OnHackFailed(this);
    }

    // GETTERS
    public float CalculateDuration(float duration_multiplier = 2.25f)
    {
        float duration = exploit.base_duration;
        int security_level_difference = target.SecurityLevel - exploit.security_level;

        // checks if the difference is > 100
        if (security_level_difference > 100) { return 1000f; }
        else if (security_level_difference < -1000) { return 0.1f; }

        // multiply the duration by multiplier once for eache security level difference
        if (security_level_difference > 0) { duration_multiplier = 1 / duration_multiplier; }
        for (int i = 0; i < security_level_difference; ++i)
        {
            duration *= duration_multiplier;
        }
        return duration;
    }
}

public enum HackState
{
    NotStarted,
    Running,
    Completed,
    Failed,
    Overflowed
}

[System.Serializable]
public class Exploit
{
    public static readonly Exploit Nmap = new Exploit("nmap", 1000, 0.1f, 1);
    public static readonly Exploit InsertPassword = new Exploit("insert_password", 1000, 0.1f, 1);

    [Header("Exploit Details")]
    public string name;
    public int security_level;
    public float base_duration;
    public int cores_cost;

    // CONSTRUCTOR
    public Exploit(string name, int security_level, float base_duration, int cores_cost)
    {
        this.name = name;
        this.security_level = security_level;
        this.base_duration = base_duration;
        this.cores_cost = cores_cost;
    }
    public Exploit(Exploit exploit, float base_duration = default)
    {
        this.name = exploit.name;
        this.security_level = exploit.security_level;
        this.base_duration = base_duration == default ? exploit.base_duration : base_duration;
        this.cores_cost = exploit.cores_cost;
    }
}