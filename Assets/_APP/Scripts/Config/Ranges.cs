using System;
using UnityEngine;

namespace DWS
{
    [Serializable]
    public struct IntRange
    {
        public int min;
        public int max;

        public IntRange(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public int ClampMinMax()
        {
            if (max < min) max = min;
            return max;
        }

        public int RandomInclusive()
        {
            if (max < min) max = min;
            // UnityEngine.Random.Range for int is min inclusive, max exclusive.
            return UnityEngine.Random.Range(min, max + 1);
        }

        public static IntRange Lerp(IntRange a, IntRange b, float t)
        {
            t = Mathf.Clamp01(t);
            return new IntRange(
                Mathf.RoundToInt(Mathf.Lerp(a.min, b.min, t)),
                Mathf.RoundToInt(Mathf.Lerp(a.max, b.max, t))
            );
        }

        public override string ToString() => $"[{min}..{max}]";
    }

    [Serializable]
    public struct FloatRange
    {
        public float min;
        public float max;

        public FloatRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public float Random()
        {
            if (max < min) max = min;
            return UnityEngine.Random.Range(min, max);
        }

        public static FloatRange Lerp(FloatRange a, FloatRange b, float t)
        {
            t = Mathf.Clamp01(t);
            return new FloatRange(
                Mathf.Lerp(a.min, b.min, t),
                Mathf.Lerp(a.max, b.max, t)
            );
        }

        public override string ToString() => $"[{min:0.###}..{max:0.###}]";
    }
}
