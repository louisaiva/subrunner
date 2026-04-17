using System.Collections;
using UnityEngine;

public class SitCapacity : Capacity
{
    private Sofa current_sofa;
    public Sofa CurrentSofa { get { return current_sofa;} }

    private Coroutine zap_coroutine;

    // Sit
    public void Sit(Sofa sofa)
    {
        current_sofa = sofa;

        // we disable the player's movements
        if (Capable is not Movable movable) { return; }
        if (!sofa.TryGetCapacity(out SortingCapacity sorting_capacity)) { Debug.LogError($"(SitCapacity) Sofa {sofa.ID} does not have a SortingCapacity"); return; }
        
        sorting_capacity.ReceiveMovable(movable, sofa.WorldSittingPosition);

        // we show the sofa UI
        UI_Manager.Instance.GetPool<UI_SofaPool>()?.ShowSofaUI(this);

        // we make the AnimPlayer play the sitting animation
        AnimPlayer.Play("idle_sit");
    }
    public void ExitSofa()
    {
        if (log) { Debug.Log($"(SitCapacity) Exiting sofa {current_sofa.ID}"); }
        if (current_sofa == null) { return; }

        // we enable the player's movements
        if (Capable is not Movable movable) { return; }
        if (!current_sofa.TryGetCapacity(out SortingCapacity sorting_capacity)) { Debug.LogError($"(SitCapacity) Sofa {current_sofa.ID} does not have a SortingCapacity"); return; }

        if (log) { Debug.Log($"(SitCapacity) Removing movable from sofa {current_sofa.ID}"); }
        sorting_capacity.RemoveMovable(movable, current_sofa.WorldStandingPosition);

        if (log) { Debug.Log($"(SitCapacity) Stopping animations for sofa {current_sofa.ID}"); }
        // we make the AnimPlayer play the sitting animation
        AnimPlayer.StopPlaying("idle_sit");
        AnimPlayer.StopPlaying("zap_tv");

        current_sofa = null;
    }

    // UPDATE
    private void Update()
    {
        if (current_sofa == null) { return; }

        // we do a little tv zap sometimes
        if (zap_coroutine == null && Time.time >= next_zap_time)
        {
            zap_coroutine = StartCoroutine(zap());
        }
    }


    // little tv zap sometimes
    private Vector2 zap_delay_range = new Vector2(5, 15);
    private float next_zap_time = 0;
    private IEnumerator zap()
    {
        AnimPlayer.Play("zap_tv");
        while (AnimPlayer.IsPlaying("zap_tv"))
        {
            yield return null;
        }

        next_zap_time = Time.time + Random.Range(zap_delay_range.x, zap_delay_range.y);
        zap_coroutine = null;
    }



    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data)
    {
        base.LoadData(data);
        
        current_sofa = null;
    }
    public override void UnloadData()
    {
        current_sofa = null;

        base.UnloadData();
    }
}