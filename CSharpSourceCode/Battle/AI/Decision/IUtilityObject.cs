namespace TOW_Core.Battle.AI.Decision
{
    public interface IUtilityObject
    {
        float GetLatestScore();
        float CalculateUtility();
    }
}