using System.Collections.Generic;
using UnityEngine;

public class PlayerSlamAttackBox : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Collider2D _slamCollider;
    [SerializeField] private PlayerSlam _playerSlam;
    [SerializeField] private GameFeedbackManager _feedback;

    [Header("Slam Settings")]
    [SerializeField] private int _damage = 5;

    private readonly HashSet<EnemyHitBox> _hitTargets = new HashSet<EnemyHitBox>();

    private bool _isAttackActive;
    private bool _feedbackPlayedThisSlam;

    private void Awake()
    {
        if (_slamCollider == null)
        {
            _slamCollider = GetComponent<Collider2D>();
        }

        if (_slamCollider != null)
        {
            _slamCollider.enabled = false;
        }

        if (_playerSlam == null)
        {
            _playerSlam = GetComponentInParent<PlayerSlam>();
        }

        if (_feedback == null)
        {
            _feedback = FindFirstObjectByType<GameFeedbackManager>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_isAttackActive) return;

        EnemyHitBox enemyHitBox = collision.GetComponent<EnemyHitBox>();

        if (enemyHitBox == null)
        {
            enemyHitBox = collision.GetComponentInParent<EnemyHitBox>();
        }

        if (enemyHitBox == null) return;
        if (_hitTargets.Contains(enemyHitBox)) return;

        EnemyHitResult hitResult = enemyHitBox.ReceiveHit(PlayerAttackType.Slam, _damage);

        if (hitResult == EnemyHitResult.None) return;

        _hitTargets.Add(enemyHitBox);

        if (!_feedbackPlayedThisSlam)
        {
            _feedback?.PlaySlamHitFeedback();
            _feedbackPlayedThisSlam = true;
        }

        if (_playerSlam != null && hitResult == EnemyHitResult.Killed)
        {
            _playerSlam.BounceAfterSlamKill();
        }
    }

    public void StartSlamAttack()
    {
        if (_isAttackActive) return;

        _isAttackActive = true;
        _feedbackPlayedThisSlam = false;
        _hitTargets.Clear();

        if (_slamCollider != null)
        {
            _slamCollider.enabled = true;
        }
    }

    public void EndSlamAttack()
    {
        _isAttackActive = false;

        if (_slamCollider != null)
        {
            _slamCollider.enabled = false;
        }

        _hitTargets.Clear();
    }
}