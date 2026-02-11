using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public class GameManager : MonoBehaviour
{
    public float maxHP = 100f;
    public float damagePerHit = 5f;
    public float invincibleDuration = 1.5f;

    [HideInInspector] public float currentHP;
    [HideInInspector] public float survivalTime;
    [HideInInspector] public bool isGameOver;

    private CubeSpawner _spawner;
    private PlayerController _player;
    private GameUI _ui;

    private float _invincibleTimer;
    private bool _isInvincible;

    void Start()
    {
        _spawner = FindObjectOfType<CubeSpawner>();
        _player = FindObjectOfType<PlayerController>();
        _ui = FindObjectOfType<GameUI>();

        currentHP = maxHP;
        survivalTime = 0f;
        isGameOver = false;
        Time.timeScale = 1f;
    }

    void LateUpdate()
    {
        if (isGameOver) return;

        survivalTime += Time.deltaTime;

        // Invincibility cooldown
        if (_isInvincible)
        {
            _invincibleTimer -= Time.deltaTime;
            if (_invincibleTimer <= 0f)
            {
                _isInvincible = false;
                if (_player != null)
                    _player.SetInvincibleVisual(false);
            }
        }

        // Collision check
        if (!_isInvincible && _spawner != null && _spawner.IsInitialized && _player != null)
        {
            float3 playerPos = new float3(_player.transform.position);
            float collisionRadius = _player.playerRadius + _spawner.CurrentSize * 0.5f;
            float radiusSq = collisionRadius * collisionRadius;

            var hitCount = new NativeArray<int>(1, Allocator.TempJob);

            var job = new CollisionJob
            {
                positions = _spawner.Positions,
                playerPos = playerPos,
                radiusSq = radiusSq,
                hitCount = hitCount
            };

            job.Schedule(_spawner.CurrentCount, 1024).Complete();

            int hits = hitCount[0];
            hitCount.Dispose();

            if (hits > 0)
            {
                currentHP -= damagePerHit;
                currentHP = Mathf.Max(0f, currentHP);

                _isInvincible = true;
                _invincibleTimer = invincibleDuration;
                if (_player != null)
                    _player.SetInvincibleVisual(true);

                if (currentHP <= 0f)
                {
                    isGameOver = true;
                    Time.timeScale = 0f;
                }
            }
        }

        // Update UI
        if (_ui != null)
            _ui.UpdateUI(currentHP / maxHP, survivalTime, isGameOver);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        currentHP = maxHP;
        survivalTime = 0f;
        isGameOver = false;
        _isInvincible = false;
        if (_player != null)
            _player.SetInvincibleVisual(false);
        if (_ui != null)
            _ui.UpdateUI(1f, 0f, false);
    }
}
