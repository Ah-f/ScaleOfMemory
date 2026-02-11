using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

public class CubeSpawner : MonoBehaviour
{
    public int cubeCount = 1000000;
    public float cubeSize = 0.05f;
    public float speed = 3f;
    public float changeInterval = 2f;

    private Mesh _mesh;
    private Material _material;
    private Camera _cam;
    private RenderParams _renderParams;

    private NativeArray<float3> _positions;
    private NativeArray<float3> _directions;
    private NativeArray<float> _timers;
    private NativeArray<uint> _seeds;
    private NativeArray<Matrix4x4> _matrices;
    private ComputeBuffer _colorBuffer;

    private int _currentCount;
    private float _currentSize;
    private bool _initialized;

    public NativeArray<float3> Positions => _positions;
    public int CurrentCount => _currentCount;
    public float CurrentSize => _currentSize;
    public bool IsInitialized => _initialized;

    void Start()
    {
        _mesh = CreateQuadMesh();
        _material = new Material(Shader.Find("Custom/InstancedColor"));
        _material.enableInstancing = true;
        _cam = Camera.main;
        Initialize();
    }

    void Update()
    {
        if (_currentCount != cubeCount || _currentSize != cubeSize)
            Initialize();

        if (!_initialized) return;

        float3 camPos = _cam.transform.position;
        float dist = math.abs(camPos.z);
        float halfH = dist * math.tan(math.radians(_cam.fieldOfView * 0.5f));
        float halfW = halfH * _cam.aspect;
        float2 boundsMin = new float2(camPos.x - halfW * 0.9f, camPos.y - halfH * 0.9f);
        float2 boundsMax = new float2(camPos.x + halfW * 0.9f, camPos.y + halfH * 0.9f);

        var job = new MoveAndMatrixJob
        {
            positions = _positions,
            directions = _directions,
            timers = _timers,
            seeds = _seeds,
            matrices = _matrices,
            dt = Time.deltaTime,
            speed = speed,
            changeInterval = changeInterval,
            boundsMin = boundsMin,
            boundsMax = boundsMax,
            scale = cubeSize
        };

        job.Schedule(_currentCount, 512).Complete();

        Graphics.RenderMeshInstanced(_renderParams, _mesh, 0, _matrices);
    }

    [BurstCompile]
    struct MoveAndMatrixJob : IJobParallelFor
    {
        public NativeArray<float3> positions;
        public NativeArray<float3> directions;
        public NativeArray<float> timers;
        public NativeArray<uint> seeds;
        [WriteOnly] public NativeArray<Matrix4x4> matrices;

        [ReadOnly] public float dt;
        [ReadOnly] public float speed;
        [ReadOnly] public float changeInterval;
        [ReadOnly] public float2 boundsMin;
        [ReadOnly] public float2 boundsMax;
        [ReadOnly] public float scale;

        public void Execute(int i)
        {
            float t = timers[i] + dt;
            float3 dir = directions[i];

            if (t >= changeInterval)
            {
                uint seed = seeds[i];
                seed = WangHash(seed + (uint)i + 1u);
                float rx = HashToFloat(seed) * 2f - 1f;
                seed = WangHash(seed);
                float ry = HashToFloat(seed) * 2f - 1f;
                seeds[i] = seed;
                dir = math.normalizesafe(new float3(rx, ry, 0f));
                t = 0f;
            }

            float3 pos = positions[i] + dir * speed * dt;

            if (pos.x <= boundsMin.x || pos.x >= boundsMax.x) dir.x = -dir.x;
            if (pos.y <= boundsMin.y || pos.y >= boundsMax.y) dir.y = -dir.y;

            pos.x = math.clamp(pos.x, boundsMin.x, boundsMax.x);
            pos.y = math.clamp(pos.y, boundsMin.y, boundsMax.y);

            positions[i] = pos;
            directions[i] = dir;
            timers[i] = t;

            matrices[i] = float4x4.TRS(pos, quaternion.identity, new float3(scale));
        }

        static uint WangHash(uint seed)
        {
            seed = (seed ^ 61u) ^ (seed >> 16);
            seed *= 9u;
            seed ^= seed >> 4;
            seed *= 0x27d4eb2du;
            seed ^= seed >> 15;
            return seed;
        }

        static float HashToFloat(uint hash)
        {
            return (hash & 0x00FFFFFFu) / (float)0x01000000u;
        }
    }

    void Initialize()
    {
        Cleanup();

        _currentCount = math.max(0, cubeCount);
        _currentSize = cubeSize;

        _positions = new NativeArray<float3>(_currentCount, Allocator.Persistent);
        _directions = new NativeArray<float3>(_currentCount, Allocator.Persistent);
        _timers = new NativeArray<float>(_currentCount, Allocator.Persistent);
        _seeds = new NativeArray<uint>(_currentCount, Allocator.Persistent);
        _matrices = new NativeArray<Matrix4x4>(_currentCount, Allocator.Persistent);

        // Generate colors and upload to GPU
        var colors = new Vector4[_currentCount];
        for (int i = 0; i < _currentCount; i++)
        {
            colors[i] = UnityEngine.Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.7f, 1f);

            _positions[i] = new float3(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-3f, 3f), 0f);
            _directions[i] = math.normalizesafe(new float3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0f));
            _timers[i] = UnityEngine.Random.Range(0f, changeInterval);
            _seeds[i] = (uint)UnityEngine.Random.Range(1, int.MaxValue);
        }

        _colorBuffer = new ComputeBuffer(_currentCount, 16);
        _colorBuffer.SetData(colors);
        _material.SetBuffer("_Colors", _colorBuffer);

        _renderParams = new RenderParams(_material);

        _initialized = true;
    }

    void Cleanup()
    {
        _initialized = false;
        if (_positions.IsCreated) _positions.Dispose();
        if (_directions.IsCreated) _directions.Dispose();
        if (_timers.IsCreated) _timers.Dispose();
        if (_seeds.IsCreated) _seeds.Dispose();
        if (_matrices.IsCreated) _matrices.Dispose();
        if (_colorBuffer != null) { _colorBuffer.Release(); _colorBuffer = null; }
    }

    Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0),
            new Vector3(0.5f, 0.5f, 0), new Vector3(-0.5f, 0.5f, 0)
        };
        mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
        mesh.normals = new Vector3[] { -Vector3.forward, -Vector3.forward, -Vector3.forward, -Vector3.forward };
        mesh.uv = new Vector2[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
        return mesh;
    }

    void OnDestroy()
    {
        Cleanup();
        if (_material != null) Destroy(_material);
    }
}
