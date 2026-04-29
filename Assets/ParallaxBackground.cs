using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private float length, startpos;
    private GameObject cam;

    [Header("Parallax Settings")]
    [Tooltip("0 = Moves with the camera, 1 = Completely still")]
    public float parallaxEffect = 0.5f;

    void Start()
    {
        cam = Camera.main.gameObject;
        startpos = transform.position.x;

        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        float dist = (cam.transform.position.x * parallaxEffect);

        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        if (temp > startpos + length)
        {
            startpos += length;
        }
        else if (temp < startpos - length)
        {
            startpos -= length;
        }
    }
}