using System.Linq;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Crosshairs
{
    public abstract class AOECrosshair : AbilityCrosshair
    {
        public AOECrosshair(AbilityTemplate template) : base(template)
        {
        }
        protected void HighlightNearbyAgents()
        {
            if (CollidedAgents != null)
            {
                previousAgents = (Agent[])CollidedAgents.Clone();
            }
            UpdateColliedeAgents(TargetType.All);
            if (CollidedAgents != null)
            {
                foreach (Agent agent in CollidedAgents)
                    if (agent.State == TaleWorlds.Core.AgentState.Active || agent.State == TaleWorlds.Core.AgentState.Routed)
                        agent.AgentVisuals.GetEntity().Root.SetContourColor(friendColor, true);
            }
            if (previousAgents != null)
            {
                foreach (Agent agent in previousAgents.Except(CollidedAgents))
                    agent.AgentVisuals.GetEntity().Root.SetContourColor(colorLess, true);
            }
        }
        private void UpdateColliedeAgents(TargetType targetType)
        {
            if (targetType == TargetType.All)
                CollidedAgents = _mission.GetNearbyAgents(Position.AsVec2, _template.TargetCapturingRadius).ToArray();
            else if (targetType == TargetType.Friendly)
                CollidedAgents = _mission.GetNearbyAllyAgents(Position.AsVec2, _template.TargetCapturingRadius, _mission.PlayerAllyTeam).ToArray();
            else if (targetType == TargetType.Enemy)
                CollidedAgents = _mission.GetNearbyEnemyAgents(Position.AsVec2, _template.TargetCapturingRadius, _mission.PlayerEnemyTeam).ToArray();
        }
        private void ClearArrays()
        {
            if (CollidedAgents != null)
                foreach (Agent agent in CollidedAgents)
                    agent.AgentVisuals.GetEntity().Root.SetContourColor(colorLess, true);
            if (previousAgents != null)
                foreach (Agent agent in previousAgents.Except(CollidedAgents))
                    agent.AgentVisuals.GetEntity().Root.SetContourColor(colorLess, true);
            previousAgents = null;
            CollidedAgents = null;
        }
        public override void Show()
        {
            base.Show();
        }
        public override void Hide()
        {
            base.Hide();
            ClearArrays();
        }

        public Agent[] CollidedAgents { get; private set; }
        private Agent[] previousAgents;
    }
}
