using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// HackCapacity is a capacity that allows its capable
/// to run exploits (create Hack) on a Vulnerable
/// </summary>

public class HackCapacity : Capacity
{
    public bool log_scan = false;
    public bool log_cancel = false;

    [Header("Exploit selection")]
    public Exploit selected_exploit;

    [Header("Hacks")]
    public List<Hack> running_hacks = new List<Hack>();
    public event System.Action<Hack> OnExploitRun = delegate { };


    [Header("Hackrays")]
    public GameObject hackray_prefab;
    [SerializeField] private Color hackray_color = Color.white;
    protected Dictionary<Hack, Hackray> hackrays = new Dictionary<Hack, Hackray>();


    [Header("Components")]
    [SerializeField] private Device device;
    [SerializeField] private ConnectCapacity connector;
    
    // START
    private void Start()
    {
        device = capable.GetComponent<Device>();
    }

    // EXPLOIT SELECTION
    public void OverrideExploit(Exploit exploit)
    {
        if (exploit == null) { return; }

        /* // we check if the exploit is TypePassword then we need to assign a password
        if (exploit is FileExploit file_exploit && Target != null && Target.capable is Lockable lockable)
        {
            Key key = device.GetKeyFor(lockable);
            file_exploit.file = key;
        } */

        // we set the selected exploit
        selected_exploit = exploit;
        if (debug) { Debug.Log($"(HackCapacity) {capable.name} selected exploit {exploit.name}."); }
    }
    
    // UPDATE
    protected override void Update()
    {
        // we cycle through all the running exploits and we check few things
        for (int i = running_hacks.Count - 1; i >= 0; --i)
        {
            Hack hack = running_hacks[i];

            // RUNNING HACKS
            if (hack.state == ProcessusState.Running) { hack.Process(); }
            else if (hack.state == ProcessusState.Waiting) { hack.Wait(); }
        }
    }


    // HACK MANAGEMENT
    public void TerminateHack(Hack hack)
    {
        if (!new List<ProcessusState> { ProcessusState.Completed, ProcessusState.Failed }.Contains(hack.state)) { return; }

        // NMAP
        if (hack.state == ProcessusState.Completed && hack.name == "nmap") { Scan(hack.target); }

        if (log_cancel && hack.state == ProcessusState.Failed) { Debug.Log($"(HackCapacity) {capable.name} finished exploit {hack.name} on {hack.target.capable.name} with state {hack.state}."); }

        // we download files if there are any
        download_files(hack);
        remove_hack(hack);
    }
    private void remove_hack(Hack hack)
    {
        if (!running_hacks.Contains(hack))
        {
            Debug.LogError($"(HackCapacity) {capable.name} tried to remove a hack {hack.name} but it's not running.");
            return;
        }

        if (debug) { Debug.Log($"(HackCapacity) {capable.name} finished exploit {hack.name}."); }

        // we remove the hackray
        Destroy(hackrays[hack].gameObject);
        hackrays.Remove(hack);

        // we remove the hack from the running hacks
        running_hacks.Remove(hack);
    }
    private void download_files(Hack hack)
    {
        // we donwload files that the hack found if there are any
        foreach (File file in hack.downloads)
        {
            bool wrote_file = device.WriteFile(file);
            if (debug && wrote_file) { Debug.Log($"(HackCapacity) {capable.name} downloaded file {file.name} from {hack.target.name} and saved it to its device."); }
            else if (debug && !wrote_file) { Debug.LogWarning($"(HackCapacity) {capable.name} downloaded file {file.name} from {hack.target.name} but could not save it to its device (maybe full storage)."); }
        }
    }


    // HACKING INPUT METHODS
    public override void Use(Capable capable)
    {
        // checks if we have a connector
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
            // ? todo : bug quelques fois on a pas d'exploit selectionné, est-ce qu'il faut re nmap ?
            if (debug) { Debug.LogWarning($"(HackCapacity) {capable.name} tried to hack {target.name} but has no exploit selected."); }
            return;
        }

