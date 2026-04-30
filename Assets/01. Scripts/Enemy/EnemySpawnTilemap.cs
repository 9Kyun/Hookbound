using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawnTilemap : MonoBehaviour
{
    [SerializeField] private EnemyKind _enemyKind;
    [SerializeField] private Tilemap _tilemap;

    public EnemyKind EnemyKind => _enemyKind;
    public Tilemap Tilemap => _tilemap;

    private void Awake()
    {
        if (_tilemap == null)
        {
            _tilemap = GetComponent<Tilemap>();
        }
    }
}