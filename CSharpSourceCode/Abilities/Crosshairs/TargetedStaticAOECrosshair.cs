using System;
using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Crosshairs
{
    public class TargetedStaticAOECrosshair : AbilityCrosshair
    {
        public TargetedStaticAOECrosshair()
        {
            crosshair.EntityFlags |= EntityFlags.NotAffectedBySeason;
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
            if (this.missionScreen.GetProjectedMousePositionOnGround(out position, out vec, true))
            {
                WorldPosition worldPosition = new WorldPosition(Mission.Current.Scene, UIntPtr.Zero, position, false);
                //isOnValidGround = (!this.IsVisible || AOEAbilityCrosshair.IsPositionOnValidGround(worldPosition));
            }
            else
            {
                //isOnValidGround = false;
                position = new Vec3(0f, 0f, -100000f, -1f);
            }
            this.Position = position;
        }
        private void UpdateColliedeAgents(TargetType targetType)
        {
            if (targetType == TargetType.All)
                CollidedAgents = mission.GetNearbyAgents(Position.AsVec2, 5).ToArray();
            else if (targetType == TargetType.Friendly)
                CollidedAgents = mission.GetNearbyAllyAgents(Position.AsVec2, 5, mission.PlayerAllyTeam).ToArray();
            else if (targetType == TargetType.Enemy)
                CollidedAgents = mission.GetNearbyEnemyAgents(Position.AsVec2, 5, mission.PlayerEnemyTeam).ToArray();
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
            this.crosshair.SetVisibilityExcludeParents(visibility);
        }

        public static bool IsPositionOnValidGround(WorldPosition worldPosition)
        {
            return Mission.Current.IsFormationUnitPositionAvailable(ref worldPosition, Mission.Current.PlayerTeam);
        }
        public Agent[] CollidedAgents { get; private set; }

        private Agent[] previousAgents;
    }
}
