using UnityEngine;

/// <summary>
/// Gives zombies and birds personality: a waddle / flap, a "!" and a startle when the cat gets
/// close, smooth turn-arounds (via SpriteFeel) and a little celebration when the cat dies.
/// Movement and collisions stay exactly as they were.
/// </summary>
[RequireComponent(typeof(SpriteFeel))]
public class EnemyFeel : MonoBehaviour
{
    public enum Kind { Auto, Zombie, Bird }

    public Kind kind = Kind.Auto;

    [Header("Notice the player")]
    public float alertRange = 6f;
    public bool alertSound = true;
    public Color alertColor = new Color(1f, 0.3f, 0.25f, 1f);

    private SpriteFeel fx;
    private Transform player;
    private bool alerted, cheering;
    private float cheerTime;

    private void Awake()
    {
        fx = GetComponent<SpriteFeel>();
        if (kind == Kind.Auto) kind = GetComponent<FlyingEnemyMovement>() != null ? Kind.Bird : Kind.Zombie;

        if (kind == Kind.Zombie)
        {
            // heavy, wobbly waddle that turns around slowly
            fx.swayAngle = 7f;
            fx.swaySpeed = 6.5f;
            fx.bobHeight = 0.07f;
            fx.bobSpeed = 13f;
            fx.bobHops = true;
            fx.turnDuration = 0.26f;
        }
        else
        {
            // flapping, drifting bird
            fx.pivotAtCenter = true;
            fx.pulseAmount = 0.2f;
            fx.pulseSpeed = 15f;
            fx.bobHeight = 0.14f;
            fx.bobSpeed = 3f;
            fx.swayAngle = 6f;
            fx.swaySpeed = 3.2f;
        }
    }

    private void OnEnable() { GameManager.PlayerDied += OnPlayerDied; }
    private void OnDisable() { GameManager.PlayerDied -= OnPlayerDied; }

    private void Start()
    {
        var gf = GameFeel.Instance;
        if (gf != null) player = gf.player;
    }

    private void Update()
    {
        if (alerted || player == null) return;

        float dx = Mathf.Abs(player.position.x - transform.position.x);
        float dy = Mathf.Abs(player.position.y - transform.position.y);
        if (dx < alertRange && dy < 5f)
        {
            alerted = true;
            fx.Punch(new Vector2(-0.22f, 0.38f));                      // startle
            var gf = GameFeel.Instance;
            if (gf != null) gf.Popup("!", fx.TopWorld + Vector3.up * 0.35f, alertColor, 7f, 0.4f, 0.8f);
            if (alertSound) SfxPlayer.Play(ProceduralSfx.Alert, 0.35f, kind == Kind.Bird ? 1.25f : 1f);
        }
    }

    private void OnPlayerDied()
    {
        if (cheering) return;
        cheering = true;
        // celebrate: bounce and rock
        fx.bobHeight = 0.3f;
        fx.bobSpeed = 17f;
        fx.bobHops = true;
        fx.swayAngle = 14f;
        fx.swaySpeed = 12f;
    }
}
