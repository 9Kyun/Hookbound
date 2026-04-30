using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Chunk Prefabs")]
    [SerializeField] private MapChunk _startChunkPrefab;
    [SerializeField] private List<MapChunk> _normalChunkPrefabs;
    [SerializeField] private MapChunk _goalChunkPrefab;

    [Header("Generate Settings")]
    [SerializeField] private int _normalChunkCount = 20;
    [SerializeField] private Transform _generatedChunkParent;
    [SerializeField] private Transform _playerTransform;

    private readonly List<MapChunk> _spawnedChunks = new List<MapChunk>();
    private int _lastRandomIndex = -1;

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        ClearMap();

        float currentY = 0f;

        MapChunk startChunk = SpawnChunk(_startChunkPrefab, currentY);
        currentY += startChunk.ChunkHeight;

        if (_playerTransform != null && startChunk.PlayerSpawnPoint != null)
        {
            _playerTransform.position = startChunk.PlayerSpawnPoint.position;
        }

        for (int i = 0; i < _normalChunkCount; i++)
        {
            MapChunk randomChunk = GetRandomNormalChunk();
            MapChunk spawnedChunk = SpawnChunk(randomChunk, currentY);
            currentY += spawnedChunk.ChunkHeight;
        }

        SpawnChunk(_goalChunkPrefab, currentY);
    }

    private MapChunk SpawnChunk(MapChunk chunkPrefab, float yPosition)
    {
        MapChunk spawnedChunk = Instantiate(
            chunkPrefab,
            new Vector3(0f, yPosition, 0f),
            Quaternion.identity,
            _generatedChunkParent
        );

        _spawnedChunks.Add(spawnedChunk);
        return spawnedChunk;
    }

    private MapChunk GetRandomNormalChunk()
    {
        if (_normalChunkPrefabs == null || _normalChunkPrefabs.Count == 0)
        {
            return null;
        }

        if (_normalChunkPrefabs.Count == 1)
        {
            return _normalChunkPrefabs[0];
        }

        int randomIndex = Random.Range(0, _normalChunkPrefabs.Count);

        while (randomIndex == _lastRandomIndex)
        {
            randomIndex = Random.Range(0, _normalChunkPrefabs.Count);
        }

        _lastRandomIndex = randomIndex;
        return _normalChunkPrefabs[randomIndex];
    }

    private void ClearMap()
    {
        for (int i = 0; i < _spawnedChunks.Count; i++)
        {
            if (_spawnedChunks[i] != null)
            {
                Destroy(_spawnedChunks[i].gameObject);
            }
        }

        _spawnedChunks.Clear();
        _lastRandomIndex = -1;
    }
}