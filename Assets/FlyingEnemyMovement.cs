using UnityEngine;

public class FlyingEnemyMovement : MonoBehaviour
{
    [Header("Flight Settings")]
    public float baseMoveSpeed = 4f;
    public bool moveLeft = true;

    private Rigidbody2D rb;
    private GameManager gameManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        float currentSpeed = baseMoveSpeed;

        if (gameManager != null)
        {
            currentSpeed = baseMoveSpeed * gameManager.difficultyMultiplier;
        }

        float speedToUse = moveLeft ? -currentSpeed : currentSpeed;

        rb.velocity = new Vector2(speedToUse, 0f);
    }
}