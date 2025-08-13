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
    [Header("Target Selection")]
    public Hackable hovered_target;

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
    [SerializeField] private CircleCollider2D hack_collider;
    public float Radius { get => hack_collider.radius; }

    [Header("Logs")]
    [SerializeField] private bool log_hack_progress = false;


    // START
    private void Start()
    {
        laptop = capable.GetComponent<Laptop>();
        hack_collider = laptop.GetCapacity<InteractHackCapacity>()?.GetComponent<CircleCollider2D>();
    }

    // TARGET MANAGEMENT
    public void Select(Hackable target)
    {
        // we check if the target is already selected
        if (target == hovered_target) { return; }

        // we set the hovered target
        if (debug) { Debug.Log($"(HackCapacity) selected target: {target.name}"); }
        hovered_target = target;
    }
    public void Deselect()
    {
        if (hovered_target == null) { return; }

        // we reset the hovered target
        if (debug) { Debug.Log($"(HackCapacity) deselected target"); }
        hovered_target = null;
    }

    // UPDATE
    protected override void Update()
    {
        base.Update();

        // we cycle through all the running exploits and we check few things
        for (int i = running_hacks.Count - 1; i >= 0; --i)
        {
            Hack hack = running_hacks[i];

            // we check if the hack is done
            if (hack.state == HackState.Completed || hack.state == HackState.Failed || hack.state == HackState.Overflowed)
            {
                if (debug) { Debug.Log($"(HackCapacity) {capable.name} finished hacking {hack.target.name} with exploit {hack.exploit.name}."); }
                remove_hack(i);
                continue;
            }

            // checks if we are too far away from the target
            Hackable hackable = hack.target;
            if (Vector3.Distance(transform.position, hackable.transform.position) > hack_collider.radius)
            {
                if (debug) { Debug.LogWarning($"(HackCapacity) {capable.name} is too far away from {hack.target.name} to continue the hack."); }

                hack.Fail();
                remove_hack(i);
                continue;
            }

            hack.Process();
            /* if (log_hack_progress)
            {
                Debug.Log($"(HackCapacity) updating exploit {hack.exploit.name}. Progress: {hack.progress}%");
            } */
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
        // checks if we have a hovered target
        if (hovered_target == null)
        {
            if (debug) { Debug.LogWarning($"(HackCapacity) {capable.name} tried to hack but no target is hovered."); }
            return;
        }

        // we check if we are not already hacking this target
        if (running_hacks.Any(h => h.target == hovered_target))
        {
            if (debug) { Debug.LogWarning($"(HackCapacity) {capable.name} is already hacking {hovered_target.name}."); }
            return;
        }

        // we try to connect to the target
        if (!Connect(hovered_target))
        {
            if (debug) { Debug.LogWarning($"(HackCapacity) {capable.name} failed to connect to {hovered_target.name}."); }
            return;
        }

        // we scan the target
        List<Exploit> vulnerabilities = Scan(hovered_target);
        if (vulnerabilities.Count == 0)
        {
            if (debug) { Debug.LogWarning($"(HackCapacity) {capable.name} found no vulnerabilities on {hovered_target.name}."); }
            return;
        }
        Exploit exploit = vulnerabilities[0];

        // we check if our laptop has enough cores for this exploit
        if (!laptop.HasFreeCores(exploit.cores_cost))
        {
            if (debug) { Debug.LogWarning($"(HackCapacity) {capable.name} tried to hack {hovered_target.name} but has no free cores for exploit {exploit.name}."); }
            return;
        }

        // we create a Hack for this target
        Hack hack = new Hack(hovered_target, exploit);

        // we run the exploit
        RunExploit(hack);
    }

    // HACKING HIGH LEVEL
    public bool Connect(Hackable target)
    {
        if (target == null) { return false; }

        if (target is Lockable lockable && !lockable.Locked) { return false; } // if the target is a lockable and it is not locked, we can't hack it
        
        // we check if the target is in range
        if (Vector3.Distance(transform.position, target.transform.position) > hack_collider.radius)
        {
            if (debug) { Debug.LogWarning($"(HackCapacity) {capable.name} tried to connect to {target.name} but it is out of range."); }
            return false;
        }
        return true;
    }
    public List<Exploit> Scan(Hackable target)
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
        if (debug) { Debug.Log($"(HackCapacity) {capable.name} scanned {target.name} : {vulnerabilities[target].Count} vulnerabilities found\n{log_exploits}"); }
        return vulnerabilities[target];
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
}




[System.Serializable]
public class Hack
{
    [Header("Hack Details")]
    public float progress;
    public Hackable target;
    public Exploit exploit;
    public float duration;
    public HackState state = HackState.NotStarted;

    // CONSTRUCTOR
    public Hack(Hackable target, Exploit exploit)
    {
        this.target = target;
        this.exploit = exploit;
        this.progress = 0f;
    }

    // PROCESS
    public void Process()
    {
        // we update the progress of the hack
        progress += Time.deltaTime / duration * 100f;

        // we check if the hack is done
        if (progress >= 100f) { Finish(); }
    }

    // RUN & FINISH
    public void Run(float duration)
    {
        this.duration = duration;
        this.progress = 0f;
        state = HackState.Running;
        Debug.Log($"Starting hack on {target.name} with exploit {exploit.name}");
    }
    public void Finish()
    {
        // the hack is done. but have we successfully hacked the target ?
        // this is another thing.
        // we check if the hackable is vulnerable to the exploit
        if (target.IsVulnerableTo(exploit)) { Complete(); }
        else { Fail(); }
    }

    // FAIL & COMPLETE
    public void Fail()
    {
        // the hack has failed :///
        Debug.Log($"Hack on {target.name} with exploit {exploit.name} was quit.");
        // this.progress = 0f;
        this.state = HackState.Failed;

        // Notify the target that the hack is failed
        target.OnHackFailed(this);
    }
    public void Complete()
    {
        // the hack is successful !!
        Debug.Log($"Hack on {target.name} with exploit {exploit.name} completed successfully.");
        this.progress = 100f;
        this.state = HackState.Completed;

        // Notify the target that the hack is completed
        target.OnHackCompleted(this);
    }
    public void Overflow()
    {
        // the hack has overflowed :///
        Debug.LogWarning($"Hack on {target.name} with exploit {exploit.name} has overflowed. Freeing cores.");
        // this.progress = 0f;
        this.state = HackState.Overflowed;

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
    public static readonly Exploit Nmap = new Exploit("nmap", 0, 0.1f, 1);
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