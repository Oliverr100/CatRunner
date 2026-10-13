using UnityEngine;

/// <summary>
/// Makes the world react: coins hover and spin, trees sway in the wind and rustle (with a few
/// falling leaves) when the cat passes. Colliders and tags are untouched.
/// </summary>
[RequireComponent(typeof(SpriteFeel))]
public class PropFeel : MonoBehaviour
{
    public enum Kind { Coin, Tree }

    public Kind kind = Kind.Coin;

    [Header("Tree rustle")]
    public float rustleDistance = 1.6f;
    public bool rustleSound = true;

    private SpriteFeel fx;
    private Transform player;
    private bool rustled;

    private void Awake()
    {
        fx = GetComponent<SpriteFeel>();
        if (kind == Kind.Coin)
        {
            fx.pivotAtCenter = true;
            fx.bobHeight = 0.1f;
            fx.bobSpeed = 3f;
            fx.spinSpeedY = 200f;
            fx.smoothTurn = false;
        }
        else
        {
            fx.swayAngle = 1.4f;
            fx.swaySpeed = 1.3f;
            fx.smoothTurn = false;
        }
    }

    private void Start()
    {
        var gf = GameFeel.Instance;
        if (gf != null) player = gf.player;
        if (kind == Kind.Coin) enabled = false;       // nothing to check per frame for coins
    }

    private void Update()
    {
        if (kind != Kind.Tree || rustled || player == null) return;

        if (Mathf.Abs(player.position.x - transform.position.x) < rustleDistance)
        {
            rustled = true;
            fx.PunchAngle(Random.value < 0.5f ? 260f : -260f);
            var gf = GameFeel.Instance;
            if (gf != null) gf.LeafPuff(fx.TopWorld + Vector3.down * 0.2f, 7);
            if (rustleSound) SfxPlayer.Play(ProceduralSfx.Rustle, 0.45f, Random.Range(0.9f, 1.1f));
        }
    }
}
