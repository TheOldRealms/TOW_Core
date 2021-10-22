namespace TOW_Core.Battle.TriggeredEffect.Scripts
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

{
    public class SummonSkeleton : SummonScript
    {
        public SummonSkeleton()
        {
            CreatureName = "tow_summoned_skeleton";
        }
        
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents)
        {
            SpawnAgent(triggeredByAgent, position);

            var pos2 = new Vec3(position.X + 1f, position.Y + 1f);
            SpawnAgent(triggeredByAgent, pos2);

            var pos3 = new Vec3(position.X - 1f, position.Y + 1f);
            SpawnAgent(triggeredByAgent, pos3);

            var pos4 = new Vec3(position.X + 1f, position.Y - 1f);
            SpawnAgent(triggeredByAgent, pos4);

            var pos5 = new Vec3(position.X - 1f, position.Y - 1f);
            SpawnAgent(triggeredByAgent, pos5);
        }

        private void SpawnAgent(Agent caster, Vec3 position)
        {
            BasicCharacterObject troopCharacter = MBObjectManager.Instance.GetObject<BasicCharacterObject>("tow_summoned_skeleton");
            IAgentOriginBase troopOrigin = null;
            if (Game.Current.GameType is Campaign)
            {
                troopOrigin = new PartyAgentOrigin(MobileParty.MainParty.Party, CharacterObject.FindFirst(x => x.StringId == troopCharacter.StringId), 1, new UniqueTroopDescriptor(1), false);
            }
            else
            {
                var supplier = new CustomBattleTroopSupplier((CustomBattleCombatant)caster.Origin.BattleCombatant, !caster.Team.IsEnemyOf(Mission.Current.PlayerTeam));
                troopOrigin = new CustomBattleAgentOrigin((CustomBattleCombatant)caster.Origin.BattleCombatant, troopCharacter, supplier as CustomBattleTroopSupplier, !caster.Team.IsEnemyOf(Mission.Current.PlayerTeam));
            }
            Formation formation = caster.Team.GetFormation(troopCharacter.GetFormationClass(troopOrigin.BattleCombatant));
            if (formation == default)
            {
                formation = new Formation(caster.Team, caster.Team.Formations.Count());
            }
            AgentBuildData buildData = new AgentBuildData(troopCharacter).
                Team(caster.Team).
                Formation(formation).
                ClothingColor1(caster.Team.Color).
                ClothingColor2(caster.Team.Color2).
                Equipment(troopCharacter.GetFirstEquipment(false)).
                TroopOrigin(troopOrigin).
                IsReinforcement(true).
                InitialPosition(position).
                InitialDirection(Vec2.Forward);
            Agent troop = Mission.Current.SpawnAgent(buildData, false, 1);
            troop.FadeIn();
            troop.SetWatchState(Agent.WatchState.Alarmed);
        }
    }
}
