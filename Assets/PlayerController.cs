using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float playerRadius = 0.3f;

    private Camera _cam;
    private SpriteRenderer _sr;

    void Start()
    {
        _cam = Camera.main;

        // Procedural circle sprite
        int res = 64;
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        float center = res * 0.5f;
        float rSq = center * center;
        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dx = x - center + 0.5f;
                float dy = y - center + 0.5f;
                float a = (dx * dx + dy * dy <= rSq) ? 1f : 0f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);

        _sr = gameObject.AddComponent<SpriteRenderer>();
        _sr.sprite = sprite;
        _sr.color = Color.cyan;
        _sr.sortingOrder = 10;

        transform.localScale = Vector3.one * playerRadius * 2f;
    }

    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(_cam.transform.position.z);
        Vector3 mouseWorld = _cam.ScreenToWorldPoint(mousePos);
        mouseWorld.z = 0f;
        transform.position = mouseWorld;
    }

    public void SetInvincibleVisual(bool invincible)
    {
        if (_sr == null) return;
        Color c = _sr.color;
        c.a = invincible ? 0.4f : 1f;
        _sr.color = c;
    }
}
