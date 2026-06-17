using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Transform Player;

    private void Update()
    {
        this.transform.position = new Vector3(Player.position.x, 0f, this.transform.position.z);
    }
}