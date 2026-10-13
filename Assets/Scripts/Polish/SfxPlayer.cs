using UnityEngine;

/// <summary>
/// Plays short sound effects from a small pool of AudioSources, with an optional pitch,
/// so effects can overlap and be pitch-shifted (e.g. the rising coin combo) without
/// touching your other AudioSources. Created automatically on first use.
/// </summary>
public class SfxPlayer : MonoBehaviour
{
    private const int PoolSize = 10;

    private static SfxPlayer instance;
    private AudioSource[] sources;
    private int next;

    /// <summary>Master volume for everything played through SfxPlayer.</summary>
    public static float MasterVolume = 0.8f;

    private static SfxPlayer Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("SfxPlayer");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<SfxPlayer>();
                instance.Init();
            }
            return instance;
        }
    }

    private void Init()
    {
        sources = new AudioSource[PoolSize];
        for (int i = 0; i < PoolSize; i++)
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.spatialBlend = 0f;
            sources[i] = s;
        }
    }

    public static void Play(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        var inst = Instance;
        var s = inst.sources[inst.next];
        inst.next = (inst.next + 1) % inst.sources.Length;
        s.pitch = pitch;
        s.PlayOneShot(clip, volume * MasterVolume);
    }

    /// <summary>Plays the clip with a little random pitch so repeated sounds do not feel robotic.</summary>
    public static void PlayVaried(AudioClip clip, float volume = 1f, float basePitch = 1f, float variation = 0.08f)
    {
        Play(clip, volume, basePitch + Random.Range(-variation, variation));
    }
}
