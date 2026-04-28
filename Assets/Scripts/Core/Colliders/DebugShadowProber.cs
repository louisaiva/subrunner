using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DebugShadowProber : MonoBehaviour
{
    ShadowCaster2D sc;
    Collider2D col;
    string tag;

    public void Init(ShadowCaster2D sc, Collider2D col, string tag)
    {
        this.sc = sc;
        this.col = col;
        this.tag = tag;
    }

    IEnumerator Start()
    {
        LogState("immediate");
        yield return new WaitForEndOfFrame();
        LogState("endOfFrame1");
        yield return new WaitForEndOfFrame();
        LogState("endOfFrame2");
        Destroy(this);
    }

    void LogState(string phase)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"[ShadowProbe:{tag}] Env={(Application.isEditor ? "EDITOR" : "PLAYER")} frame={Time.frameCount} time={Time.realtimeSinceStartup:F3}");
        sb.AppendLine($" GameObject activeInHierarchy={gameObject.activeInHierarchy}");
        sb.AppendLine($" ShadowCaster enabled={(sc != null ? sc.enabled.ToString() : "null")} selfShadows={(sc != null ? sc.selfShadows.ToString() : "n/a")} castingOption={(sc != null ? sc.castingOption.ToString() : "n/a")}");
        var shape = sc?.shapePath;
        sb.AppendLine($" ShapePath length={(shape == null ? 0 : shape.Length)}");
        if (shape != null)
        {
            for (int i = 0; i < shape.Length; i++) sb.AppendLine($"  p{i}: {shape[i]}");
        }

        if (col != null)
        {
            sb.AppendLine($" Collider type={col.GetType().Name} offset={col.offset} localPos={col.transform.localPosition} worldPos={col.transform.position}");
            sb.AppendLine($" localScale={col.transform.localScale} lossyScale={col.transform.lossyScale}");
            if (col is BoxCollider2D b) sb.AppendLine($" size={b.size} bounds.center={b.bounds.center} bounds.extents={b.bounds.extents}");
            if (col is CircleCollider2D c) sb.AppendLine($" radius={c.radius} bounds.center={c.bounds.center} bounds.extents={c.bounds.extents}");
        }

        var p = transform.parent;
        sb.AppendLine($" Parent={(p != null ? p.name : "null")} parentLocalScale={(p != null ? p.localScale : Vector3.zero)} parentLossyScale={(p != null ? p.lossyScale : Vector3.zero)}");

        // get internal sorting layers if possible
        try
        {
            var fi = typeof(ShadowCaster2D).GetField("m_ApplyToSortingLayers", BindingFlags.Instance | BindingFlags.NonPublic);
            var arr = fi?.GetValue(sc) as int[];
            if (arr != null)
            {
                sb.Append(" SortingLayers:");
                foreach (var id in arr) sb.Append($"{SortingLayer.IDToName(id)},");
                sb.AppendLine("");
            }
        }
        catch { }

        Debug.Log(sb.ToString());
    }
}