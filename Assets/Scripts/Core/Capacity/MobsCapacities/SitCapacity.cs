using System.Collections;
using UnityEngine;

public class SitCapacity : Capacity
{

    // todo : make this work with Container rather than Sofa, so we can use it for anything,
    // like chairs, beds toilets etc
    private Sofa current_sofa;
    public Sofa CurrentSofa { get { return current_sofa;} }
    public bool IsSitting { get { return current_sofa != null; } }

    private Coroutine zap_coroutine;
    private Coroutine sit_stand_coroutine;



    // SIT & STAND
    public void Sit(Sofa sofa, bool instant = false)
    {
        current_sofa = sofa;

        // we disable the player's movements
        if (Capable is not Movable movable) { return; }
        if (!sofa.TryGetCapacity(out SortingCapacity sorting_capacity)) { Debug.LogError($"(SitCapacity) Sofa {sofa.ID} does not have a SortingCapacity"); return; }
        
        sorting_capacity.ReceiveMovable(movable, sofa.WorldSittingPosition);

        // we show the sofa UI if we are on hud
        if (GameManager.State == GameState.Gaming)
        {
            UI_Manager.Instance.GetPool<UI_SofaPool>()?.ShowSofaUI(this);
        }

        // sit instantly (when loading game)
        if (instant)
        {
            sitInstantly();
            return;
        }

        // else we play the sit animation
        StopAllCoroutines();
        sit_stand_coroutine = StartCoroutine(sitCoroutine());
    }
    public void ExitSofa()
    {
        StopAllCoroutines();
        sit_stand_coroutine = StartCoroutine(standCoroutine());
    }


    // SIT & STAND COROUTINES
    private IEnumerator sitCoroutine()
    {
        // we play the sit anim
        AnimPlayer.Play("sit");
        while (AnimPlayer.IsPlaying("sit"))
        {
            yield return null;
        }

        // and the zip one
        AnimPlayer.Play("unzip");
        while (AnimPlayer.IsPlaying("unzip"))
        {
            yield return null;
        }

        // finally we make the AnimPlayer play the idle sit animation
        AnimPlayer.Play("idle_sit");
        sit_stand_coroutine = null;
    }
    private IEnumerator standCoroutine(bool add_force = false)
    {
        if (log) { Debug.Log($"(SitCapacity) Exiting sofa {current_sofa.ID}"); }
        if (current_sofa == null) { yield break; }
        if (Capable is not Movable movable) { yield break; }

        // and the zip one
        AnimPlayer.Play("zip");
        while (AnimPlayer.IsPlaying("zip"))
        {
            yield return null;
        }

        if (!current_sofa.TryGetCapacity(out SortingCapacity sorting_capacity)) { Debug.LogError($"(SitCapacity) Sofa {current_sofa.ID} does not have a SortingCapacity"); yield break; }

        // we play the stand anim
        AnimPlayer.Play("stand");
        while (AnimPlayer.IsPlaying("stand"))
        {
            yield return null;
        }


        // we remove the movable from the sofa's sorting capacity
        if (log) { Debug.Log($"(SitCapacity) Removing movable from sofa {current_sofa.ID}"); }
        sorting_capacity.RemoveMovable(movable, current_sofa.WorldStandingPosition);

        if (log) { Debug.Log($"(SitCapacity) Stopping animations for sofa {current_sofa.ID}"); }
        // we stop the animations
        AnimPlayer.StopPlaying("idle_sit");
        AnimPlayer.StopPlaying("zap_tv");
        AnimPlayer.StopPlaying("stand");

        // we put a little force on the player
        movable.Orientation = Vector2.down;
        if (add_force) { movable.AddForce(new Force("stand_from_sofa", Vector2.down, 60f)); }

        current_sofa = null;
        sit_stand_coroutine = null;
    }
    private void sitInstantly()
    {
        // we make the AnimPlayer play the idle sit animation
        AnimPlayer.AddToPile("idle_sit");
        sit_stand_coroutine = null;
    }




    // UPDATE
    private void Update()
    {
        if (current_sofa == null) { return; }
        if (sit_stand_coroutine != null) { return; }

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