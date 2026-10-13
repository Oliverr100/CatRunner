using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // POLISH: why the cat died - lets the feedback differ (hit vs. falling into a pit)
    public enum DeathCause { Enemy, Obstacle, Wall, Fall }

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Ground Check")]
    public bool isGrounded;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask whatIsGround;

    [Header("Wall Crash Check")]
    public float wallCheckDistance = 0.2f;

    [Header("Pit Death (polish)")]
    [Tooltip("The cat dies when it falls below this height (the ground top is at y = -1). The original waited until y = -10, long after the cat left the screen.")]
    public float fallDeathY = -3.5f;

    [Header("Death Reaction (polish)")]
    [Tooltip("Knock-back speed (x, y) of the death hop when the cat is hit by an enemy / tree / wall.")]
    public Vector2 deathHop = new Vector2(2.5f, 6.5f);

    [Header("Audio")]
    public AudioSource catAudioSource;
    public AudioClip jumpSound;
    public AudioClip deathSound;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    [SerializeField] private bool isFacingLeft;

    private GameManager gameManager;
    private bool isDead = false;

    // POLISH: read-only state + events for the feel scripts (PlayerFeel)
    public bool IsGrounded => isGrounded;
    public bool IsDead => isDead;
    public bool FacingLeft => isFacingLeft;
    public float MoveInput { get; private set; }
    public event Action Jumped;
    public event Action<DeathCause, Vector3> Died;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (isDead) return;

        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        }

        float moveInput = Input.GetAxisRaw("Horizontal");
        MoveInput = moveInput;
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        if (moveInput <= -1)
        {
            isFacingLeft = true;
        }
        else if (moveInput >= 1)
        {
            isFacingLeft = false;
        }
        sr.flipX = isFacingLeft;

        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(moveInput));
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            if (anim != null) anim.SetTrigger("Jump");

            if (catAudioSource != null && jumpSound != null)
            {
                catAudioSource.PlayOneShot(jumpSound);
            }

            Jumped?.Invoke();
        }

        if (Mathf.Abs(moveInput) > 0)
        {
            Vector2 lookDirection = isFacingLeft ? Vector2.left : Vector2.right;

            RaycastHit2D wallHit = Physics2D.Raycast(transform.position, lookDirection, wallCheckDistance, whatIsGround);

            if (wallHit.collider != null)
            {
                Die(DeathCause.Wall, wallHit.point);
            }
        }

        if (transform.position.y < fallDeathY)
        {
            Die(DeathCause.Fall, transform.position);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("coins"))
        {
            if (gameManager != null)
            {
                gameManager.AddScore(collision.transform.position);   // POLISH: tells the game where the coin was
            }
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Die(DeathCause.Obstacle, transform.position);
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Die(DeathCause.Enemy, transform.position);
        }
    }

    private void Die(DeathCause cause, Vector3 hitPoint)
    {
        if (isDead) return;
        isDead = true;

        if (deathSound != null)
        {
            GameObject ghostSpeaker = new GameObject("GhostSpeaker");
            DontDestroyOnLoad(ghostSpeaker);

            AudioSource source = ghostSpeaker.AddComponent<AudioSource>();
            source.clip = deathSound;
            source.Play();

            Destroy(ghostSpeaker, deathSound.length);
        }

        // POLISH: instead of freezing on the spot, the cat is knocked back, hops and falls through the ground.
        if (cause != DeathCause.Fall)
        {
            foreach (var c in GetComponents<Collider2D>()) c.enabled = false;
            float dir = isFacingLeft ? 1f : -1f;
            rb.velocity = new Vector2(dir * deathHop.x, deathHop.y);
        }
        else
        {
            // POLISH: into the sea - the cat sinks slowly instead of plunging out of sight
            rb.velocity = new Vector2(0f, -1.5f);
            rb.gravityScale = 0.15f;
        }
        if (anim != null) anim.SetFloat("Speed", 0f);

        Died?.Invoke(cause, hitPoint);

        if (gameManager != null)
        {
            gameManager.TriggerGameOver();
        }
    }
}
