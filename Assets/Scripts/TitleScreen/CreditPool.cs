using System.Collections;
using UnityEngine;

public class CreditPool : UI_SlottablePool
{
    [Header("Credits Settings")]
    public float speed = 20f;

    [Header("Texts")]
    public RectTransform text_parent;
    public float max_y_position = 100f;
    public float base_y_position = 0f;

    void Start()
    {
        slottable.GetButtonByName("exit_button").OnClick += () => UI_Manager.Instance.SwitchTo("home");
    }

    // enable pool
    protected override IEnumerator disable_coroutine()
    {
        yield return base.disable_coroutine();
        
        // on reset la position du texte
        text_parent.anchoredPosition = new Vector2(
            text_parent.anchoredPosition.x,
            base_y_position
        );
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