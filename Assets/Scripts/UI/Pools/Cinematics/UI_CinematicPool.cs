using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class UI_CinematicPool : UI_Pool
{
    private Dictionary<string, PlayableDirector> timelines;
    private PlayableDirector current_timeline;

    protected override void Awake()
    {
        base.Awake();
        timelines = new Dictionary<string, PlayableDirector>();
        foreach (PlayableDirector timeline in GetComponentsInChildren<PlayableDirector>(true))
        {
            timelines.Add(timeline.gameObject.name, timeline);
            timeline.gameObject.SetActive(false);
        }
    }

    public void PlayCinematic(string cinematic_id)
    {
        if (!timelines.TryGetValue(cinematic_id, out PlayableDirector timeline))
        {
            Debug.LogError("(UI_CinematicPool) No timeline found with id " + cinematic_id);
            return;
        }
        current_timeline = timeline;

        // then we enable ourself so the timeline will play automatically
        UI_Manager.Instance.SwitchTo(pool_name: this.Reference, force: true);
    }

    protected override void before_adding_to_stack()
    {
        GameManager.State = GameState.Cinematic;

        // we play the timeline if we have one
        if (current_timeline != null)
        {
            current_timeline.gameObject.SetActive(true);
            current_timeline.Play();
        }
    }
    protected override void after_removed_from_stack()
    {
        GameManager.State = GameState.Gaming;

        // we stop the timeline if we have one
        if (current_timeline != null)
        {
            current_timeline.Stop();
            current_timeline.gameObject.SetActive(false);
            current_timeline = null;
        }

    }
}