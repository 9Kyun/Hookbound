using Unity.VisualScripting;
using UnityEngine;

public class CurrencyPickUp : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb;

    [Header("Currency")]
    [SerializeField] private CurrencyType _currencyType;
    [SerializeField] private int _amount = 1;

    [Header("LifeTime")]
    [SerializeField] private float _lifeTime = 8f;

    [Header("Magnet Settings")]
    [SerializeField] private float _magnetSpeed = 12f;
    [SerializeField] private float _collectDistance = 0.2f;

    private Transform _targetPlayer;
    private bool _isMagnetActive;
    private float _lifeTimer;
    private float _originalGravity;

    private void Awake()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody2D>();
        }
    }

    private void OnEnable()
    {
        _lifeTimer = _lifeTime;
        _targetPlayer = null;
        _isMagnetActive = false;
    }

    private void Update()
    {
        if (_isMagnetActive)
        {
            MoveToPlayer();
            return;
        }

        _lifeTime -= Time.deltaTime;

        if (_lifeTimer<=0f)
        {
            Destroy(gameObject);
        }
    }

    private void MoveToPlayer()
    {
        if (_targetPlayer == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, _targetPlayer.position, _magnetSpeed * Time.deltaTime);

        float distance = Vector2.Distance(transform.position, _targetPlayer.position);

        if (distance <= _collectDistance)
        {
            Collect();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerHitBox playerHitBox = collision.GetComponent<PlayerHitBox>();

        if (playerHitBox == null) return;

        Collect();
    }

    private void Collect()
    {
        RunDataManager runDataManager = FindAnyObjectByType<RunDataManager>();

        if (runDataManager != null && runDataManager.IsRunActive)
        {
            runDataManager.AddCurrency(_currencyType, _amount);
        }
        else
        {
            CurrencyManager.Instance.AddCurrency(_currencyType, _amount);
        }

        Destroy(gameObject);
    }

    public void StartMagnet(Transform playerTransform)
    {
        if (playerTransform == null) return;

        _targetPlayer = playerTransform;
        _isMagnetActive = true;
        _rb.gravityScale = 0f;

        _lifeTimer = _lifeTime;
    }

    public void SetCurrency(CurrencyType currencyType, int amount)
    {
        _currencyType = currencyType;
        _amount = amount;
    }
}
