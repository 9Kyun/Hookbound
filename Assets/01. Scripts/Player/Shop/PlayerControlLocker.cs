using UnityEngine;

public class PlayerControlLocker : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb;

    [Header("Scripts To Disable")]
    [SerializeField] private MonoBehaviour[] _controlScripts;

    private bool _isLocked;
    private float _originalGravityScale;
    private RigidbodyConstraints2D _originalConstraints;

    private void Awake()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody2D>();
        }
    }

    public void LockControls()
    {
        if (_isLocked) return;

        _isLocked = true;

        if (_rb != null)
        {
            _originalGravityScale = _rb.gravityScale;
            _originalConstraints = _rb.constraints;

            _rb.linearVelocity = Vector2.zero;
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        for (int i = 0; i < _controlScripts.Length; i++)
        {
            if (_controlScripts[i] == null) continue;

            _controlScripts[i].enabled = false;
        }
    }

    public void UnlockControls()
    {
        if (!_isLocked) return;

        _isLocked = false;

        for (int i = 0; i < _controlScripts.Length; i++)
        {
            if (_controlScripts[i] == null) continue;

            _controlScripts[i].enabled = true;
        }

        if (_rb != null)
        {
            _rb.constraints = _originalConstraints;
            _rb.gravityScale = _originalGravityScale;
            _rb.linearVelocity = Vector2.zero;
        }
    }
}