        // cas spécial de si on a un TypePassword
        if (exploit is FileExploit file_exploit && target.capable is Lockable lockable && file_exploit.file == null)
        {
            Key key = device.GetKeyFor(lockable);
            if (key != null) { file_exploit.file = key; }
        }

        // we check if our device has enough cores for this exploit
        if (!device.Processor.HasFreeCores(exploit.cores_cost))
        {
            if (debug) { Debug.LogWarning($"(HackCapacity) {capable.name} tried to hack {target.name} but has no free cores for exploit {exploit.name}."); }
            return;
        }

        // we create a Hack for this target
        Hack hack = new Hack(connector.connection, exploit, this);

        // we run the exploit
        RunExploit(hack);
    }
    public void RunExploit(Hack hack)
    {
        // we get some free cores from the device
        List<Core> free_cores = device.Processor.GetFreeCores(hack.program.cores_cost);

        if (debug) { Debug.Log($"(HackCapacity) {capable.name} is running exploit {hack.name} on {hack.target.capable.name} using {free_cores.Count} cores."); }

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
    public void CancelLastHack()
    {
        if (running_hacks.Count == 0) { return; }

        // we get the last hack
        Hack hack = running_hacks[running_hacks.Count - 1];

        // if it's already failed then we don't want to remove it (bug the CancelLastHack() method were called
        // twice in a frame by the Controller.control() which call UI_Manager.SwitchTo("device") which
        // recalls UI_ExploitSelector.ConfirmChoice() but it will be fixed when we rework UI_Manager)
        if (hack.state == ProcessusState.Failed) { return; }

        // we cancel the last hack
        if (log_cancel) { Debug.Log($"(HackCapacity) {capable.name} is cancelling its last hack {hack.name} on {hack.target.capable.name}."); }
        hack.Fail();
    }
    public void CancelControlHacks()
    {
        // we cancel all trojan & cyborg_puppets hacks
        for (int i = running_hacks.Count - 1; i >= 0; --i)
        {
            Hack hack = running_hacks[i];
            if (hack.name == "trojan" || hack.name == "cyborg_puppet")
            {
                hack.Fail();
            }
        }
    }
    public bool IsHacking(Vulnerable vulnerable)
    {
        // checks if we are hacking this target
        return running_hacks.Any(h => h.target == vulnerable);
    }


    // SCANNING
    public void Scan(Vulnerable target)
    {
        // we list all the exploits we have
        List<Exploit> exploits = device.GetExploits();

        string s = $"(HackCapacity) {capable.name} scanning {target.name} ";

        // we associate the key file to type_password if it's a lockable and if we have the key
        if (target.capable is Lockable lockable)
        {
            Key key = device.GetKeyFor(lockable);
            if (key != null)
            {
                FileExploit type_password_exploit = FileBank.Instance.TypePassword;
                type_password_exploit.file = key;
                exploits.Add(type_password_exploit);
                s += $"(found key) ";
            }
        }

        foreach (Exploit exploit in exploits)
        {
            s += $"\n - {exploit.name}";
            if (exploit is FileExploit file_exploit && file_exploit.file != null)
            {
                s += $" (with file {file_exploit.file.data})";
            }
        }
        if (log_scan) { Debug.Log(s); }

        // we find the better suited exploit & select it
        Controller.Instance.ExploitNavigator.SetScannedExploit(target.GetHighestVulnerability(exploits));
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
        // hackray.SetLaptopAndTarget(device, target.transform);
        hackray.SetConnectors(connector, target.Connector);

        // apply color & material
        hackray.SetColor(hackray_color);
        hackray.SetMaterial(GetComponent<HackrayMaterialVariation>().hackray_material);

        return hackray;
    }
    public void RetractHackray(Hack hack)
    {
        if (!hackrays.ContainsKey(hack)) { return; }
        hackrays[hack].SetThickness();
    }

    // ON DESTROY
    private void OnDestroy()
    {
        for (int i = running_hacks.Count - 1; i >= 0; --i)
        {
            Hack hack = running_hacks[i];
            hack.Fail();
        }
    }
}


