using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Fade to black between scenes, plus a quick full-screen colour flash (hit flash).
/// Creates itself on first use and survives scene loads.
/// </summary>
public class SceneFader : MonoBehaviour
{
    private static SceneFader instance;

    public float fadeOutTime = 0.3f;
    public float fadeInTime = 0.4f;

    private Image image;
    private bool loading;
    private Coroutine flashRoutine;

    /// <summary>Makes sure the fader exists. The first scene then fades in from black.</summary>
    public static void EnsureExists()
    {
        if (instance == null) Create(1f);
    }

    public static void Load(int buildIndex)
    {
        if (instance == null) Create(0f);
        instance.StartCoroutine(instance.LoadRoutine(buildIndex, null));
    }

    public static void Load(string sceneName)
    {
        if (instance == null) Create(0f);
        instance.StartCoroutine(instance.LoadRoutine(-1, sceneName));
    }

    /// <summary>Flashes the whole screen with a colour (peak alpha) and fades it away.</summary>
    public static void Flash(Color color, float peakAlpha, float duration)
    {
        if (instance == null) Create(0f);
        if (instance.loading) return;
        if (instance.flashRoutine != null) instance.StopCoroutine(instance.flashRoutine);
        instance.flashRoutine = instance.StartCoroutine(instance.FlashRoutine(color, peakAlpha, duration));
    }

    private static void Create(float startAlpha)
    {
        var go = new GameObject("SceneFader");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<SceneFader>();
        instance.Build(startAlpha);
    }

    private void Build(float startAlpha)
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        var imgGo = new GameObject("Fade");
        imgGo.transform.SetParent(transform, false);
        image = imgGo.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, startAlpha);
        image.raycastTarget = false;
        var rt = image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        SceneManager.sceneLoaded += OnSceneLoaded;
        if (startAlpha > 0f) StartCoroutine(Fade(Color.black, 0f, fadeInTime));
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this) instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllCoroutines();
        loading = false;
        image.raycastTarget = false;
        StartCoroutine(Fade(Color.black, 0f, fadeInTime));
    }

    private IEnumerator LoadRoutine(int index, string sceneName)
    {
        if (loading) yield break;
        loading = true;
        if (flashRoutine != null) { StopCoroutine(flashRoutine); flashRoutine = null; }
        image.raycastTarget = true;                       // block clicks while fading
        yield return Fade(Color.black, 1f, fadeOutTime);
        if (sceneName != null) SceneManager.LoadScene(sceneName);
        else SceneManager.LoadScene(index);
    }

    private IEnumerator FlashRoutine(Color color, float peak, float duration)
    {
        image.color = new Color(color.r, color.g, color.b, peak);
        yield return Fade(color, 0f, duration);
        flashRoutine = null;
    }

    private IEnumerator Fade(Color color, float toAlpha, float duration)
    {
        float from = image.color.a;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            image.color = new Color(color.r, color.g, color.b, Mathf.Lerp(from, toAlpha, k));
            yield return null;
        }
        image.color = new Color(color.r, color.g, color.b, toAlpha);
    }
}
