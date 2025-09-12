using UnityEngine;

// HACKS (PROCESSUS)

[System.Serializable]
public class Hack : Processus
{
    [Header("Hack Details")]
    public float progress;
    public Connection tunnel;
    // public Exploit exploit;
    public float duration;
    public HackState state = HackState.NotStarted;
    public Hackable target => tunnel.target;
    // public string name => exploit.name;

    // CONSTRUCTOR
    public Hack(Connection tunnel, Exploit exploit) : base(exploit)
    {
        this.tunnel = tunnel;
        this.progress = 0f;
    }

    // RUN , PROCESS & FINISH
    public override void Run(float duration)
    {
        // open the tunnel
        tunnel.Open();

        // run the hack
        this.duration = duration;
        state = HackState.Running;
        Debug.Log($"Starting hack on {target.name} with exploit {name}");
    }
    public override void Process()
    {
        // checks if the tunnel is still open
        if (tunnel.state != ConnectionState.Opened)
        {
            Debug.LogWarning($"Hack on {target.name} with exploit {name} was interrupted because the tunnel was closed");
            Fail();
            return;
        }

        // we update the progress of the hack
        progress += Time.deltaTime / duration * 100f;

        // we check if the hack is done
        if (progress >= 100f) { Finish(); }
    }
    public override void Finish()
    {
        // we check if the hackable is vulnerable to the exploit
        if (name == "nmap" || target.IsVulnerableTo(program as Exploit)) { Complete(); }
        else { Fail(); }
    }

    // FAIL & COMPLETE
    public void Fail()
    {
        // the hack has failed :///
        Debug.Log($"Hack on {target.name} with exploit {name} was quit.");
        this.state = HackState.Failed;

        // we close the connection
        tunnel.Close();

        // Notify the target that the hack is failed
        target.OnHackFailed(this);
    }
    public void Complete()
    {
        // the hack is successful !!
        Debug.Log($"Hack on {target.name} with exploit {name} completed successfully.");
        this.progress = 100f;
        this.state = HackState.Completed;

        // we close the connection
        tunnel.Close();

        // Notify the target that the hack is completed
        target.OnHackCompleted(this);
    }
    public override void Overflow()
    {
        // the hack has overflowed :///
        Debug.LogWarning($"Hack on {target.name} with exploit {name} has overflowed. Freeing cores.");
        this.state = HackState.Overflowed;

        // we close the connection
        tunnel.Close();

        // Notify the target that the hack is failed
        target.OnHackFailed(this);
    }

    // GETTERS
    public float CalculateDuration(float duration_multiplier = 2.25f)
    {
        float duration = program.base_duration;
        int security_level_difference = target.SecurityLevel - (program as Exploit).security_level;

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




// PROGRAMS & EXPLOITS (FILES)

[System.Serializable]
public class Program : File
{
    [Header("Program Details")]
    public float base_duration;
    public int cores_cost;

    // CONSTRUCTOR
    public Program(string name, float base_duration, int cores_cost)
    {
        this.extension = ".exe"; // default extension for programs
        this.name = name;
        this.base_duration = base_duration;
        this.cores_cost = cores_cost;
    }
}

[System.Serializable]
public class Exploit : Program
{
    public static readonly Exploit Nmap = new Exploit("nmap", 1000, 0.1f, 1);
    public static readonly Exploit TypePassword = new Exploit("type_password", 1000, 0.1f, 1);

    [Header("Exploit Details")]
    public int security_level;

    // CONSTRUCTOR
    public Exploit(string name, int security_level, float base_duration, int cores_cost) : base(name, base_duration, cores_cost)
    {
        this.security_level = security_level;
    }
    public Exploit(Exploit exploit) : base(exploit.name, exploit.base_duration, exploit.cores_cost)
    {
        this.extension = exploit.extension;
        this.security_level = exploit.security_level;
    }
}

[System.Serializable]
public class FileExploit : Exploit
{
    public File file;
    public FileExploit(Exploit exploit, File file) : base(exploit)
    {
        this.file = file;
    }
}