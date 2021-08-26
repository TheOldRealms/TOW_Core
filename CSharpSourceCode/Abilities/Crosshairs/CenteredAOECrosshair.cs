using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Crosshairs
{
    public class CenteredAOECrosshair : AbilityCrosshair
    {
        public CenteredAOECrosshair(AbilityTemplate template, Agent caster) : base(template)
        {
            _caster = caster;
            _crosshair = GameEntity.Instantiate(Mission.Current.Scene, "targeting_rune_empire", false);
            _crosshair.EntityFlags |= EntityFlags.NotAffectedBySeason;
            MatrixFrame frame = _crosshair.GetFrame();
            frame.Scale(new Vec3(template.TargetCapturingRadius, template.TargetCapturingRadius, 1, -1));
            _crosshair.SetFrame(ref frame);
            InitializeColors();
            AddLight();
            IsVisible = false;
        }

        public override void Tick()
        {
            if (!_isBound)
            {
                if (_caster.AgentVisuals != null)
                {
                    _isBound = true;
                    _caster.AgentVisuals.AddChildEntity(_crosshair);
                }
            }
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

        protected void UpdateTargets(AbilityTargetType targetType)
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
                        var playerTeam = _mission.GetNearbyAllyAgents(Position.AsVec2, 5, _mission.PlayerTeam);
                        
                        if (_mission.PlayerAllyTeam != null)
                        {
                            var allyTeam = _mission.GetNearbyAllyAgents(Position.AsVec2, 5, _mission.PlayerAllyTeam);
                            playerTeam.Concat(allyTeam);
                        }
                        
                        Targets = playerTeam.ToArray();
                        break;
                    }
                case AbilityTargetType.Enemies:
                    {
                        Targets = _mission.GetNearbyEnemyAgents(Position.AsVec2, 5, _mission.PlayerEnemyTeam).ToArray();
                        break;
                    }
            }
        }

        protected void UpdateAgentsGlow()
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

        private bool _isBound;

        private Agent _caster;

        private Agent[] previousTargets;
    }
}
