using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] _enemyPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private int _maxSpawnCount = 12;
    [Range(0f, 1f)]
    [SerializeField] private float _spawnChancePerTile = 0.6f;
    [SerializeField] private float _minDistanceFromPlayer = 5f;

    [Header("References")]
    [SerializeField] private Transform _enemyRoot;
    [SerializeField] private Transform _player;

    private readonly List<SpawnCandidate> _spawnCandidates = new List<SpawnCandidate>();

    private void Start()
    {
        if (_player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                _player = playerObject.transform;
            }
        }

        SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        _spawnCandidates.Clear();

        CollectSpawnCandidates();
        Shuffle(_spawnCandidates);

        int spawnedCount = 0;

        for (int i = 0; i < _spawnCandidates.Count; i++)
        {
            if (spawnedCount >= _maxSpawnCount) break;

            SpawnCandidate candidate = _spawnCandidates[i];

            if (Random.value > _spawnChancePerTile)
            {
                continue;
            }

            if (_player != null)
            {
                float distance = Vector2.Distance(_player.position, candidate.Position);

                if (distance < _minDistanceFromPlayer)
                {
                    continue;
                }
            }

            GameObject enemyPrefab = GetRandomEnemyPrefab(candidate.EnemyKind);

            if (enemyPrefab == null) continue;

            Instantiate(
                enemyPrefab,
                candidate.Position,
                Quaternion.identity,
                _enemyRoot
            );

            spawnedCount++;
        }
    }

    private void CollectSpawnCandidates()
    {
        EnemySpawnTilemap[] spawnTilemaps = FindObjectsByType<EnemySpawnTilemap>(FindObjectsSortMode.None);

        for (int i = 0; i < spawnTilemaps.Length; i++)
        {
            EnemySpawnTilemap spawnTilemap = spawnTilemaps[i];

            if (spawnTilemap == null) continue;
            if (spawnTilemap.Tilemap == null) continue;

            CollectFromTilemap(spawnTilemap);
        }
    }

    private void CollectFromTilemap(EnemySpawnTilemap spawnTilemap)
    {
        Tilemap tilemap = spawnTilemap.Tilemap;
        BoundsInt bounds = tilemap.cellBounds;

        foreach (Vector3Int cellPosition in bounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(cellPosition)) continue;

            Vector3 worldPosition = tilemap.GetCellCenterWorld(cellPosition);

            _spawnCandidates.Add(new SpawnCandidate(
                worldPosition,
                spawnTilemap.EnemyKind
            ));
        }
    }

    private GameObject GetRandomEnemyPrefab(EnemyKind enemyKind)
    {
        List<GameObject> candidates = new List<GameObject>();

        for (int i = 0; i < _enemyPrefabs.Length; i++)
        {
            GameObject prefab = _enemyPrefabs[i];

            if (prefab == null) continue;

            EnemyData enemyData = prefab.GetComponent<EnemyData>();

            if (enemyData == null) continue;
            if (enemyData.EnemyKind != enemyKind) continue;

            candidates.Add(prefab);
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning($"생성 가능한 {enemyKind} 적 프리팹이 없습니다.");
            return null;
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    private void Shuffle(List<SpawnCandidate> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);

            SpawnCandidate temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private struct SpawnCandidate
    {
        public Vector3 Position;
        public EnemyKind EnemyKind;

        public SpawnCandidate(Vector3 position, EnemyKind enemyKind)
        {
            Position = position;
            EnemyKind = enemyKind;
        }
    }
}