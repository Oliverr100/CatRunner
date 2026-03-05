using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public Transform player;
    public GameObject levelChunkPrefab;

    public float chunkWidth = 20f;
    public float spawnLookAheadDistance = 30f;

    private float nextSpawnX;

    void Start()
    {
        nextSpawnX = chunkWidth;
        ChunkLoader();
    }

    void ChunkLoader()
    {
        Instantiate(levelChunkPrefab, new Vector3(nextSpawnX, 0, 0), Quaternion.identity);

        nextSpawnX += chunkWidth;
    }

    void Update()
    {
        if (player == null)
        {
            return;
        }

        if (player.position.x > nextSpawnX - spawnLookAheadDistance)
        {
            ChunkLoader();
        }
    }
}