using UnityEngine;

public enum ChunkType
{
    Start,
    Normal,
    Goal
}

public class MapChunk : MonoBehaviour
{
    [SerializeField] private ChunkType _chunkType = ChunkType.Normal;
    [SerializeField] private float _chunkHeight = 15f;
    [SerializeField] private Transform _playerSpawnPoint;
    [SerializeField] private Transform _goalPoint;

    public ChunkType ChunkType => _chunkType;
    public float ChunkHeight => _chunkHeight;
    public Transform PlayerSpawnPoint => _playerSpawnPoint;
    public Transform GoalPoint => _goalPoint;
}