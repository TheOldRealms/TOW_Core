﻿using System;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.AgentBehavior.AgentTacticalBehavior;
using TOW_Core.Battle.AI.Decision;

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
            var width = target.TacticalPosition.Width / 2;
            target.SelectedWorldPosition = target.TacticalPosition.Position.GetGroundVec3() + target.TacticalPosition.Direction.RightVec().ToVec3() * (float) (_random.NextDouble() * width - width / 2);
            return target;
        }
    }
}