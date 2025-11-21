using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_CreditsPool : UI_SlottablePool
{
    [Header("Credits Settings")]
    public float speed = 20f;

    [Header("Texts")]
    public RectTransform text_parent;
    public float max_y_position = 100f;
    public float base_y_position = 0f;

    // enable pool
    protected override IEnumerator show_coroutine(List<GameObject> dont_show = null, float duration_override = -1f, bool was_stacked = false)
    {
        // on reset la position du texte
        text_parent.anchoredPosition = new Vector2(
            text_parent.anchoredPosition.x,
            base_y_position
        );

        yield return base.show_coroutine(dont_show, duration_override, was_stacked);
    }

    // UPDATE
    protected void Update()
    {
        if (!Showed) { return; }

        // on calcule le prochain y
        float new_y = text_parent.anchoredPosition.y + speed * Time.unscaledDeltaTime;
        if (new_y >= max_y_position && speed > 0) { speed *= -1f; } // on inverse la vitesse si on dépasse
        if (new_y <= base_y_position && speed < 0) { speed *= -1f; } // on inverse la vitesse si on dépasse

        // on applique la nouvelle position
        text_parent.anchoredPosition = new Vector2(
            text_parent.anchoredPosition.x,
            new_y
        );
    }
}