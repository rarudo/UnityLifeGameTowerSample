using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace LifeGame3D.Job
{
    /// <summary>
    /// GreedyMeshアルゴリズムでメッシュを結合する
    /// </summary>
    [BurstCompile]
    public struct GreedyMeshJob : IJob
    {
        public DataChunk input;
        public int height;

        [ReadOnly] public LifeGameUtil.LifeGameMesh referenceMesh;

        [WriteOnly] public NativeList<LifeGameUtil.VerticesData> verticesData;
        [WriteOnly] public NativeList<int> triangles;

        private int _lastVertexIndex;

        public void Execute()
        {
            // xz平面だけで作ってみる
            for (var index = 0; index < input.Length; index++)
            {
                if (!input.IsFlag(index, LifeGameFlags.Alive)) continue;
                if (input.IsFlag(index, LifeGameFlags.Masked)) continue;

                GetFaceLength(ref input, index, out int xLength, out int zLength);
                index += xLength;

                input.GetIndex(index, out int x, out _, out int z);
                CreateCube(x, height, z, xLength, zLength, zLength);
            }
        }

        private void CreateCube(int x, int y, int z, int xLength, int yLength, int zLength)
        {
            var trs = float4x4.TRS(new float3(x, y, z), quaternion.identity, new float3(xLength, yLength, zLength));

            for (var index = 0; index < referenceMesh.vertices.Length; index++)
            {
                var cv = referenceMesh.vertices[index];
                var v = math.mul(trs, new float4(cv.x, cv.y, cv.z, 1f));
                verticesData.Add(new LifeGameUtil.VerticesData()
                {
                    positions = new float3(v.x, v.y, v.z),
                    normals = referenceMesh.normals[index],
                    uv = referenceMesh.uv[index] 
                });
            }

            foreach (var a in referenceMesh.triangles)
            {
                triangles.Add(_lastVertexIndex + a);
            }

            _lastVertexIndex += referenceMesh.vertices.Length;
        }

        private void GetFaceLength(ref DataChunk chunkInput, int index, out int xLength, out int zLength)
        {
            uint maskFlag = (uint)LifeGameFlags.Masked;
            xLength = 1;
            zLength = 1;

            chunkInput.GetIndex(index, out int x, out _, out int z);

            // X軸での長さ
            while (x + xLength < chunkInput.xLength)
            {
                var xD = x + xLength;
                if (chunkInput.IsFlag(xD, 0, z, (LifeGameFlags)maskFlag)) break;
                if (!chunkInput.IsFlag(xD, 0, z, LifeGameFlags.Alive)) break;
                chunkInput.AddFlag(xD, 0, z, LifeGameFlags.Masked);
                xLength++;
            }

            // Z軸での長さ
            while (z + zLength < chunkInput.zLength)
            {
                var zD = z + zLength;
                bool extendHeight = false;
                for (int xOffset = 0; xOffset < xLength; xOffset++)
                {
                    int xD = x + xOffset;
                    if (chunkInput.IsFlag(xD,0 , zD, (LifeGameFlags)maskFlag)) break;
                    if (!chunkInput.IsFlag(xD, 0, zD, LifeGameFlags.Alive)) break;
                    // 最後まで捜査が終わったら
                    if (xOffset + 1 == xLength)
                    {
                        extendHeight = true;
                    }
                }

                if (!extendHeight) break;

                for (int xOffset = 0; xOffset < xLength; xOffset++)
                {
                    int xD = x + xOffset;
                    chunkInput.AddFlag(xD, 0, zD, LifeGameFlags.Masked);
                }
                zLength++;
            }
        }

    }
}
