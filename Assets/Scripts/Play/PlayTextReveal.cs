using UnityEngine;
using UnityEngine.UI;

public class PlayTextReveal : BaseMeshEffect
{
    const int VERTICES_PER_GLYPH = 4;
    const float TOTAL_REVEAL_SECONDS = 0.8f;
    const float GLYPH_SECONDS = 0.35f;
    const float MAX_STAGGER_SECONDS = 0.03f;
    const float RISE = 10f;
    const float SHIMMER_SECONDS = 0.7f;
    const float SHIMMER_WIDTH = 0.22f;
    const float SHIMMER_STRENGTH = 0.8f;

    readonly UIVertex[] quad = new UIVertex[VERTICES_PER_GLYPH];

    bool playing;
    float start_time;
    int glyph_count;

    public void play(float delay)
    {
        playing = true;
        start_time = Time.unscaledTime + Mathf.Max(0f, delay);
        if (graphic != null) graphic.SetVerticesDirty();
    }

    protected override void OnDisable()
    {
        playing = false;
        base.OnDisable();
    }

    void Update()
    {
        if (!playing) return;

        var elapsed = Time.unscaledTime - start_time;
        if (elapsed < 0f) return;

        if (elapsed >= reveal_seconds() + SHIMMER_SECONDS) playing = false;
        if (graphic != null) graphic.SetVerticesDirty();
    }

    float stagger_seconds()
    {
        return Mathf.Min(MAX_STAGGER_SECONDS, (TOTAL_REVEAL_SECONDS - GLYPH_SECONDS) / Mathf.Max(1, glyph_count - 1));
    }

    float reveal_seconds()
    {
        return stagger_seconds() * Mathf.Max(0, glyph_count - 1) + GLYPH_SECONDS;
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        glyph_count = vh.currentVertCount / VERTICES_PER_GLYPH;
        if (!IsActive() || !playing || glyph_count == 0) return;

        var elapsed = Time.unscaledTime - start_time;
        var stagger = stagger_seconds();
        var rect = graphic.rectTransform.rect;
        var shimmer_t = Mathf.Clamp01((elapsed - reveal_seconds()) / SHIMMER_SECONDS);
        var shimmer_center = Mathf.Lerp(-SHIMMER_WIDTH, 1f + SHIMMER_WIDTH, shimmer_t);
        var shimmer_active = elapsed >= reveal_seconds() && shimmer_t < 1f;
        Color32 tint = PlayTheme.gold_soft;

        for (var glyph = 0; glyph < glyph_count; glyph++)
        {
            var glyph_t = Mathf.Clamp01((elapsed - glyph * stagger) / GLYPH_SECONDS);
            var eased = 1f - (1f - glyph_t) * (1f - glyph_t) * (1f - glyph_t);

            var x_sum = 0f;
            for (var corner = 0; corner < VERTICES_PER_GLYPH; corner++)
            {
                vh.PopulateUIVertex(ref quad[corner], glyph * VERTICES_PER_GLYPH + corner);
                x_sum += quad[corner].position.x;
            }

            var shimmer = 0f;
            if (shimmer_active)
            {
                var position = Mathf.InverseLerp(rect.xMin, rect.xMax, x_sum / VERTICES_PER_GLYPH);
                shimmer = Mathf.Clamp01(1f - Mathf.Abs(position - shimmer_center) / SHIMMER_WIDTH) * SHIMMER_STRENGTH;
            }

            for (var corner = 0; corner < VERTICES_PER_GLYPH; corner++)
            {
                var vertex = quad[corner];
                var base_alpha = vertex.color.a;
                vertex.color = Color32.Lerp(vertex.color, tint, shimmer);
                vertex.color.a = (byte)Mathf.RoundToInt(base_alpha * eased);
                vertex.position.y -= RISE * (1f - eased);
                vh.SetUIVertex(vertex, glyph * VERTICES_PER_GLYPH + corner);
            }
        }
    }
}
