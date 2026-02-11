using UnityEngine;

public static class EnemySpawner
{
    public static GameObject SpawnEnemy(EnemyData data, Vector3 position)
    {
        GameObject root = new GameObject(data.enemyName);
        root.transform.position = position;

        // Pixel art sprite
        GameObject spriteObj = new GameObject("Sprite");
        spriteObj.transform.SetParent(root.transform, false);
        var sr = spriteObj.AddComponent<SpriteRenderer>();
        sr.sprite = PixelArtGenerator.GenerateSprite(data);
        sr.sortingOrder = 5;
        float scale = data.bodyScale * 0.4f;
        spriteObj.transform.localScale = Vector3.one * scale;

        // Billboard (face camera)
        spriteObj.AddComponent<Billboard>();

        // Eye glow (subtle point light)
        var glowObj = new GameObject("EyeGlow");
        glowObj.transform.SetParent(root.transform, false);
        glowObj.transform.localPosition = new Vector3(0, 0.3f * scale, -0.3f);
        var light = glowObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = data.eyeColor;
        light.range = 1f * scale;
        light.intensity = 1.5f;

        // Idle animation
        var idleAnim = root.AddComponent<EnemyIdleAnimation>();
        idleAnim.basePosition = position;

        // Hit flash component
        var hitFlash = root.AddComponent<HitFlash>();
        hitFlash.spriteRenderer = sr;

        return root;
    }
}

public class EnemyIdleAnimation : MonoBehaviour
{
    public Vector3 basePosition;
    float _timeOffset;

    void Start()
    {
        _timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float bob = Mathf.Sin((Time.time + _timeOffset) * 2f) * 0.08f;
        float sway = Mathf.Sin((Time.time + _timeOffset) * 1.3f) * 0.02f;
        transform.position = basePosition + new Vector3(sway, bob, 0f);
    }
}

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
            transform.forward = Camera.main.transform.forward;
    }
}

public class HitFlash : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    float _flashTimer;
    Color _originalColor = Color.white;
    MaterialPropertyBlock _mpb;

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
    }

    public void Flash()
    {
        _flashTimer = 0.15f;
    }

    void Update()
    {
        if (_flashTimer > 0f)
        {
            _flashTimer -= Time.deltaTime;
            float t = _flashTimer / 0.15f;
            Color c = Color.Lerp(_originalColor, Color.red, t);
            if (spriteRenderer != null)
            {
                spriteRenderer.GetPropertyBlock(_mpb);
                _mpb.SetColor("_Color", c);
                spriteRenderer.SetPropertyBlock(_mpb);
            }
        }
    }
}
