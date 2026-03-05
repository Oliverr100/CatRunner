using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float left_limit;

    public Vector3 offset = new Vector3(0, 2f, -10f);

    void Update()
    {
        if (target != null)
        {
            Vector3 new_position;
            new_position = transform.position;
            new_position.x = target.position.x + offset.x;
            if (new_position.x < left_limit)
            {
                new_position.x = left_limit;
            }
            transform.position = new_position;

        }
    }
}