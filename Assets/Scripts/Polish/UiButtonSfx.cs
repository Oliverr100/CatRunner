using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Hover / click sounds for a normal uGUI button (and optionally a scale animation: it grows when
/// hovered and shrinks when pressed). Add it next to the Button component. Turn "animateScale" off
/// when the button already has its own hover animation (like the main-menu buttons).
/// </summary>
public class UiButtonSfx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public float hoverScale = 1.08f;
    public float pressScale = 0.93f;
    public float speed = 16f;
    public bool animateScale = true;
    public bool playSounds = true;

    private Vector3 baseScale = Vector3.one;
    private float current = 1f, target = 1f;
    private bool hovered;

    private void Awake() { baseScale = transform.localScale; }

    private void OnEnable()
    {
        current = target = 1f;
        hovered = false;
        if (animateScale) transform.localScale = baseScale;
    }

    private void Update()
    {
        if (!animateScale || Mathf.Abs(current - target) < 0.0005f) return;
        current = Mathf.Lerp(current, target, 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime));
        transform.localScale = baseScale * current;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        hovered = true;
        target = hoverScale;
        if (playSounds) SfxPlayer.Play(ProceduralSfx.Hover, 0.35f);
    }

    public void OnPointerExit(PointerEventData e) { hovered = false; target = 1f; }
    public void OnPointerDown(PointerEventData e) { target = pressScale; }
    public void OnPointerUp(PointerEventData e) { target = hovered ? hoverScale : 1f; }

    public void OnPointerClick(PointerEventData e)
    {
        if (playSounds) SfxPlayer.Play(ProceduralSfx.Click, 0.7f);
    }
}
