using System;
using UnityEngine;

/// <summary>
/// Small placeholder sound effects that are generated in code, so the polish works
/// without importing any audio files. Every effect is a simple synthesised waveform.
/// Swap them for real recordings any time by playing your own AudioClips instead
/// (SfxPlayer.Play(yourClip)).
/// </summary>
public static class ProceduralSfx
{
    private const int SampleRate = 44100;

    private static AudioClip _land, _click, _hover, _hit, _newBest, _alert, _step, _rustle, _fall, _pop, _splash;

    /// <summary>Soft thump when the cat lands.</summary>
    public static AudioClip Land => _land != null ? _land : (_land = Make("sfx_land", 0.16f, Sweep(160f, 55f, 7f, 0.25f, 1)));
    /// <summary>UI button click.</summary>
    public static AudioClip Click => _click != null ? _click : (_click = Make("sfx_click", 0.07f, Sweep(900f, 620f, 9f, 0f, 2)));
    /// <summary>UI button hover tick.</summary>
    public static AudioClip Hover => _hover != null ? _hover : (_hover = Make("sfx_hover", 0.05f, Sweep(1100f, 1300f, 8f, 0f, 3)));
    /// <summary>Crunchy impact when the cat gets hit.</summary>
    public static AudioClip Hit => _hit != null ? _hit : (_hit = Make("sfx_hit", 0.30f, Sweep(220f, 50f, 6f, 0.85f, 4)));
    /// <summary>Rising blip (Game Over panel pop-in).</summary>
    public static AudioClip Pop => _pop != null ? _pop : (_pop = Make("sfx_pop", 0.12f, Sweep(420f, 840f, 8f, 0f, 5)));
    /// <summary>Two-tone beep ("!" above an enemy).</summary>
    public static AudioClip Alert => _alert != null ? _alert : (_alert = Make("sfx_alert", 0.22f, Beeps()));
    /// <summary>Happy arpeggio for a new best score.</summary>
    public static AudioClip NewBest => _newBest != null ? _newBest : (_newBest = Make("sfx_newbest", 0.75f, Arpeggio(new[] { 523.25f, 659.25f, 783.99f, 1046.5f }, 0.1f)));
    /// <summary>Tiny footstep tick.</summary>
    public static AudioClip Step => _step != null ? _step : (_step = Make("sfx_step", 0.05f, NoiseBurst(9f, 0.25f, 7, 2.6f, false)));
    /// <summary>Leaves rustling.</summary>
    public static AudioClip Rustle => _rustle != null ? _rustle : (_rustle = Make("sfx_rustle", 0.28f, NoiseBurst(2.5f, 0.35f, 8, 1.1f, true)));
    /// <summary>Water splash (pit death).</summary>
    public static AudioClip Splash => _splash != null ? _splash : (_splash = Make("sfx_splash", 0.5f, NoiseBurst(3.5f, 0.45f, 9, 2.2f, false)));
    /// <summary>Falling whistle (pit death).</summary>
    public static AudioClip Fall => _fall != null ? _fall : (_fall = Make("sfx_fall", 0.6f, Sweep(720f, 110f, 3.5f, 0.05f, 6)));

    // ------------------------------------------------------------------ generators

    private static AudioClip Make(string name, float seconds, Func<float, float, float> generator)
    {
        int n = Mathf.CeilToInt(seconds * SampleRate);
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)SampleRate;
            float p = i / (float)n;
            float taper = Mathf.Clamp01((1f - p) * 30f);           // avoids a click at the very end
            data[i] = Mathf.Clamp(generator(t, p) * taper, -1f, 1f);
        }
        var clip = AudioClip.Create(name, n, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // sine that glides from f0 to f1, mixed with noise, with an exponential fade-out
    private static Func<float, float, float> Sweep(float f0, float f1, float decay, float noiseAmount, int seed)
    {
        float phase = 0f;
        var rng = new System.Random(seed);
        return (t, p) =>
        {
            float f = Mathf.Lerp(f0, f1, p);
            phase += 2f * Mathf.PI * f / SampleRate;
            float env = Mathf.Exp(-decay * p) * Mathf.Clamp01(p * 60f);
            float noise = (float)rng.NextDouble() * 2f - 1f;
            return (Mathf.Sin(phase) * (1f - noiseAmount) + noise * noiseAmount) * env * 0.9f;
        };
    }

    private static Func<float, float, float> Beeps()
    {
        float phase = 0f;
        return (t, p) =>
        {
            float seg = p < 0.5f ? p * 2f : (p - 0.5f) * 2f;
            float f = p < 0.5f ? 760f : 1010f;
            phase += 2f * Mathf.PI * f / SampleRate;
            float env = Mathf.Sin(Mathf.PI * Mathf.Clamp01(seg));
            return Mathf.Sign(Mathf.Sin(phase)) * 0.3f * env;
        };
    }

    private static Func<float, float, float> Arpeggio(float[] notes, float noteLength)
    {
        float phase = 0f;
        return (t, p) =>
        {
            int idx = Mathf.Min(notes.Length - 1, (int)(t / noteLength));
            float local = (t - idx * noteLength) / noteLength;
            phase += 2f * Mathf.PI * notes[idx] / SampleRate;
            float env = Mathf.Exp(-2.2f * local) * Mathf.Clamp01(local * 40f);
            return (Mathf.Sin(phase) + 0.35f * Mathf.Sin(phase * 2f)) * env * 0.45f;
        };
    }

    private static Func<float, float, float> NoiseBurst(float decay, float lowpass, int seed, float gain, bool highpass)
    {
        var rng = new System.Random(seed);
        float lp = 0f;
        return (t, p) =>
        {
            float n = (float)rng.NextDouble() * 2f - 1f;
            lp += (n - lp) * lowpass;
            float s = highpass ? (n - lp) : lp;
            float env = highpass ? Mathf.Pow(Mathf.Sin(Mathf.PI * p), 2f) : Mathf.Exp(-decay * p);
            return s * env * gain * Mathf.Clamp01(p * 60f);
        };
    }
}
