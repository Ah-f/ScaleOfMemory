using System.Collections;
using UnityEngine;

public class BattleVFX : MonoBehaviour
{
    public static BattleVFX Instance { get; private set; }

    Camera _cam;
    Vector3 _camOriginalPos;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _cam = Camera.main;
        if (_cam != null)
            _camOriginalPos = _cam.transform.position;

        GameEvents.OnEnemyDamaged += OnEnemyDamaged;
        GameEvents.OnPlayerDamaged += OnPlayerDamaged;
        GameEvents.OnCardPlayed += OnCardPlayed;
        GameEvents.OnEnemyDefeated += OnEnemyDefeated;
        GameEvents.OnEnemyDodged += OnEnemyDodged;
        GameEvents.OnEnemyBlocked += OnEnemyBlocked;
        GameEvents.OnPlayerBlockAbsorbed += OnPlayerBlockAbsorbed;
        GameEvents.OnPlayerScalesAbsorbed += OnPlayerScalesAbsorbed;
        GameEvents.OnEnemyAttacking += OnEnemyAttacking;
    }

    void OnEnemyDamaged(EnemyInstance enemy, int dmg)
    {
        // Hit flash on enemy sprite
        if (enemy.ViewObject != null)
        {
            var hitFlash = enemy.ViewObject.GetComponent<HitFlash>();
            if (hitFlash != null) hitFlash.Flash();
        }

        // Spawn hit particles at enemy position
        if (enemy.ViewObject != null)
            SpawnHitParticles(enemy.ViewObject.transform.position, Color.white, 10);

        // Screen shake (small)
        StartCoroutine(ScreenShake(0.1f, 0.05f));
    }

    void OnPlayerDamaged(int dmg)
    {
        // Screen shake (bigger for player damage)
        StartCoroutine(ScreenShake(0.2f, 0.12f));

        // Red flash overlay
        StartCoroutine(DamageFlash());
    }

    void OnCardPlayed(CardInstance card)
    {
        // Element-based particle burst
        Color particleColor = BattleConstants.GetElementColor(card.Data.element);
        if (card.Data.cardType == CardType.Attack)
        {
            // Find target enemy and spawn particles there
            foreach (var enemy in BattleManager.Instance.Enemies)
            {
                if (!enemy.IsDead && enemy.ViewObject != null)
                {
                    SpawnElementParticles(enemy.ViewObject.transform.position, particleColor, card.Data.element);
                    break;
                }
            }
        }
        else if (card.Data.cardType == CardType.Defend)
        {
            // Shield effect around player area
            SpawnShieldParticles(particleColor);
        }
    }

    void OnEnemyDodged(EnemyInstance enemy)
    {
        if (enemy.ViewObject != null)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
                DamagePopup.CreateText(canvas.transform, enemy.ViewObject.transform.position,
                    "MISS!", new Color(0.7f, 0.7f, 0.7f));
        }
    }

    void OnEnemyBlocked(EnemyInstance enemy, int amount)
    {
        if (enemy.ViewObject != null)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
                DamagePopup.CreateText(canvas.transform, enemy.ViewObject.transform.position,
                    $"BLOCK {amount}", BattleConstants.Highlight);
        }
    }

    void OnEnemyAttacking(EnemyInstance enemy, int damage)
    {
        // Enemy lunge animation
        if (enemy.ViewObject != null)
            StartCoroutine(EnemyLunge(enemy.ViewObject.transform));
    }

    void OnPlayerBlockAbsorbed(int amount)
    {
        // Block popup near player area
        var canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            Vector3 playerPos = new Vector3(-0.3f, 1.2f, 0f);
            DamagePopup.CreateText(canvas.transform, playerPos,
                $"BLOCK -{amount}", BattleConstants.Highlight);
        }
    }

    void OnPlayerScalesAbsorbed(int amount)
    {
        // Scales popup near player area
        var canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            Vector3 playerPos = new Vector3(-0.3f, 0.8f, 0f);
            DamagePopup.CreateText(canvas.transform, playerPos,
                $"SCALES -{amount}", BattleConstants.Scales);
        }
    }

    IEnumerator EnemyLunge(Transform enemyTransform)
    {
        if (enemyTransform == null) yield break;

        Vector3 originalPos = enemyTransform.position;
        Vector3 lungeTarget = originalPos + (Vector3.left + Vector3.down).normalized * 0.5f;

        // Lunge forward
        float duration = 0.12f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (enemyTransform == null) yield break;
            elapsed += Time.deltaTime;
            enemyTransform.position = Vector3.Lerp(originalPos, lungeTarget, elapsed / duration);
            yield return null;
        }

        // Return back
        elapsed = 0f;
        duration = 0.2f;
        while (elapsed < duration)
        {
            if (enemyTransform == null) yield break;
            elapsed += Time.deltaTime;
            enemyTransform.position = Vector3.Lerp(lungeTarget, originalPos, elapsed / duration);
            yield return null;
        }

        if (enemyTransform != null)
            enemyTransform.position = originalPos;
    }

    void OnEnemyDefeated(EnemyInstance enemy)
    {
        if (enemy.ViewObject != null)
        {
            SpawnDeathParticles(enemy.ViewObject.transform.position, enemy.Data.bodyColor);
            StartCoroutine(ScreenShake(0.15f, 0.08f));
        }
    }

    // ===== SCREEN SHAKE =====
    IEnumerator ScreenShake(float duration, float magnitude)
    {
        if (_cam == null) yield break;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            _cam.transform.position = _camOriginalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _cam.transform.position = _camOriginalPos;
    }

    // ===== DAMAGE FLASH =====
    IEnumerator DamageFlash()
    {
        // Create red overlay
        var flashGo = new GameObject("DamageFlash");
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) yield break;

        flashGo.transform.SetParent(canvas.transform, false);
        var rt = flashGo.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = flashGo.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(1f, 0f, 0f, 0.3f);
        img.raycastTarget = false;

        float elapsed = 0f;
        float duration = 0.3f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(0.3f, 0f, elapsed / duration);
            img.color = new Color(1f, 0f, 0f, a);
            yield return null;
        }

        Destroy(flashGo);
    }

    // ===== PARTICLES =====
    void SpawnHitParticles(Vector3 position, Color color, int count)
    {
        var go = CreateParticleSystem("HitParticles", position);
        var ps = go.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = color;
        main.startSpeed = 3f;
        main.startSize = 0.08f;
        main.startLifetime = 0.4f;
        main.maxParticles = count;

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, (short)count)
        });
        emission.rateOverTime = 0;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        ps.Play();
        Destroy(go, 1f);
    }

    void SpawnElementParticles(Vector3 position, Color color, ElementType element)
    {
        var go = CreateParticleSystem("ElementParticles", position);
        var ps = go.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = color;
        main.startLifetime = 0.6f;
        main.maxParticles = 20;

        switch (element)
        {
            case ElementType.Fire:
                main.startSpeed = 2f;
                main.startSize = 0.15f;
                var colorLife = ps.colorOverLifetime;
                colorLife.enabled = true;
                var grad = new Gradient();
                grad.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(Color.yellow, 0f),
                        new GradientColorKey(color, 0.5f),
                        new GradientColorKey(Color.red, 1f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(1f, 0f),
                        new GradientAlphaKey(0f, 1f)
                    }
                );
                colorLife.color = grad;
                break;

            case ElementType.Ice:
                main.startSpeed = 1f;
                main.startSize = 0.1f;
                main.gravityModifier = -0.5f;
                break;

            case ElementType.Lightning:
                main.startSpeed = 8f;
                main.startSize = 0.05f;
                main.startLifetime = 0.15f;
                break;

            default:
                main.startSpeed = 2f;
                main.startSize = 0.1f;
                break;
        }

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 20)
        });
        emission.rateOverTime = 0;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        ps.Play();
        Destroy(go, 1.5f);
    }

    void SpawnShieldParticles(Color color)
    {
        Vector3 pos = _cam != null
            ? _cam.transform.position + _cam.transform.forward * 3f + Vector3.down * 1f
            : Vector3.zero;

        var go = CreateParticleSystem("ShieldParticles", pos);
        var ps = go.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(color.r, color.g, color.b, 0.5f);
        main.startSpeed = 0.5f;
        main.startSize = 0.15f;
        main.startLifetime = 0.8f;
        main.maxParticles = 15;

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 15)
        });
        emission.rateOverTime = 0;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 1f;

        ps.Play();
        Destroy(go, 1.5f);
    }

    void SpawnDeathParticles(Vector3 position, Color color)
    {
        var go = CreateParticleSystem("DeathParticles", position);
        var ps = go.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = color;
        main.startSpeed = 3f;
        main.startSize = 0.12f;
        main.startLifetime = 0.8f;
        main.maxParticles = 30;
        main.gravityModifier = 1f;

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 30)
        });
        emission.rateOverTime = 0;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        ps.Play();
        Destroy(go, 2f);
    }

    GameObject CreateParticleSystem(string name, Vector3 position)
    {
        var go = new GameObject(name);
        go.transform.position = position;

        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Default renderer setup
        var psr = go.GetComponent<ParticleSystemRenderer>();
        psr.material = new Material(Shader.Find("Particles/Standard Unlit"));
        psr.material.SetFloat("_Mode", 1); // Additive-ish

        return go;
    }

    void OnDestroy()
    {
        GameEvents.OnEnemyDamaged -= OnEnemyDamaged;
        GameEvents.OnPlayerDamaged -= OnPlayerDamaged;
        GameEvents.OnCardPlayed -= OnCardPlayed;
        GameEvents.OnEnemyDefeated -= OnEnemyDefeated;
        GameEvents.OnEnemyDodged -= OnEnemyDodged;
        GameEvents.OnEnemyBlocked -= OnEnemyBlocked;
        GameEvents.OnPlayerBlockAbsorbed -= OnPlayerBlockAbsorbed;
        GameEvents.OnPlayerScalesAbsorbed -= OnPlayerScalesAbsorbed;
        GameEvents.OnEnemyAttacking -= OnEnemyAttacking;
        Instance = null;
    }
}
