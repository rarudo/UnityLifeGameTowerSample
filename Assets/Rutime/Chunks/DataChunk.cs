using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace LifeGame3D
{
    public struct DataChunk : IDisposable
    {
        private NativeArray<LifeGameEntity> _data;

        public readonly int xLength;
        public readonly int yLength;
        public readonly int zLength;
        public int Length => _data.Length;

        public int GetIndex(int x, int y, int z)
        {
            return x + (z * xLength) + (y * xLength * zLength);
        }

        public void GetIndex(int index, out int x, out int y, out int z)
        {
            x = index % xLength;
            z = index / xLength % zLength;
            y = index / xLength / zLength;
        }

        public unsafe void AddFlag(int x, int y, int z, LifeGameFlags flags)
        {
            var index = GetIndex(x, y, z);
            var ptr = (LifeGameEntity*)_data.GetUnsafePtr();
            ptr[index].AddFlag(flags);
        }

        public unsafe void AddFlag(int index, LifeGameFlags flags)
        {
            var ptr = (LifeGameEntity*)_data.GetUnsafePtr();
            ptr[index].AddFlag(flags);
        }

        public unsafe bool IsFlag(int x, int y, int z, LifeGameFlags flags)
        {
            var index = GetIndex(x, y, z);
            var ptr = (LifeGameEntity*)_data.GetUnsafePtr();
            return ptr[index].IsFlag(flags);
        }

        public unsafe bool IsFlag(int index, LifeGameFlags flags)
        {
            var ptr = (LifeGameEntity*)_data.GetUnsafePtr();
            return ptr[index].IsFlag(flags);
        }

        public DataChunk(int xLength, int yLength, int zLength)
        {
            this.xLength = xLength;
            this.yLength = yLength;
            this.zLength = zLength;

            _data = new NativeArray<LifeGameEntity>(this.xLength * this.yLength * this.zLength, Allocator.Persistent);
        }

        private DataChunk(int xLength, int yLength, int zLength, NativeArray<LifeGameEntity> d)
        {
            this.xLength = xLength;
            this.yLength = yLength;
            this.zLength = zLength;
            _data = d;
        }

        public DataChunk Get2DimArray(int y)
        {
            var start = (xLength * zLength * y);
            var length = (xLength * zLength);

            return new DataChunk(xLength, 0, zLength, _data.GetSubArray(start, length));
        }

        public void Dispose()
        {
            _data.Dispose();
        }
    }
}
