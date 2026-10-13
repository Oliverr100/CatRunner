using UnityEngine;

/// <summary>
/// "Juice" for any sprite: squash &amp; stretch springs, smooth turning, tilt, bobbing, swaying,
/// hit flash and spin.
///
/// How it works: at runtime the original SpriteRenderer is switched off and an identical
/// "visual copy" is created as a child. Only the copy is animated, so physics, colliders,
/// Animators and your own scripts keep working on the untouched original object.
/// Nothing is added to the scene while you edit (it all happens in play mode).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFeel : MonoBehaviour
{
    [Header("Squash & stretch spring")]
    public float stiffness = 240f;
    public float damping = 15f;

    [Header("Smooth turning (instead of an instant flip)")]
    public bool smoothTurn = true;
    public float turnDuration = 0.16f;

    [Header("Pivot")]
    [Tooltip("Off = squash/rotate around the feet (bottom of the sprite). On = around the centre.")]
    public bool pivotAtCenter = false;

    [Header("Idle motion")]
    [Tooltip("Breathing / flapping: stretches taller and thinner in a loop.")]
    public float pulseAmount = 0f;
    public float pulseSpeed = 3f;
    [Tooltip("Up-and-down motion in world units.")]
    public float bobHeight = 0f;
    public float bobSpeed = 3f;
    [Tooltip("On = |sin| (little hops, like footsteps). Off = smooth sine.")]
    public bool bobHops = false;
    [Tooltip("Rocking left and right in degrees.")]
    public float swayAngle = 0f;
    public float swaySpeed = 2f;
    [Tooltip("Fake 3D spin around the vertical axis (coins), degrees per second.")]
    public float spinSpeedY = 0f;
    [Tooltip("Flat spin in degrees per second (used for the death spin).")]
    public float spinSpeedZ = 0f;

    [Header("Tilt / rotation spring")]
    public float tiltSharpness = 14f;
    public float angleStiffness = 140f;
    public float angleDamping = 5f;

    private SpriteRenderer source;
    private SpriteRenderer proxy;
    private Transform pivot;
    private Vector3 pivotBaseLocal;
    private Material normalMaterial;
    private bool built;

    private Vector2 off, vel;           // scale spring (offset from 1 and velocity)
    private float angOff, angVel;       // rotation spring (degrees)
    private float tilt, tiltTarget;
    private float pulsePhase, bobPhase, swayPhase, spinY, spinZ;

    private bool initializedFacing, turning;
    private float shownFacing, turnT;

    private float flashTimer;
    private Color flashColor = Color.white;
    private bool wasFlashing;

    private static Material flashMaterial;
    private static bool flashLoaded;

    /// <summary>World position of the pivot (the feet by default).</summary>
    public Vector3 FeetWorld => transform.TransformPoint(pivotBaseLocal);

    /// <summary>World position of the top of the sprite (for floating "!" icons).</summary>
    public Vector3 TopWorld
    {
        get
        {
            float h = 0.5f;
            if (source != null && source.sprite != null)
                h = source.sprite.bounds.max.y * Mathf.Abs(transform.lossyScale.y);
            return new Vector3(transform.position.x, transform.position.y + h, transform.position.z);
        }
    }

    private static Material FlashMaterial
    {
        get
        {
            if (!flashLoaded)
            {
                flashLoaded = true;
                flashMaterial = Resources.Load<Material>("PolishFlash");
            }
            return flashMaterial;
        }
    }

    // ------------------------------------------------------------------ public API

    /// <summary>Adds an instant scale offset that springs back, e.g. (0.3, -0.3) = squash, (-0.2, 0.3) = stretch.</summary>
    public void Punch(Vector2 scaleOffset)
    {
        off = Vector2.ClampMagnitude(off + scaleOffset, 0.7f);
    }

    /// <summary>Kicks the rotation spring (degrees per second), e.g. to make a tree wobble.</summary>
    public void PunchAngle(float degreesPerSecond)
    {
        angVel += degreesPerSecond;
    }

    /// <summary>Target lean angle in degrees (smoothed).</summary>
    public void SetTilt(float degrees)
    {
        tiltTarget = degrees;
    }

    /// <summary>Paints the whole sprite in a flat colour for a moment (hit flash). Works in slow motion / hit-stop too.</summary>
    public void Flash(Color color, float duration = 0.1f)
    {
        flashColor = color;
        flashTimer = duration;
    }

    // ------------------------------------------------------------------ setup

    private void Start()
    {
        source = GetComponent<SpriteRenderer>();
        if (source == null) { enabled = false; return; }

        var anim = GetComponent<Animator>();
        if (anim != null) anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;   // keep animating while the original renderer is off

        // pivot (feet or centre) as a child of this object
        Vector3 feet = Vector3.zero;
        if (source.sprite != null)
        {
            Bounds b = source.sprite.bounds;
            feet = new Vector3(0f, pivotAtCenter ? b.center.y : b.min.y, 0f);
        }
        pivotBaseLocal = feet;

        var pivotGo = new GameObject("Visual (SpriteFeel)");
        pivot = pivotGo.transform;
        pivot.SetParent(transform, false);
        pivot.localPosition = pivotBaseLocal;

        var proxyGo = new GameObject("Sprite");
        proxyGo.transform.SetParent(pivot, false);
        proxyGo.transform.localPosition = -pivotBaseLocal;
        proxy = proxyGo.AddComponent<SpriteRenderer>();

        normalMaterial = source.sharedMaterial;
        proxy.sharedMaterial = normalMaterial;
        proxy.sprite = source.sprite;
        proxy.color = source.color;
        proxy.flipX = source.flipX;
        proxy.flipY = source.flipY;
        proxy.sortingLayerID = source.sortingLayerID;
        proxy.sortingOrder = source.sortingOrder;
        proxy.maskInteraction = source.maskInteraction;
        proxy.drawMode = SpriteDrawMode.Simple;

        // random start phases so neighbouring coins / trees do not move in lock-step
        float seed = Mathf.Abs(transform.position.x * 12.9898f + transform.position.y * 78.233f) % 6.2831f;
        pulsePhase = seed;
        bobPhase = seed * 1.7f;
        swayPhase = seed * 2.3f;
        spinY = seed * 57.3f;

        source.enabled = false;
        built = true;
    }

    private void OnEnable()
    {
        if (built)
        {
            source.enabled = false;
            proxy.enabled = true;
        }
    }

    private void OnDisable()
    {
        if (built)
        {
            if (source != null) source.enabled = true;
            if (proxy != null) proxy.enabled = false;
        }
    }

    // ------------------------------------------------------------------ per frame

    private void LateUpdate()
    {
        if (!built) return;

        float dt = Mathf.Min(Time.deltaTime, 0.05f);

        // --- mirror the original renderer
        proxy.sprite = source.sprite;
        proxy.flipY = source.flipY;
        proxy.sortingOrder = source.sortingOrder;

        // --- smooth turning
        float yawSign = transform.right.x >= 0f ? 1f : -1f;                       // enemies flip by rotating 180 degrees
        float orient = yawSign * (transform.lossyScale.x >= 0f ? 1f : -1f);
        float facing = (source.flipX ? -1f : 1f) * orient;                       // the cat flips via flipX
        if (!initializedFacing) { shownFacing = facing; initializedFacing = true; }

        float turnScale = 1f;
        if (smoothTurn)
        {
            if (!turning && !Mathf.Approximately(facing, shownFacing)) { turning = true; turnT = 0f; }
            if (turning)
            {
                turnT += dt / Mathf.Max(0.02f, turnDuration);
                if (turnT >= 0.5f) shownFacing = facing;
                turnScale = Mathf.Abs(turnT * 2f - 1f);                           // 1 -> 0 -> 1
                if (turnT >= 1f) { turning = false; turnScale = 1f; shownFacing = facing; }
            }
            else shownFacing = facing;
        }
        else shownFacing = facing;
        turnScale = Mathf.Max(0.05f, turnScale);
        proxy.flipX = shownFacing * orient < 0f;

        // --- springs
        Vector2 acc = -stiffness * off - damping * vel;
        vel += acc * dt;
        off += vel * dt;

        float angAcc = -angleStiffness * angOff - angleDamping * angVel;
        angVel += angAcc * dt;
        angOff += angVel * dt;

        tilt = Mathf.Lerp(tilt, tiltTarget, 1f - Mathf.Exp(-tiltSharpness * dt));

        // --- idle motion
        pulsePhase += pulseSpeed * dt;
        bobPhase += bobSpeed * dt;
        swayPhase += swaySpeed * dt;
        spinY += spinSpeedY * dt;
        spinZ += spinSpeedZ * dt;

        float pulse = pulseAmount * Mathf.Sin(pulsePhase);
        float spinScale = Mathf.Approximately(spinSpeedY, 0f) ? 1f : Mathf.Max(0.12f, Mathf.Abs(Mathf.Cos(spinY * Mathf.Deg2Rad)));

        float sx = (1f + off.x - pulse) * turnScale * spinScale;
        float sy = 1f + off.y + pulse;
        pivot.localScale = new Vector3(sx, sy, 1f);

        float bob = bobHeight * (bobHops ? Mathf.Abs(Mathf.Sin(bobPhase)) : Mathf.Sin(bobPhase));
        float bobLocal = bob / Mathf.Max(0.0001f, Mathf.Abs(transform.lossyScale.y));
        pivot.localPosition = pivotBaseLocal + new Vector3(0f, bobLocal, 0f);

        float rotZ = (tilt + Mathf.Sin(swayPhase) * swayAngle + angOff) * orient + spinZ;
        pivot.localRotation = Quaternion.Euler(0f, 0f, rotZ);

        // --- hit flash (uses unscaled time so it is visible during hit-stop)
        bool flashing = flashTimer > 0f;
        if (flashing) flashTimer -= Time.unscaledDeltaTime;

        if (flashing)
        {
            Material fm = FlashMaterial;
            if (fm != null)
            {
                proxy.sharedMaterial = fm;
                proxy.color = new Color(flashColor.r, flashColor.g, flashColor.b, source.color.a);
            }
            else
            {
                proxy.color = Color.Lerp(source.color, flashColor, 0.6f);        // fallback: tint
            }
        }
        else
        {
            if (wasFlashing) proxy.sharedMaterial = normalMaterial;
            proxy.color = source.color;
        }
        wasFlashing = flashing;
    }
}
