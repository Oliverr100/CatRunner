using Nova;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    public UIBlock2D RestartButton;
    public UIBlock2D ExitButton;

    public GameManager gameManager;

    [Header("Button feel (polish)")]
    public float hoverScale = 1.08f;
    public float pressScale = 0.94f;
    [Tooltip("How much lighter the button gets when hovered (0 - 1).")]
    public float hoverBrighten = 0.2f;
    public float feelSpeed = 16f;

    // small helper that animates one button towards its hover / press look
    private class ButtonFx
    {
        public UIBlock2D block;
        public Color baseColor;
        public Vector3 baseScale;
        public bool hovered, pressed;
        public float scale = 1f, bright = 0f;
    }

    private ButtonFx restartFx, exitFx;

    void Awake()
    {
        restartFx = Hook(RestartButton, OnRestartClick);
        exitFx = Hook(ExitButton, OnExitClick);
    }

    private ButtonFx Hook(UIBlock2D block, System.Action<Gesture.OnClick> onClick)
    {
        var fx = new ButtonFx { block = block, baseColor = block.Color, baseScale = block.transform.localScale };

        block.AddGestureHandler<Gesture.OnHover>(e => { fx.hovered = true; SfxPlayer.Play(ProceduralSfx.Hover, 0.35f); });
        block.AddGestureHandler<Gesture.OnUnhover>(e => { fx.hovered = false; fx.pressed = false; });
        block.AddGestureHandler<Gesture.OnPress>(e => { fx.pressed = true; });
        block.AddGestureHandler<Gesture.OnRelease>(e => { fx.pressed = false; });
        block.AddGestureHandler<Gesture.OnClick>(e => { SfxPlayer.Play(ProceduralSfx.Click, 0.7f); onClick(e); });
        return fx;
    }

    void Update()
    {
        Animate(restartFx);
        Animate(exitFx);
    }

    private void Animate(ButtonFx fx)
    {
        if (fx == null || fx.block == null) return;

        float targetScale = fx.pressed ? pressScale : (fx.hovered ? hoverScale : 1f);
        float targetBright = fx.hovered ? hoverBrighten : 0f;

        if (Mathf.Abs(fx.scale - targetScale) < 0.0005f && Mathf.Abs(fx.bright - targetBright) < 0.002f) return;

        float k = 1f - Mathf.Exp(-feelSpeed * Time.unscaledDeltaTime);   // unscaled: the game is frozen on this screen
        fx.scale = Mathf.Lerp(fx.scale, targetScale, k);
        fx.bright = Mathf.Lerp(fx.bright, targetBright, k);

        fx.block.transform.localScale = fx.baseScale * fx.scale;
        fx.block.Color = Color.Lerp(fx.baseColor, Color.white, fx.bright);
    }

    void OnRestartClick(Gesture.OnClick evt)
    {
        gameManager.RestartGame();
    }

    void OnExitClick(Gesture.OnClick evt)
    {
        gameManager.ExitGame();
    }
}
