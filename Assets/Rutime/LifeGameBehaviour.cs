using System;
using LifeGame3D.Job;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LifeGame3D
{
    public enum RenderType
    {
        GameObject,
        DrawMesh,
        DrawMeshInstanced,
        DrawMeshInstancedProcedural,
        CombinedMesh,
    }

    public class LifeGameBehaviour : MonoBehaviour
    {
        public static event Action OnPlay;
        public static event Action OnStopped;

        public int3 size = new int3(64, 64, 128);
        public RenderType renderType;
        private ILifeGameRenderer _renderer;
        private DataChunk _dataChunk;

        private int _currentHeight;
        public bool useRandom;
        [SerializeField] private int seed = 372198379;
        [SerializeField] private float _warmUpFrame = 30;
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _material;
        [SerializeField] private Material _materialProcedural;

        public int initialWidth;
        public bool[] initialData;


        private void Awake()
        {
            _dataChunk = new DataChunk(size.x, size.y, size.z);

            var localToWorldMatrix = transform.localToWorldMatrix;
            switch (renderType)
            {
                case RenderType.GameObject:
                    _renderer = new LifeGameRendererGameObject(_mesh, _material, localToWorldMatrix);
                    break;
                case RenderType.DrawMesh:
                    _renderer = new LifeGameRendererDrawMesh(_mesh, _material, localToWorldMatrix);
                    break;
                case RenderType.DrawMeshInstanced:
                    _renderer = new LifeGameRendererDrawMeshInstanced(_mesh, _material, localToWorldMatrix);
                    break;
                case RenderType.DrawMeshInstancedProcedural:
                    _renderer = new LifeGameRendererInstancedProcedural(_mesh, _materialProcedural,
                        new Vector3Int(size.x, size.y, size.z), localToWorldMatrix);
                    break;
                case RenderType.CombinedMesh:
                    _renderer = new LifeGameRendererCombinedMesh(_mesh, _material, localToWorldMatrix);
                    break;
                // 速度出せなかったので除外
                // case RenderType.CombinedMeshBRG:
                //     _renderer = new LifeGameRendererBatchRendererGroup(_mesh, _material);
                //     break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Random.InitState(seed);

            var us = _dataChunk.Get2DimArray(0);
            if (useRandom)
            {
                for (int i = 0; i < (size.x * size.z); i++)
                {
                    if (Random.Range(0f, 1f) < 0.5f)
                    {
                        us.AddFlag(i, LifeGameFlags.Alive);
                    }
                }
            }
            else
            {
                // 中心に寄せるように
                var offsetX = size.x / 2;
                var offsetZ = size.z / 2;
                for (int z = 0; z < initialWidth; z++)
                {
                    for (int x = 0; x < initialWidth; x++)
                    {
                        if (initialData[x + z * initialWidth])
                        {
                            us.AddFlag(x + offsetX, 0, z + offsetZ, LifeGameFlags.Alive);
                        }
                    }
                }
            }
        }

        private int _height;
        private bool _isFirstUpdate;

        private void Update()
        {
            if (Time.frameCount < _warmUpFrame) return;

            if (_isFirstUpdate == false)
            {
                _isFirstUpdate = true;
                OnPlay?.Invoke();
            }

            if (_height == 0)
            {
                _renderer.AddRenderBuffer(_dataChunk.Get2DimArray(0), 0);
                _height++;
            }
            else if (_height < _dataChunk.yLength)
            {
                var prev = _dataChunk.Get2DimArray(_height - 1);
                var current = _dataChunk.Get2DimArray(_height);

                LifeGameJob calculateJob = new LifeGameJob()
                {
                    input = prev,
                    output = current,
                };
                var calcHandle = calculateJob.Schedule(size.x * size.z, 1);
                calcHandle.Complete();

                _renderer.AddRenderBuffer(current, _height);
                _height++;
            }
            else if (_height >= _dataChunk.yLength)
            {
                OnStopped?.Invoke();
            }

            var vecSize = transform.localToWorldMatrix.MultiplyPoint(new Vector3(size.x, size.y, size.z));
            var bounds = new Bounds();
            bounds.SetMinMax(Vector3.zero, vecSize);
            _renderer.RenderInstance(new Bounds(vecSize * 0.5f, vecSize));
        }

        private void OnDestroy()
        {
            _renderer.Dispose();
            _dataChunk.Dispose();
        }
    }
}
