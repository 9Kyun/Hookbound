using UnityEngine;

public class PlayerAbilityApplier : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PlayerPerkInventory _perkInventory;
    [SerializeField] private PlayerPassiveStats _passiveStats;

    [Header("Player Components")]
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private PlayerJump _playerJump;
    [SerializeField] private PlayerHook _playerHook;
    [SerializeField] private PlayerHookAim _playerHookAim;
    [SerializeField] private PlayerDash _playerDash;
    [SerializeField] private PlayerDamageReceiver _playerDamageReceiver;
    [SerializeField] private PlayerCurrencyMagnet _playerCurrencyMagnet;
    [SerializeField] private PlayerAttack _playerAttack;
    [SerializeField] private PlayerLifeSteal _playerLifeSteal;

    [Header("Perk Values")]
    [SerializeField] private int _doubleJumpExtraCount = 1;
    [SerializeField] private int _hookCountBonus = 1;
    [SerializeField] private int _dashCountBonus = 1;
    [SerializeField] private float _invincibleBonusDuration = 0.3f;

    private bool _isSubscribed;

    private void Awake()
    {
        ResolvePlayerComponents();
    }

    private void Start()
    {
        ResolveDataManagers();
        SubscribeEvents();
        ApplyAbilities();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void ResolvePlayerComponents()
    {
        if (_playerHealth == null)
        {
            _playerHealth = GetComponent<PlayerHealth>();
        }

        if (_playerMovement == null)
        {
            _playerMovement = GetComponent<PlayerMovement>();
        }

        if (_playerJump == null)
        {
            _playerJump = GetComponent<PlayerJump>();
        }

        if (_playerHook == null)
        {
            _playerHook = GetComponent<PlayerHook>();
        }

        if (_playerHookAim == null)
        {
            _playerHookAim = GetComponent<PlayerHookAim>();
        }

        if (_playerDash == null)
        {
            _playerDash = GetComponent<PlayerDash>();
        }

        if (_playerDamageReceiver == null)
        {
            _playerDamageReceiver = GetComponent<PlayerDamageReceiver>();
        }

        if (_playerCurrencyMagnet == null)
        {
            _playerCurrencyMagnet = GetComponent<PlayerCurrencyMagnet>();
        }

        if (_playerAttack == null)
        {
            _playerAttack = GetComponent<PlayerAttack>();
        }

        if (_playerLifeSteal == null)
        {
            _playerLifeSteal = GetComponent<PlayerLifeSteal>();
        }
    }

    private void ResolveDataManagers()
    {
        if (_perkInventory == null)
        {
            _perkInventory = FindAnyObjectByType<PlayerPerkInventory>();
        }

        if (_passiveStats == null)
        {
            _passiveStats = FindAnyObjectByType<PlayerPassiveStats>();
        }
    }

    private void SubscribeEvents()
    {
        if (_isSubscribed) return;

        if (_perkInventory != null)
        {
            _perkInventory.OnEquippedPerksChanged += ApplyAbilities;
        }

        if (_passiveStats != null)
        {
            _passiveStats.OnStatsChanged += ApplyAbilities;
        }

        _isSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!_isSubscribed) return;

        if (_perkInventory != null)
        {
            _perkInventory.OnEquippedPerksChanged -= ApplyAbilities;
        }

        if (_passiveStats != null)
        {
            _passiveStats.OnStatsChanged -= ApplyAbilities;
        }

        _isSubscribed = false;
    }

    private void ApplyAbilities()
    {
        ResolveDataManagers();
        ResolvePlayerComponents();

        ApplyPassiveStats();
        ApplyEquippedPerks();
    }

    private void ApplyPassiveStats()
    {
        if (_passiveStats == null) return;

        if (_playerHealth != null)
        {
            _playerHealth.SetMaxHp(_passiveStats.MaxHp);
        }

        if (_playerMovement != null)
        {
            _playerMovement.SetMoveSpeed(_passiveStats.MoveSpeed);
        }

        if (_playerHookAim != null)
        {
            _playerHookAim.SetHookLengthMax(_passiveStats.HookRange);
        }

        if (_playerHook != null)
        {
            _playerHook.SetHookSpeedBonusPercent(_passiveStats.HookSpeedBonusPercent);
        }
    }

    private void ApplyEquippedPerks()
    {
        if (_perkInventory == null) return;

        bool hasDoubleJump = _perkInventory.HasEquippedPerk(PerkType.DoubleJump);
        bool hasHookCountUp = _perkInventory.HasEquippedPerk(PerkType.HookCountUp);
        bool hasDashCountUp = _perkInventory.HasEquippedPerk(PerkType.DashCountUp);
        bool hasMagnet = _perkInventory.HasEquippedPerk(PerkType.Magnet);
        bool hasAttackPierce = _perkInventory.HasEquippedPerk(PerkType.AttackPierce);
        bool hasLifeSteal = _perkInventory.HasEquippedPerk(PerkType.LifeSteal);
        bool hasInvincibleTimeUp = _perkInventory.HasEquippedPerk(PerkType.InvincibleTimeUp);

        if (_playerJump != null)
        {
            _playerJump.SetExtraJumpCount(hasDoubleJump ? _doubleJumpExtraCount : 0);
        }

        if (_playerHook != null)
        {
            _playerHook.SetBonusMaxHookCount(hasHookCountUp ? _hookCountBonus : 0);
        }

        if (_playerDash != null)
        {
            _playerDash.SetBonusMaxDashCount(hasDashCountUp ? _dashCountBonus : 0);
        }

        if (_playerCurrencyMagnet != null)
        {
            _playerCurrencyMagnet.SetMagnetEnabled(hasMagnet);
        }

        if (_playerDamageReceiver != null)
        {
            _playerDamageReceiver.SetInvincibleBonusDuration(
                hasInvincibleTimeUp ? _invincibleBonusDuration : 0f
            );
        }

        if (_playerAttack != null)
        {
            _playerAttack.SetAttackPierceEnabled(hasAttackPierce);
        }

        if (_playerLifeSteal != null)
        {
            _playerLifeSteal.SetLifeStealEnabled(hasLifeSteal);
        }
    }
}