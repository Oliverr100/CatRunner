using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public Transform player;
    public GameObject levelChunkPrefab;

    public float chunkWidth = 20f;

    // Where the next floor piece should snap into place
    private float nextSpawnX;

    void Start()
    {
        nextSpawnX = chunkWidth;
    }

    void Update()
    {
        if (player.position.x > nextSpawnX - 15f)
        {
            Instantiate(levelChunkPrefab, new Vector3(nextSpawnX, 0, 0), Quaternion.identity);

            nextSpawnX += chunkWidth;
        }
    }
}