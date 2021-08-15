namespace TOW_Core.Battle.AI.Decision.Scoring
{
    public abstract class ScoringFunction
    {
        public float Min = 0;
        public float Max = 0;
        public abstract float Evaluate(float x);
    }
}