﻿using HarmonyLib;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Decision;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.AI.AgentBehavior.AgentCastingBehavior
{
    public class CenteredStaticAoEAgentCastingBehavior : AbstractAgentCastingBehavior
    {
        public CenteredStaticAoEAgentCastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
        }

        public override void Execute()
        {
            var castingPosition = CalculateCastingPosition(CurrentTarget.Formation);
            var worldPosition = new WorldPosition(Mission.Current.Scene, castingPosition);
            Agent.SetScriptedPosition(ref worldPosition, false);

            if (Agent.Position.AsVec2.Distance(castingPosition.AsVec2) > 5) return;

            base.Execute();
        }

        public override void Terminate()
        {
            Agent.DisableScriptedMovement();
        }

        protected override bool HaveLineOfSightToAgent(Agent targetAgent)
        {
            return true;
        }

        private static Vec3 CalculateCastingPosition(Formation targetFormation)
        {
            var medianPositionPosition = targetFormation.QuerySystem.MedianPosition.Position;
            return medianPositionPosition + (targetFormation.Direction * targetFormation.GetMovementSpeedOfUnits()).ToVec3(medianPositionPosition.z);
        }

        protected override float UtilityFunction(Target target)
        {
            AxisList.Do(axis => TOWCommon.Say(axis.Evaluate(Agent, target).ToString()));
            return base.UtilityFunction(target);
        }

        public override bool IsPositional()
        {
            return true;
        }
    }
}