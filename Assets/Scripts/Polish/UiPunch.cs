using UnityEngine;

/// <summary>
/// Pops a UI element (or any transform) bigger and lets it wobble back to its normal size.
/// Use UiPunch.Pop(someText) from code. Runs on unscaled time, so it also plays while the game is paused.
/// </summary>
public class UiPunch : MonoBehaviour
{
    private Vector3 baseScale = Vector3.one;
    private bool hasBase;
    private float t = 99f, amount, duration = 0.4f;

    public static UiPunch Pop(Component target, float amount = 0.3f, float duration = 0.4f)
    {
        if (target == null) return null;
        var p = target.GetComponent<UiPunch>();
        if (p == null) p = target.gameObject.AddComponent<UiPunch>();
        p.Play(amount, duration);
        return p;
    }

    private void Play(float a, float d)
    {
        if (!hasBase || t >= duration)
        {
            baseScale = transform.localScale;
            hasBase = true;
        }
        amount = a;
        duration = Mathf.Max(0.05f, d);
        t = 0f;
    }

    private void Update()
    {
        if (!hasBase || t >= duration) return;
        t += Time.unscaledDeltaTime;
        float k = Mathf.Clamp01(t / duration);
        float s = 1f + amount * Mathf.Exp(-5f * k) * Mathf.Cos(k * 14f);
        transform.localScale = k >= 1f ? baseScale : baseScale * s;
    }
}
