using System;
using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Crosshairs
{
    public class TargetedAOECrosshair : AbilityCrosshair
    {
        public TargetedAOECrosshair(AbilityTemplate template) : base(template)
        {
            _crosshair.EntityFlags |= EntityFlags.NotAffectedBySeason;
            UpdateFrame();
            //MatrixFrame frame = crosshair.GetFrame();
            //frame.Scale(new Vec3(10f, 10f, 1f, -1f));
            //crosshair.SetFrame(ref frame);
            IsVisible = false;
        }
        public override void Tick()
        {
            UpdateFrame();
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
        private void UpdateFrame()
        {
            Vec3 position;
            Vec3 vec;
            if (this._missionScreen.GetProjectedMousePositionOnGround(out position, out vec, true))
            {
                Position = position;
            }
            else
            {
                Position = new Vec3(0f, 0f, -100000f, -1f);
            }
        }
        private void UpdateColliedeAgents(TargetType targetType)
        {
            if (targetType == TargetType.All)
                CollidedAgents = _mission.GetNearbyAgents(Position.AsVec2, 5).ToArray();
            else if (targetType == TargetType.Friendly)
                CollidedAgents = _mission.GetNearbyAllyAgents(Position.AsVec2, 5, _mission.PlayerAllyTeam).ToArray();
            else if (targetType == TargetType.Enemy)
                CollidedAgents = _mission.GetNearbyEnemyAgents(Position.AsVec2, 5, _mission.PlayerEnemyTeam).ToArray();
        }
        public void ClearArrays()
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
        public void SetVisibilty(bool visibility)
        {
            this._crosshair.SetVisibilityExcludeParents(visibility);
        }

        public static bool IsPositionOnValidGround(WorldPosition worldPosition)
        {
            return Mission.Current.IsFormationUnitPositionAvailable(ref worldPosition, Mission.Current.PlayerTeam);
        }
        public Agent[] CollidedAgents { get; private set; }

        private Agent[] previousAgents;
    }
}
