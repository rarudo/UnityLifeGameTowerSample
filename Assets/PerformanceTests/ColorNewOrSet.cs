using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;

namespace LifeGame3D.ExampleCode
{
    public class ColorNewOrSet
    {
        public Color color;

        public void NewColorAlpha1(float alpha)
        {
            color = new Color(color.r, color.g, color.b, alpha);
        }
        
        public void NewColorAlpha2(float alpha)
        {
            var c = color;
            c.a = alpha;
            color = c;
        }

        public void CheckColorAlpha(float alpha)
        {
            if (!Mathf.Approximately(color.a, alpha))
            {
                color = new Color(color.r, color.g, color.b, alpha);
            }
        }

        [Test]
        [Performance]
        public void NewColorAlpha1Test()
        {
            Measure.Method(() => NewColorAlpha1(0f))
                .WarmupCount(10) // ウォームアップとして計測する回数
                .MeasurementCount(10) // 計測する回数
                .IterationsPerMeasurement(10000) // 1回の計測で計測対象を実行する回数
                .Run();

        }
        
        [Test]
        [Performance]
        public void NewColorAlpha2Test()
        {
            Measure.Method(() => NewColorAlpha2(0f))
                .WarmupCount(10) // ウォームアップとして計測する回数
                .MeasurementCount(10) // 計測する回数
                .IterationsPerMeasurement(10000) // 1回の計測で計測対象を実行する回数
                .Run();
        }

        [Test]
        [Performance]
        public void CheckColorAlphaTest()
        {
            Measure.Method(() => CheckColorAlpha(0f))
                .WarmupCount(10) // ウォームアップとして計測する回数
                .MeasurementCount(10) // 計測する回数
                .IterationsPerMeasurement(10000) // 1回の計測で計測対象を実行する回数
                .Run();
        }
    }
}

