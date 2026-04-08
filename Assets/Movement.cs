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

    [Header("Audio")]
    public AudioSource catAudioSource;
    public AudioClip jumpSound;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    [SerializeField] private bool isFacingLeft;

    private GameManager gameManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
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

        if (transform.position.y < -10f)
        {
            if (gameManager != null)
            {
                gameManager.RestartGame();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("coins"))
        {
            if (gameManager != null)
            {
                gameManager.AddScore();
            }
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (gameManager != null)
            {
                gameManager.RestartGame();
            }
        }
    }
}