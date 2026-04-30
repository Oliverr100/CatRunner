using UnityEngine;

public class SimpleEnemyBrain : MonoBehaviour
{
    [Header("Movement Settings")]
    public float baseMoveSpeed = 3f;
    public bool moveLeft = true;

    [Header("Patrol Eyes")]
    public Transform edgeCheck;
    public float rayDistance = 1f;
    public LayerMask whatIsGround;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float speedToUse = moveLeft ? -baseMoveSpeed : baseMoveSpeed;
        rb.velocity = new Vector2(speedToUse, rb.velocity.y);

        RaycastHit2D groundInfo = Physics2D.Raycast(edgeCheck.position, Vector2.down, rayDistance, whatIsGround);

        if (groundInfo.collider == false)
        {
            Flip(); // Turn around!
        }
    }

    private void Flip()
    {
        moveLeft = !moveLeft;

        transform.Rotate(0f, 180f, 0f);
    }

    private void OnDrawGizmos()
    {
        if (edgeCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(edgeCheck.position, edgeCheck.position + Vector3.down * rayDistance);
        }
    }
}