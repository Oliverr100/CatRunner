using TMPro;
using UnityEngine;

/// <summary>
/// A short-lived world-space text ("+1", "!", "NEW BEST!") that pops in, floats up and fades out.
/// Pooled and driven by GameFeel.Popup().
/// </summary>
public class FloatingText : MonoBehaviour
{
    private TextMeshPro tmp;
    private float t, life, rise, baseScale;
    private Vector3 start;
    private Color color;

    public static FloatingText Create(Transform parent)
    {
        var go = new GameObject("FloatingText");
        go.transform.SetParent(parent, false);
        var ft = go.AddComponent<FloatingText>();
        ft.tmp = go.AddComponent<TextMeshPro>();
        ft.tmp.alignment = TextAlignmentOptions.Center;
        ft.tmp.enableWordWrapping = false;
        ft.tmp.fontStyle = FontStyles.Bold;
        ft.tmp.sortingOrder = 100;
        // Dark outline keeps the text readable on the bright sky. It lives in a material asset
        // (Resources/PolishTextOutline) so the outline shader variant is included in WebGL builds.
        var outlined = Resources.Load<Material>("PolishTextOutline");
        if (outlined != null) ft.tmp.fontSharedMaterial = outlined;
        go.SetActive(false);
        return ft;
    }

    public bool IsPlaying => gameObject.activeSelf;

    public void Play(string text, Vector3 worldPos, Color c, float size, float riseHeight, float lifetime)
    {
        tmp.text = text;
        tmp.fontSize = size;
        color = c;
        tmp.color = c;
        start = worldPos;
        rise = riseHeight;
        life = Mathf.Max(0.1f, lifetime);
        t = 0f;
        baseScale = 1f;
        transform.position = start;
        transform.localScale = Vector3.one * 0.3f;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        t += Time.unscaledDeltaTime;
        float k = t / life;
        if (k >= 1f) { gameObject.SetActive(false); return; }

        // pop-in (overshoot) then settle
        float s;
        if (k < 0.12f) s = Mathf.Lerp(0.3f, 1.3f, k / 0.12f);
        else if (k < 0.25f) s = Mathf.Lerp(1.3f, 1f, (k - 0.12f) / 0.13f);
        else s = 1f;
        transform.localScale = Vector3.one * (s * baseScale);

        // ease-out rise
        float e = 1f - Mathf.Pow(1f - k, 3f);
        transform.position = start + Vector3.up * (rise * e);

        // fade over the last 40 %
        float a = k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f;
        tmp.color = new Color(color.r, color.g, color.b, color.a * a);
    }
}
