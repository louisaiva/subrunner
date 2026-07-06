using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// todo : renommer en UI_Reader
public class UI_Paper : UI_Pool
{
    [Header("Paper parameters")]
    public UI_Readable content = null;


    public void SetReadable(GameObject readable_prefab)
    {
        // if we already have a content we don't instantiate a new one (it won't appear anyway it means the transition was not done)
        if (content != null) { return; }

        GameObject content_go = Instantiate(readable_prefab, transform);
        if (content_go == null) { return; }
        // last_prefab = readable_prefab;
        RegisterToPool(content_go, is_stacked: true);

        content = content_go.GetComponent<UI_Readable>();
        // Settings = content.transition_settings;
        Settings = content.transi_settings;

        if (log) { Debug.Log("(UI_Reader) Set readable " + content.name); content.log = true; }
    }
    protected override IEnumerator show_coroutine(List<GameObject> dont_show = null, float duration_override = -1f, bool was_stacked = false)
    {
        // we check if we are stacking we simply do the virtual coroutine
        if (was_stacked) { yield return base.show_coroutine(dont_show, duration_override, was_stacked);  yield break; }

        content.gameObject.SetActive(true);
        content.Show(duration_override >= 0f ? duration_override : Settings.Duration);
        if (dont_show == null) { dont_show = new List<GameObject>(); }
        dont_show.Add(content.gameObject);

        // we transition to the ui_paper pool
        yield return base.show_coroutine(dont_show, duration_override, was_stacked);
    }
    protected override IEnumerator hide_coroutine(List<GameObject> dont_hide = null, float duration_override = -1f, bool stacking = false)
    {
        if (stacking)
        {
            // si on stacke on veut pas cacher tous les trucs donc on lance simplement la base
            yield return base.hide_coroutine(dont_hide, duration_override, stacking);
            yield break;
        }

        // sinon on veut vraiment cacher UI_Reader, on cache donc le UI_Readable puis on le supprime

        // we transition the content away
        float duration = duration_override >= 0f ? duration_override : Settings.Duration;
        content.Hide(duration);

        // we transition away from the ui_paper pool
        yield return base.hide_coroutine(dont_hide, duration_override, stacking);

        // we remove the text of the paper
        if (log) { Debug.Log("(UI_Reader) Cleaning readable " + content.name); }
        QuitPool(content.gameObject);
        Destroy(content.gameObject);
        content = null;
    }
}