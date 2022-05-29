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
            _targetType = _template.AbilityTargetType;
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
            if (_caster != null)
            {
                UpdatePosition();
                if (Targets != null)
                {
                    _previousTargets = (Agent[])Targets.Clone();
                }
                UpdateTargets();
                UpdateAgentsGlow();
                Rotate();
                ChangeColor();
            }
        }

        public override void Hide()
        {
            base.Hide();
            ClearArrays();
        }

        private void UpdatePosition()
        {
            if (_caster != null)
            {
                if (_missionScreen.GetProjectedMousePositionOnGround(out _position, out _normal, true))
                {
                    _currentDistance = _caster.Position.Distance(_position);
                    if (_currentDistance > _template.MaxDistance)
                    {
                        _position = _caster.LookFrame.Advance(_template.MaxDistance).origin;
                        _position.z = _mission.Scene.GetGroundHeightAtPosition(Position);
                    }
                    Position = _position;
                }
                else
                {
                    _position = _caster.LookFrame.Advance(_template.MaxDistance).origin;
                    _position.z = _mission.Scene.GetGroundHeightAtPosition(Position);
                    Position = _position;
                }
            }
        }

        private void UpdateTargets()
        {
            switch (_targetType)
            {
                case AbilityTargetType.AlliesInAOE:
                    {
                        Targets = _mission.GetNearbyAllyAgents(Position.AsVec2, 5, _mission.PlayerTeam).ToArray();
                        break;
                    }
                case AbilityTargetType.EnemiesInAOE:
                    {
                        Targets = _mission.GetNearbyEnemyAgents(Position.AsVec2, 5, _mission.PlayerTeam).ToArray();
                        break;
                    }
            }
        }

        private void UpdateAgentsGlow()
        {
            if (Targets != null)
            {
                for (int i = 0; i < Targets.Length; i++)
                {
                    var agent = Targets[i];
                    if (agent.State == TaleWorlds.Core.AgentState.Active || agent.State == TaleWorlds.Core.AgentState.Routed)
                    {
                        switch (_targetType)
                        {
                            case AbilityTargetType.AlliesInAOE:
                                {
                                    agent.AgentVisuals.GetEntity().Root.SetContourColor(friendColor, true);
                                    break;
                                }
                            case AbilityTargetType.EnemiesInAOE:
                                {
                                    agent.AgentVisuals.GetEntity().Root.SetContourColor(enemyColor, true);
                                    break;
                                }
                        }
                    }
                }
            }
            if (_previousTargets != null)
            {
                foreach (Agent agent in _previousTargets.Except(Targets))
                    agent.AgentVisuals.GetEntity().Root.SetContourColor(colorLess, true);
            }
        }

        private void ClearArrays()
        {
            if (Targets != null)
                foreach (Agent agent in Targets)
                    agent.AgentVisuals.GetEntity().Root.SetContourColor(colorLess, true);
            if (_previousTargets != null)
                foreach (Agent agent in _previousTargets.Except(Targets))
                    agent.AgentVisuals.GetEntity().Root.SetContourColor(colorLess, true);
            _previousTargets = null;
            Targets = null;
        }

        public Agent[] Targets { get; private set; }

        private Agent[] _previousTargets;

        private float _currentDistance;

        private Vec3 _position;

        private Vec3 _normal;

        private AbilityTargetType _targetType;
    }
}
