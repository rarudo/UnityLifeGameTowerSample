using LifeGame3D.ExampleCode.Jobs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.PerformanceTesting;
using Unity.Profiling.Memory;
using UnityEngine;
using UnityEngine.Profiling;
using Vector3 = UnityEngine.Vector3;

namespace LifeGame3D.ExampleCode
{
    public class LifeGameClass
    {
        public Vector3 position;
    }

    public struct LifeGameStruct
    {
        public float3 position;
    }

    public struct LifeGameCell
    {
        public bool isAlive;
        public Vector3 position;
    }


    public struct LifeGame
    {
        public bool isAlive;
        public Vector3 position;
    }
    

    public class ClassOrStruct
    {
        private const int Limit = 1000000;


        [Test]
        [Performance]
        public void CheckClassTest()
        {
            Measure.Method(CheckClass)
                .WarmupCount(1) // ウォームアップとして計測する回数
                .MeasurementCount(100) // 計測する回数
                .IterationsPerMeasurement(1) // 1回の計測で計測対象を実行する回数
                .CleanUp(() => { }) // 計測対象の実行直後に毎回実行される
                .Run();
        }

        [Test]
        [Performance]
        public void CheckStructTest()
        {
            Measure.Method(CheckStruct)
                .WarmupCount(1) // ウォームアップとして計測する回数
                .MeasurementCount(100) // 計測する回数
                .IterationsPerMeasurement(1) // 1回の計測で計測対象を実行する回数
                .CleanUp(() => { }) // 計測対象の実行直後に毎回実行される
                .Run();
        }

        [Test]
        [Performance]
        public void CheckNativeArrayBurstTest()
        {
            Measure.Method(CheckNativeArrayBurst)
                .WarmupCount(1) // ウォームアップとして計測する回数
                .MeasurementCount(100) // 計測する回数
                .IterationsPerMeasurement(1) // 1回の計測で計測対象を実行する回数
                .CleanUp(() =>
                    new NativeArray<LifeGameStruct>(Limit, Allocator.Persistent).Dispose()) // 計測対象の実行直後に毎回実行される
                .Run();
        }
        
        [Test]
        [Performance]
        public void CheckStructMemoryUsage()
        {
            Measure.Method(CheckStructMemory)
                .WarmupCount(1) // ウォームアップとして計測する回数
                .MeasurementCount(100) // 計測する回数
                .IterationsPerMeasurement(1) // 1回の計測で計測対象を実行する回数
                .Run();
        }
        
        [Test]
        [Performance]
        public void CheckUintMemoryUsage()
        {
            Measure.Method(CheckUintMemory)
                .WarmupCount(1) // ウォームアップとして計測する回数
                .MeasurementCount(100) // 計測する回数
                .IterationsPerMeasurement(1) // 1回の計測で計測対象を実行する回数
                .Run();
        }
        
        [Test]
        [Performance]
        public void CheckNativeArrayTest()
        {
            Measure.Method(CheckNativeArray)
                .WarmupCount(1) // ウォームアップとして計測する回数
                .MeasurementCount(100) // 計測する回数
                .IterationsPerMeasurement(1) // 1回の計測で計測対象を実行する回数
                .CleanUp(() =>
                    new NativeArray<LifeGameStruct>(Limit, Allocator.Persistent).Dispose()) // 計測対象の実行直後に毎回実行される
                .Run();
        }

        private void CheckStruct()
        {
            var structs = new LifeGameStruct[Limit];
            
            for (var index = 0; index < structs.Length; index++)
            {
                structs[index] = new LifeGameStruct()
                {
                    position = Vector3.one
                };
            }

            var position = new float3();
            foreach (var t in structs)
            {
                position += t.position;
            }

            Debug.Log(position);
        }
        
        private void CheckStructMemory()
        {
            var structs = new LifeGameCell[Limit];

            LifeGameCell c;
            foreach (var t in structs)
            {
                c = t;
            }
        }
        
        private void CheckUintMemory()
        {
            var uints = new uint[Limit];

            uint u = 0u;
            foreach (var t in uints)
            {
                u = t;
            }
        }

        private void CheckClass()
        {
            var classes = new LifeGameClass[Limit];
            
            for (var index = 0; index < classes.Length; index++)
            {
                classes[index] = new LifeGameClass()
                {
                    position = Vector3.one
                };
            }

            var position = new Vector3();
            foreach (var t in classes)
            {
                position += t.position;
            }

            Debug.Log(position);
        }

        private void CheckNativeArrayBurst()
        {
            var nativeArrayStructs = new NativeArray<LifeGameStruct>(Limit, Allocator.TempJob);
            var createJob = new CreateJob()
            {
                output = nativeArrayStructs
            }.Schedule(Limit, 128);

            var result = new NativeReference<float3>(Vector3.zero, Allocator.TempJob);
            var sumJob = new SumJob()
            {
                input = nativeArrayStructs,
                result = result
            }.Schedule(createJob);
            sumJob.Complete();

            Debug.Log(result.Value);

            nativeArrayStructs.Dispose();
            result.Dispose();
        }
        
        
        
        private void CheckNativeArray()
        {
            var nativeArrayStructs = new NativeArray<LifeGameStruct>(Limit, Allocator.TempJob);
            for (var index = 0; index < nativeArrayStructs.Length; index++)
            {
                nativeArrayStructs[index] = new LifeGameStruct()
                {
                    position = Vector3.one
                };
            }

            var position = new float3();
            foreach (var t in nativeArrayStructs)
            {
                position += t.position;
            }

            Debug.Log(position);
        }
    }
}
