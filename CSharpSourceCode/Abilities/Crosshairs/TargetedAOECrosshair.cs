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
            InitializeColors();
            AddLight();
            IsVisible = false;
        }

        public override void Tick()
        {
            UpdateFrame();
            if (Targets != null)
            {
                previousTargets = (Agent[])Targets.Clone();
            }
            UpdateTargets(_template.AbilityTargetType);
            UpdateAgentsGlow();
            Rotate();
            ChangeColor();
        }

        public override void Hide()
        {
            base.Hide();
            ClearArrays();
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

        private void UpdateTargets(AbilityTargetType targetType)
        {
            switch (targetType)
            {
                case AbilityTargetType.All:
                    {
                        Targets = _mission.GetNearbyAgents(Position.AsVec2, 5).ToArray();
                        break;
                    }
                case AbilityTargetType.Allies:
                    {
                        Targets = _mission.GetNearbyAllyAgents(Position.AsVec2, 5, _mission.PlayerAllyTeam).ToArray();
                        break;
                    }
                case AbilityTargetType.Enemies:
                    {
                        Targets = _mission.GetNearbyEnemyAgents(Position.AsVec2, 5, _mission.PlayerEnemyTeam).ToArray();
                        break;
                    }
            }
        }

        private void UpdateAgentsGlow()
        {
            if (Targets != null)
            {
                foreach (Agent agent in Targets)
                    if (agent.State == TaleWorlds.Core.AgentState.Active || agent.State == TaleWorlds.Core.AgentState.Routed)
                        agent.AgentVisuals.GetEntity().Root.SetContourColor(friendColor, true);
            }
            if (previousTargets != null)
            {
                foreach (Agent agent in previousTargets.Except(Targets))
                    agent.AgentVisuals.GetEntity().Root.SetContourColor(colorLess, true);
            }
        }

        private void ClearArrays()
        {
            if (Targets != null)
                foreach (Agent agent in Targets)
                    agent.AgentVisuals.GetEntity().Root.SetContourColor(colorLess, true);
            if (previousTargets != null)
                foreach (Agent agent in previousTargets.Except(Targets))
                    agent.AgentVisuals.GetEntity().Root.SetContourColor(colorLess, true);
            previousTargets = null;
            Targets = null;
        }

        public Agent[] Targets { get; private set; }

        private Agent[] previousTargets;
    }
}
