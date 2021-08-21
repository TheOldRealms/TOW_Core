using System;

namespace TOW_Core.Battle.AI.Decision
{
    public static class ScoringFunctions
    {
        public static Func<float, float> Logistic(float mid = 0.0f, float k = 1.0f, Func<float, float> input = null)
        {
            if (input == null)
            {
                return x => (float) (1 / (1 + Math.Pow(Math.E, -k * (x - mid))));
            }

            return x => (float) (1 / (1 + Math.Pow(Math.E, -k * (input.Invoke(x) - mid))));
        }
    }
}