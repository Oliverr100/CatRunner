using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public Vector3 offset = new Vector3(0, 2f, -10f);

    void Update()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}