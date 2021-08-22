using System;

namespace TOW_Core.Battle.AI.Decision
{
    public static class ScoringFunctions
    {
        public static Func<float, float> Logistic(float mid = 0.0f, float L = 1.0f, float k = 10, float m = 1)
        {
            return x =>
            {
                var pow = (float) (L / (1 + m * Math.Pow(Math.E, -k * (x - mid))));
                return pow;
            };
        }
    }
}