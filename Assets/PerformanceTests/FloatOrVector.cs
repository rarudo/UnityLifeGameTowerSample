using NUnit.Framework;
using Unity.Mathematics;
using Unity.PerformanceTesting;
using UnityEngine;

namespace LifeGame3D.ExampleCode
{
    public class FloatOrVector
    {
        public static readonly int Limit = 10000000;
        
        [Test]
        [Performance]
        public void CheckFloat()
        {
            Measure.Method(CheckFloatInternal)
                .WarmupCount(1) // ウォームアップとして計測する回数
                .MeasurementCount(100) // 計測する回数
                .IterationsPerMeasurement(1) // 1回の計測で計測対象を実行する回数
                .CleanUp(() => { }) // 計測対象の実行直後に毎回実行される
                .Run();
        }
        [Test]
        [Performance]
        public void CheckVector()
        {
            Measure.Method(CheckVectorInternal)
                .WarmupCount(1) // ウォームアップとして計測する回数
                .MeasurementCount(100) // 計測する回数
                .IterationsPerMeasurement(1) // 1回の計測で計測対象を実行する回数
                .CleanUp(() => { }) // 計測対象の実行直後に毎回実行される
                .Run();
        }

        private void CheckFloatInternal()
        {
            var floatArray = new float3[Limit];

            float3 f = new float3(0, 0, 0);
            foreach (var float3 in floatArray)
            {
                f += float3;
            }

            {
                Quaternion q1 = Quaternion.identity;
                Quaternion q2 = Quaternion.identity;
                Quaternion q3 = Quaternion.identity;

                var a = q1 * q2 * q3;
            }

            {
                quaternion q1 = quaternion.identity;
                quaternion q2 = quaternion.identity;
                quaternion q3 = quaternion.identity;

                var a = math.mul(math.mul(q1, q2), q3);
            }

            var b = new float4(0, 0, 0,0) * new float4(0, 0, 0,0);
            
        }
        
        private void CheckVectorInternal()
        {
            var vectorArray = new Vector3[Limit];

            Vector3 v = new Vector3(0, 0, 0);
            foreach (var vector3 in vectorArray)
            {
                v += vector3;
            }
        }
    }
}
