using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Paper : UI_Pool
{
    [Header("Paper parameters")]
    public Paper paper = null;
    public UI_Readable content = null;


    public void SetPaper(Paper paper)
    {
        this.paper = paper;
    
        GameObject content_go = Instantiate(paper.prefab, transform);
        if (content_go == null) { return; }
        RegisterToPool(content_go, is_stacked: true);

        content = content_go.GetComponent<UI_Readable>();
        TransitionSettings = content.transition_settings;
    }
    protected override IEnumerator show_coroutine(List<GameObject> dont_show = null, float duration_override = -1f)
    {

        // todo : faire un UI_Readable à la racine du content qui s'occupe d'afficher bien les bails sinon ça va etre le sbeul
        // todo : d'ailleurs remplacer UI_Paper par un UI_Reader

        content.gameObject.SetActive(true);
        content.Show(duration_override >= 0f ? duration_override : TransitionSettings.Duration);
        if (dont_show == null) { dont_show = new List<GameObject>(); }
        dont_show.Add(content.gameObject);

        // we transition to the ui_paper pool
        yield return base.show_coroutine(dont_show, duration_override);
    }
    protected override IEnumerator hide_coroutine(List<GameObject> dont_hide = null, float duration_override = -1f)
    {
        // we transition the content away
        content.Hide(duration_override >= 0f ? duration_override : TransitionSettings.Duration);
        if (dont_hide == null) { dont_hide = new List<GameObject>(); }
        dont_hide.Add(content.gameObject);

        // we transition away from the ui_paper pool
        yield return base.hide_coroutine(dont_hide, duration_override);

        // we remove the text of the paper
        if (paper != null && content != null)
        {
            QuitPool(content.gameObject);
            Destroy(content.gameObject);
            content = null;
            paper = null;
        }
    }
}