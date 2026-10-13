using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public Transform player;
    public GameObject levelChunkPrefab;

    public float chunkWidth = 20f;
    public float spawnLookAheadDistance = 30f;

    [Tooltip("POLISH: chunks further than this behind the player are destroyed. Keeps memory and physics cost flat (important for WebGL).")]
    public float despawnDistance = 120f;

    private float nextSpawnX;
    private readonly List<GameObject> spawnedChunks = new List<GameObject>();

    void Start()
    {
        nextSpawnX = chunkWidth;
        ChunkLoader();
    }

    void ChunkLoader()
    {
        spawnedChunks.Add(Instantiate(levelChunkPrefab, new Vector3(nextSpawnX, 0, 0), Quaternion.identity));

        nextSpawnX += chunkWidth;
    }

    void Update()
    {
        if (player == null)
        {
            return;
        }

        // POLISH: removed the per-frame Debug.Log calls (they spammed the Console and slowed the game down)
        if (player.position.x > nextSpawnX - spawnLookAheadDistance)
        {
            ChunkLoader();
        }

        // POLISH: throw away chunks the player has long left behind
        while (spawnedChunks.Count > 0)
        {
            GameObject oldest = spawnedChunks[0];
            if (oldest == null) { spawnedChunks.RemoveAt(0); continue; }
            if (oldest.transform.position.x < player.position.x - despawnDistance)
            {
                Destroy(oldest);
                spawnedChunks.RemoveAt(0);
            }
            else break;
        }
    }
}
