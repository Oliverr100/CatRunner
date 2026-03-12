using System.Security;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float jumpForce = 1f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private GameManager gameManager;
    [SerializeField]private bool isFacingLeft;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        Debug.Log(moveInput);
            
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        if(moveInput <= -1)
        {
            isFacingLeft = true;
        }
        else
        {
            isFacingLeft = false;
        }

        sr.flipX = isFacingLeft;

        anim.SetFloat("Speed", Mathf.Abs(moveInput));

        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            anim.SetTrigger("Jump");
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
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            if (gameManager != null)
            {
                gameManager.RestartGame();
            }
        }
    }
}