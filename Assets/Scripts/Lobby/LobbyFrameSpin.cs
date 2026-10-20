using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class LobbyFrameSpin : MonoBehaviour
{
    const string SHADER_PATH = "Shaders/UiFrameSpinLight";

    static Shader shader;

    Image target;
    Material material;
    RectTransform rect;
    float speed = 0.9f;
    float phase;

    public static LobbyFrameSpin attach(Image image, Color light_color, float speed, float phase, float intensity, bool as_glow)
    {
        if (image == null) return null;

        if (shader == null) shader = Resources.Load<Shader>(SHADER_PATH);
        if (shader == null || !shader.isSupported) return null;

        var spin = image.gameObject.AddComponent<LobbyFrameSpin>();
        spin.speed = speed;
        spin.phase = phase;
        spin.target = image;
        spin.rect = image.rectTransform;
        spin.material = new Material(shader);
        spin.material.SetColor("_LightColor", light_color);
        spin.material.SetFloat("_Intensity", intensity);
        spin.material.SetFloat("_AlphaMix", as_glow ? 1f : 0f);
        image.material = spin.material;
        return spin;
    }

    void LateUpdate()
    {
        if (material == null) return;

        var size = rect.rect.size;
        material.SetVector("_Size", new Vector4(size.x, size.y, 0f, 0f));
        material.SetFloat("_Angle", phase + Time.unscaledTime * speed);
    }

    void OnDestroy()
    {
        if (target != null) target.material = null;
        if (material != null) Destroy(material);
    }
}
