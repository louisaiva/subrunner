#if UNITY_EDITOR
using UnityEditor;
#endif

using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering;
// using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
#pragma warning disable 4014

public class PauseMenuBackgroundEffect : MonoBehaviour
{
    [Header("Background effect")]
    [SerializeField] protected Image bg;
    [SerializeField] protected Vector2Int bg_alpha_range = new Vector2Int(0, 245);

    // START
    private void Start()
    {
        // on set le bg alpha
        if (bg == null) { return; }
        bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, 0f);
    }

    // SHOW / HIDE
    public async Awaitable TransitionAlpha(bool show, float duration,float override_final_alpha = default)
    {
        float start_alpha = bg.color.a;
        float end_alpha = show ? bg_alpha_range.y / 255f : bg_alpha_range.x / 255f;

        if (override_final_alpha != default)
        {
            end_alpha = override_final_alpha;
        }

        // checks duration
        if (duration <= 0f)
        {
            bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, end_alpha);
            return;
        }

        // tween
        await Tween.Custom(start_alpha, end_alpha, duration: duration,
                onValueChange: ctx => bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, ctx), useUnscaledTime: true);
    }
    
    public float Alpha => bg.color.a;


#if UNITY_EDITOR
    [CustomEditor(typeof(PauseMenuBackgroundEffect))]
    public class UI_PauseMenuBackgroundEffectEditor : Editor
    {
        private bool bg_showed = false;
        public override void OnInspectorGUI()
        {
            PauseMenuBackgroundEffect effect = (PauseMenuBackgroundEffect)target;
            if (!bg_showed && GUILayout.Button("Show Background"))
            {
                effect.bg.color = new Color(effect.bg.color.r, effect.bg.color.g, effect.bg.color.b, effect.bg_alpha_range.y / 255f);
                bg_showed = true;
            }
            if (bg_showed && GUILayout.Button("Hide Background"))
            {
                effect.bg.color = new Color(effect.bg.color.r, effect.bg.color.g, effect.bg.color.b, effect.bg_alpha_range.x / 255f);
                bg_showed = false;
            }

            DrawDefaultInspector();
        }
    }
#endif
}