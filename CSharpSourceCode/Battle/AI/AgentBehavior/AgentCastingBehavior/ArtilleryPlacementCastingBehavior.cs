using System;
using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.AgentBehavior.AgentTacticalBehavior;
using TOW_Core.Battle.AI.Decision;
using TOW_Core.Battle.Artillery;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public class ArtilleryPlacementCastingBehavior : AbstractAgentCastingBehavior
    {
        private static Random _random = new Random();

        public ArtilleryPlacementCastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
            Hysteresis = 0.1f;
            TacticalBehavior = new AdjacentAoETacticalBehavior(agent, Component, this);
        }

        public override void Execute()
        {
            var castingPosition = ((AdjacentAoETacticalBehavior) TacticalBehavior)?.CastingPosition;
            if (castingPosition.HasValue && Agent.Position.AsVec2.Distance(castingPosition.Value.AsVec2) > 5) return;

            base.Execute();
        }

        protected override Target UpdateTarget(Target target)
        {
            var width = target.TacticalPosition.Width / 1.5f;
            var direction = target.TacticalPosition.Position.GetGroundVec3() - Agent.Team.QuerySystem.AverageEnemyPosition.ToVec3();
            direction /= direction.Length;
            target.SelectedWorldPosition = target.TacticalPosition.Position.GetGroundVec3() + direction.AsVec2.RightVec().ToVec3() * (float) (_random.NextDouble() * width - width / 2);
            return target;
        }

        protected override bool HaveLineOfSightToTarget(Target target)
        {
            var activeEntitiesWithScriptComponentOfType = Mission.Current.GetActiveEntitiesWithScriptComponentOfType<ArtilleryRangedSiegeWeapon>();
            return !activeEntitiesWithScriptComponentOfType.Any(entity => entity.GlobalPosition.Distance(target.SelectedWorldPosition) < 3.5);
        }
    }
}