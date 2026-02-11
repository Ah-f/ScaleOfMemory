using UnityEngine;

public class RandomMover : MonoBehaviour
{
    public float speed = 3f;
    public float changeInterval = 2f;

    private Camera _cam;
    private Vector3 _direction;
    private float _timer;

    void Start()
    {
        _cam = Camera.main;
        PickRandomDirection();
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= changeInterval)
        {
            PickRandomDirection();
            _timer = 0f;
        }

        transform.position += _direction * speed * Time.deltaTime;

        Vector3 vp = _cam.WorldToViewportPoint(transform.position);

        if (vp.x <= 0.05f || vp.x >= 0.95f)
            _direction.x = -_direction.x;
        if (vp.y <= 0.05f || vp.y >= 0.95f)
            _direction.y = -_direction.y;

        vp.x = Mathf.Clamp(vp.x, 0.05f, 0.95f);
        vp.y = Mathf.Clamp(vp.y, 0.05f, 0.95f);

        transform.position = _cam.ViewportToWorldPoint(vp);
    }

    void PickRandomDirection()
    {
        _direction = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            0f
        ).normalized;
    }
}
