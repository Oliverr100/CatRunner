using System.Runtime.CompilerServices;
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
        Debug.Log("LevelGenerator started. Initializing first chunk.");
        nextSpawnX = chunkWidth;
        ChunkLoader();
    }

    void ChunkLoader()
    {
        Debug.Log($"Spawning chunk at X = {nextSpawnX}");
        Instantiate(levelChunkPrefab, new Vector3(nextSpawnX, 0, 0), Quaternion.identity);

        nextSpawnX += chunkWidth;
    }

    void Update()
    {
        if (player == null)
        {
            return;
        }

        Debug.Log
            (
       $"Player X: {player.position.x}, " +
       $"Next Spawn X: {nextSpawnX}, " +
       $"Trigger At: {nextSpawnX - spawnLookAheadDistance}");


        if (player.position.x > nextSpawnX - spawnLookAheadDistance)
        {
            Debug.Log("Player is approaching the end of the current chunk. Spawning next chunk.");
            ChunkLoader();
        }
    }
}