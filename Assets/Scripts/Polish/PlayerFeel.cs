using UnityEngine;

/// <summary>
/// All the "feel" for the cat: squash &amp; stretch on jump / landing, running dust and footsteps,
/// leaning into the run and the jump arc, skid dust when turning around, and the hit reaction
/// (flash, spin, particles, camera shake, zoom, sound). Gameplay itself stays in PlayerMovement.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(SpriteFeel))]
public class PlayerFeel : MonoBehaviour
{
    [Header("Jump / landing squash & stretch (x = width, y = height)")]
    public Vector2 jumpPunch = new Vector2(-0.22f, 0.34f);
    public Vector2 landPunch = new Vector2(0.34f, -0.36f);

    [Header("Running")]
    public float stepInterval = 0.27f;
    public bool footstepSound = true;
    [Range(0f, 1f)] public float footstepVolume = 0.14f;
    public Vector2 stepPunch = new Vector2(0.05f, -0.06f);

    [Header("Leaning")]
    [Tooltip("Degrees the cat leans forward while running.")]
    public float runLean = 5f;
    [Tooltip("Degrees of nose-up / nose-down per unit of vertical speed in the air.")]
    public float airTilt = 2.2f;

    [Header("Hit reaction")]
    public float deathSpin = 540f;
    public bool screenFlashOnHit = true;

    private PlayerMovement pm;
    private SpriteFeel fx;
    private Rigidbody2D rb;
    private bool wasGrounded;
    private float lastVy, lastInput, stepTimer;

    private void Awake()
    {
        pm = GetComponent<PlayerMovement>();
        fx = GetComponent<SpriteFeel>();
        rb = GetComponent<Rigidbody2D>();

        // gentle breathing so the cat is never perfectly still
        fx.pulseAmount = 0.02f;
        fx.pulseSpeed = 3.5f;
    }

    private void OnEnable()
    {
        pm.Jumped += OnJumped;
        pm.Died += OnDied;
    }

    private void OnDisable()
    {
        pm.Jumped -= OnJumped;
        pm.Died -= OnDied;
    }

    private void Start()
    {
        wasGrounded = pm.IsGrounded;
    }

    private void LateUpdate()
    {
        if (pm.IsDead) return;

        var gf = GameFeel.Instance;
        bool grounded = pm.IsGrounded;
        float vy = rb.velocity.y;
        float input = pm.MoveInput;
        float facingSign = pm.FacingLeft ? -1f : 1f;
        Vector3 feet = fx.FeetWorld;

        // landing
        if (grounded && !wasGrounded && lastVy < -3f)
        {
            float k = Mathf.InverseLerp(3f, 12f, -lastVy);           // 0 = tiny hop, 1 = long fall
            fx.Punch(landPunch * (0.5f + k));
            SfxPlayer.Play(ProceduralSfx.Land, 0.35f + 0.5f * k, Random.Range(0.92f, 1.08f));
            if (gf != null)
            {
                gf.LandDust(feet, 0.5f + k);
                gf.ShakeSoft(0.4f + k);
            }
        }

        // running: dust + footsteps, skid dust when reversing
        if (grounded && Mathf.Abs(input) > 0.1f)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                stepTimer = stepInterval;
                fx.Punch(stepPunch);
                if (footstepSound) SfxPlayer.PlayVaried(ProceduralSfx.Step, footstepVolume, 1f, 0.15f);
                if (gf != null) gf.RunDust(feet, Mathf.Sign(input));
            }
            if (Mathf.Abs(lastInput) > 0.1f && Mathf.Sign(input) != Mathf.Sign(lastInput) && gf != null)
            {
                gf.RunDust(feet, Mathf.Sign(lastInput));
                gf.RunDust(feet, Mathf.Sign(lastInput));
            }
        }
        else
        {
            stepTimer = 0.05f;                                       // first step comes quickly after starting to run
        }

        // leaning: forward when running, nose up / down in the air
        float target = grounded
            ? -Mathf.Abs(input) * runLean * facingSign
            : Mathf.Clamp(vy * airTilt, -16f, 16f) * facingSign;
        fx.SetTilt(target);

        wasGrounded = grounded;
        lastVy = vy;
        lastInput = input;
    }

    private void OnJumped()
    {
        fx.Punch(jumpPunch);
        var gf = GameFeel.Instance;
        if (gf != null) gf.JumpDust(fx.FeetWorld);
    }

    private void OnDied(PlayerMovement.DeathCause cause, Vector3 point)
    {
        bool fell = cause == PlayerMovement.DeathCause.Fall;

        fx.Flash(Color.white, 0.14f);
        fx.Punch(new Vector2(0.45f, -0.3f));
        if (!fell) fx.spinSpeedZ = (pm.FacingLeft ? 1f : -1f) * deathSpin;
        fx.SetTilt(0f);
        fx.pulseAmount = 0f;

        if (!fell) SfxPlayer.Play(ProceduralSfx.Hit, 0.7f, 1f);

        var gf = GameFeel.Instance;
        if (gf == null) return;

        gf.DuckMusic();
        gf.ZoomPunch(-0.6f, 0.45f);
        if (fell)
        {
            SfxPlayer.Play(ProceduralSfx.Splash, 0.7f, 1f);
            gf.Splash(point);
            gf.ShakeHard(0.35f);
            gf.FreezeCamera();
        }
        else
        {
            gf.HitBurst(point);
            gf.ShakeHard(1f);
            if (screenFlashOnHit) SceneFader.Flash(new Color(1f, 1f, 1f), 0.45f, 0.25f);
        }
    }
}
