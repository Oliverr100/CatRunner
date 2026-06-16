using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
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
        }

        if (Mathf.Abs(moveInput) > 0)
        {
            Vector2 lookDirection = isFacingLeft ? Vector2.left : Vector2.right;

            RaycastHit2D wallHit = Physics2D.Raycast(transform.position, lookDirection, wallCheckDistance, whatIsGround);

            if (wallHit.collider != null)
            {
                Die();
            }
        }

        if (transform.position.y < -10f)
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("coins"))
        {
            if (gameManager != null)
            {
                gameManager.AddScore();
            }
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Hit a tree!");
            Die();
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Hit an active Enemy! Ouch!");
            Die();
        }
    }

    private void Die()
    {
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

        if (gameManager != null)
        {
            gameManager.TriggerGameOver();
        }
    }
}