using System.Collections.Generic;
using UnityEngine;

public class UI_Paper : UI_Pool
{
    [Header("Paper parameters")]
    public Paper paper = null;
    public GameObject content = null;


    public void SetPaper(Paper paper)
    {
        this.paper = paper;
        content = Instantiate(paper.prefab, transform);
        if (content == null) { return; }

        ui_elements.Add(content);
    }

    public override async Awaitable Hide(float duration, List<GameObject> dont_hide = null)
    {
        // we remove the text of the paper
        if (paper != null && content != null)
        {
            ui_elements.Remove(content);
            Destroy(content);
            content = null;
            paper = null;
        }

        await base.Hide(duration, dont_hide);
    }
}