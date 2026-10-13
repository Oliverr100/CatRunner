using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pops the Game Over title and buttons in (scale overshoot, one after another) with a sound instead
/// of everything just appearing. Put it on the Game Over panel root. Uses unscaled time because the
/// game is frozen at that point. (The root itself is controlled by Nova, so its children are animated.)
/// </summary>
public class GameOverReveal : MonoBehaviour
{
    public float duration = 0.45f;
    public float startScale = 0.55f;
    public float stagger = 0.09f;
    public bool playSound = true;
    [Tooltip("Children to animate. Leave empty to animate all direct children except the overlay.")]
    public List<Transform> targets = new List<Transform>();

    private readonly List<Vector3> baseScales = new List<Vector3>();
    private bool captured;

    private void Capture()
    {
        if (captured) return;
        captured = true;
        if (targets.Count == 0)
        {
            foreach (Transform child in transform)
            {
                if (child.name.ToLower().Contains("overlay")) continue;
                targets.Add(child);
            }
        }
        foreach (var t in targets) baseScales.Add(t.localScale);
    }

    private void OnEnable()
    {
        Capture();

        // the panel is enabled once when the scene loads (before the game hides it); only react to the real Game Over
        var gm = FindObjectOfType<GameManager>();
        if (gm == null || !gm.IsGameOver) return;

        if (playSound) SfxPlayer.Play(ProceduralSfx.Pop, 0.6f);
        StopAllCoroutines();
        StartCoroutine(Reveal());
    }

    private IEnumerator Reveal()
    {
        float total = duration + stagger * Mathf.Max(0, targets.Count - 1);
        float t = 0f;
        while (t < total)
        {
            t += Time.unscaledDeltaTime;
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] == null) continue;
                float k = Mathf.Clamp01((t - i * stagger) / duration);
                targets[i].localScale = baseScales[i] * Mathf.LerpUnclamped(startScale, 1f, EaseOutBack(k));
            }
            yield return null;
        }
        for (int i = 0; i < targets.Count; i++)
            if (targets[i] != null) targets[i].localScale = baseScales[i];
    }

    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}
