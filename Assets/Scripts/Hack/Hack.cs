using System.Collections.Generic;
using UnityEngine;

// HACKS (PROCESSUS)

[System.Serializable]
public class Hack : Processus
{
    [Header("Hack Details")]
    public float progress;
    public Connection tunnel;
    public float duration;
    public HackState state = HackState.NotStarted;
    public Vulnerable target => tunnel.target;

    // CONSTRUCTOR
    public Hack(Connection tunnel, Exploit exploit) : base(exploit)
    {
        this.tunnel = tunnel;
        this.progress = 0f;
    }

    // RUN , PROCESS & FINISH
    public override void Run()
    {
        // we calculate the duration
        this.duration = CalculateDuration();

        // open the tunnel
        tunnel.Open();

        // run the hack
        state = HackState.Running;
        Debug.Log($"Starting hack on {target.capable.name} with exploit {name}");
    }
    public override void Process()
    {
        // checks if the tunnel is still open
        if (tunnel.state != ConnectionState.Opened)
        {
            Debug.LogWarning($"Hack on {target.capable.name} with exploit {name} was interrupted because the tunnel was closed");
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

        // we check if we failed or not
        if (name != "nmap" && !target.IsVulnerableTo(Exploit.Nmap)) { Fail(); return; }
        if (program is not Exploit exploit) { Fail(); return; }
        if (!target.IsVulnerableTo(exploit)) { Fail(); return; }




        // we check which type of exploit it is. if it's not a timer not a wait end we complete !
        if (exploit.end_timer == 0f && !exploit.wait_end) { Complete(); return; }


        // Notify the target that the hack is completed
        target.OnHackSucceeded(this);

        // we check if it's a timer or a wait end
        if (exploit.end_timer > 0f)
        {
            // we start a timer
            Debug.Log($"Hack on {target.capable.name} with exploit {name} is now in Timer mode for {exploit.end_timer} seconds.");
            progress = 100f;
            duration = exploit.end_timer;
            state = HackState.Waiting; // we set the state to waiting while the timer is running
        }
        else if (exploit.wait_end)
        {
            progress = 100f;
            duration = 0f;
            state = HackState.Waiting; // we set the state to waiting while the user is expected to end the exploit
        }
    }

    // FAIL & COMPLETE
    public void Fail()
    {
        // the hack has failed :///
        Debug.Log($"Hack on {target.capable.name} with exploit {name} was quit.");
        this.state = HackState.Failed;

        // we close the connection
        tunnel.Close();

        // Notify the target that the hack is failed
        target.OnHackFailed(this);
        target.OnHackDone(this);
    }
    public void Complete()
    {
        // the hack is successful !!
        Debug.Log($"Hack on {target.capable.name} with exploit {name} completed successfully.");
        this.progress = 100f;
        this.state = HackState.Completed;

        // we close the connection
        tunnel.Close();

        // Notify the target that the hack is completed
        target.OnHackSucceeded(this);
        target.OnHackDone(this);
    }
    public override void Overflow()
    {
        // the hack has overflowed :///
        Debug.LogWarning($"Hack on {target.capable.name} with exploit {name} has overflowed. Freeing cores.");
        this.state = HackState.Overflowed;

        // we close the connection
        tunnel.Close();

        // Notify the target that the hack is failed
        target.OnHackFailed(this);
        target.OnHackDone(this);
    }


    // WAITING
    public void Wait()
    {
        // checks if the tunnel is still open
        if (tunnel.state != ConnectionState.Opened)
        {
            Debug.LogWarning($"Hack on {target.capable.name} with exploit {name} was interrupted because the tunnel was closed");
            Fail();
            return;
        }

        // we update the progress of the hack
        Exploit exploit = program as Exploit;
        if (exploit.end_timer > 0f) { progress -= Time.deltaTime / duration * 100f; }
        else if (!exploit.wait_end) { progress = 0f; } // we set progress at 0f if we don't need to wait anymore

        // we check if the hack is done
        if (progress <= 0f) { terminate(); }
    }
    private void terminate()
    {
        // we close the connection
        tunnel.Close();

        // we are done (no complete bcz we already hacked successfully the target)
        target.OnHackDone(this);

        // we set the state to completed
        state = HackState.Completed;
    }



    [Header("Files found")]
    public List<File> downloads = new List<File>();
    public void Download(File file)
    {
        if (downloads.Contains(file)) { return; }
        downloads.Add(file);
        Debug.Log($"(Hack) {name} found file {file.name} on {target.capable.name} and downloaded it.");
    }
    
    // GETTERS
    public float CalculateDuration()
    {
        float duration = program.base_duration;
        int security_level_difference = target.SecurityLevel - (program as Exploit).security_level;

        // checks if the difference is > 100
        if (security_level_difference > 100) { return 1000f; }
        else if (security_level_difference < -1000) { return 0.1f; }

        // multiply the duration by multiplier once for each security level difference
        float security_multiplier = 2.25f;
        if (security_level_difference > 0) { security_multiplier = 1 / security_multiplier; }
        for (int i = 0; i < security_level_difference; ++i)
        {
            duration *= security_multiplier;
        }

        // we apply the cores speed modifier
        if (average_cores_speed <= 0) { Debug.LogError($"(Hack) {name} has an invalid average cores speed: {average_cores_speed}"); }
        else { duration /= average_cores_speed; }

        // we apply the os speed modifier
        duration /= os_speed;

        return duration;
    }
}

public enum HackState
{
    NotStarted,
    Running,
    Completed,
    Failed,
    Overflowed,
    Waiting, // for WaitEnd & Timer exploits
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
    public float end_timer = 0f; // if > 0f it will make the exploit a Timer exploit
    public bool wait_end = false; // if true it will make the exploit a WaitEnd exploit (and will wait until the "wait_end" bool become false again)

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

[System.Serializable]
public class DamageExploit : Exploit
{
    public float damage;
    public float knockback_magnitude;

    public DamageExploit(string name, int security_level, float base_duration, int cores_cost, float damage, float knockback_magnitude) : base(name, security_level, base_duration, cores_cost)
    {
        this.damage = damage;
        this.knockback_magnitude = knockback_magnitude;
    }
}