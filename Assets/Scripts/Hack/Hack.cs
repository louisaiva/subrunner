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
    public Vulnerable target => tunnel.destination.Vulnerable;
    public Exploit exploit => (Exploit)program;
    public HackCapacity hacker;

    // CONSTRUCTOR
    public Hack(Connection tunnel, Exploit exploit, HackCapacity hacker) : base(exploit)
    {
        this.tunnel = tunnel;
        this.hacker = hacker;
        this.progress = 0f;
    }

    // RUN , PROCESS & FINISH
    public override void Run(List<Core> cores)
    {
        // we use the cores
        provided_cores = cores;
        use_all_cores();

        // we calculate the duration
        this.duration = CalculateDuration();

        // open the tunnel
        tunnel.Open();

        // run the hack
        state = ProcessusState.Running;
        if (Logger.Instance.LOG_HACKS) { Debug.Log($"---> (Hack) on {target.capable.name} : {name} : started"); }
    }
    public override void Process()
    {
        // checks if the tunnel is still open
        if (tunnel.state != ConnectionState.Opened)
        {
            if (Logger.Instance.LOG_HACKS) { Debug.LogWarning($"---> (Hack) on {target.capable.name} : {name} : interrupted because the tunnel was closed"); }
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
        if (program is not Exploit exploit) { Fail(); return; }
        if (!target.IsVulnerableTo(exploit)) { Fail(); return; }




        // we check which type of exploit it is. if it's not a timer not a wait end we complete !
        if (exploit is not WaitExploit wait_exploit) { Complete(); return; }

        // Notify the target that the hack is completed
        target.OnHackSucceeded(this);

        // Notify the hacker we need to retract the hackray
        hacker.RetractHackray(this);

        // we free some cores until we only have the exploit.cores_cost_after_completion cores
        free_cores(provided_cores.Count - wait_exploit.cores_cost_after_exploit);

        // we check if it's a timer or a wait end
        if (wait_exploit is TimerExploit timer)
        {
            // we start a timer
            if (Logger.Instance.LOG_HACKS) { Debug.Log($"---> (Hack) on {target.capable.name} : {name} : is now in Timer mode for {timer.end_timer} seconds."); }
            duration = timer.end_timer;
        }
        else
        {
            if (Logger.Instance.LOG_HACKS) { Debug.Log($"---> (Hack) on {target.capable.name} : {name} : is now in Wait mode."); }
            duration = 0f;
        }
        progress = 100f;
        state = ProcessusState.Waiting;
    }

    // FAIL & COMPLETE
    public override void Fail()
    {

        // the hack has failed :///
        if (Logger.Instance.LOG_HACKS) { Debug.Log($"---> (Hack) on {target.capable.name} : {name} : failed."); }
        this.state = ProcessusState.Failed;

        // Notify the target that the hack is failed
        target.OnHackFailed(this);

        // terminate properly
        terminate();
    }
    public void Complete()
    {
        // the hack is successful !!
        if (Logger.Instance.LOG_HACKS) { Debug.Log($"---> (Hack) on {target.capable.name} : {name} : completed successfully."); }
        this.progress = 100f;
        this.state = ProcessusState.Completed;

        // Notify the target that the hack is completed
        target.OnHackSucceeded(this);

        // terminate properly
        terminate();
    }

    // WAITING
    public void Wait()
    {
        // checks if the tunnel is still open
        if (tunnel.state != ConnectionState.Opened)
        {
            if (Logger.Instance.LOG_HACKS) { Debug.LogWarning($"---> (Hack) on {target.capable.name} : {name} : was interrupted because the tunnel was closed"); }
            Fail();
            return;
        }

        // we update the progress of the hack
        if (exploit is TimerExploit) { progress -= Time.deltaTime / duration * 100f; }

        // we check if the hack is done
        if (progress <= 0f)
        {
            // we set the state to completed
            state = ProcessusState.Completed;
            terminate();
        }
    }
    private void terminate()
    {
        // we close the connection
        tunnel.Close();

        // we are done (no complete bcz we already hacked successfully the target)
        target.OnHackDone(this);

        // we free the cores
        free_all_cores();

        // we notify the hacker capacity that the hack is done so it can remove it from its list
        hacker.TerminateHack(this);
    }



    [Header("Files found")]
    public List<File> downloads = new List<File>();
    public void Download(File file)
    {
        if (downloads.Contains(file)) { return; }
        downloads.Add(file);
        if (Logger.Instance.LOG_HACKS) { Debug.Log($"---> (Hack) on {target.capable.name} : {name} : found file {file.name} and downloaded it."); }
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
        if (average_cores_speed <= 0) { Debug.LogError($"---> (Hack) on {target.capable.name} : {name} : has an invalid average cores speed: {average_cores_speed}"); }
        else { duration /= average_cores_speed; }

        // we apply the os speed modifier
        duration /= os_speed;

        return duration;
    }
}

