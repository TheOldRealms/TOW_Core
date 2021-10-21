using HarmonyLib;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.ObjectSystem;
using TOW_Core.Abilities;

namespace TOW_Core.Battle.TriggeredEffect.Scripts
{
    public class SummonSkeleton : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent)
        {
            IAgentOriginBase agentOrigin = triggeredByAgent.Origin;
            IMissionTroopSupplier supplier = null;
            if (Game.Current.GameType is Campaign)
            {
                supplier = Traverse.Create(agentOrigin).Field("_supplier").GetValue<PartyGroupTroopSupplier>();
            }
            else if (Game.Current.GameType is CustomGame)
            {
                supplier = Traverse.Create(agentOrigin).Field("_troopSupplier").GetValue<CustomBattleTroopSupplier>();
            }
            if (supplier != null)
            {
                Traverse.Create(supplier).Field("_numAllocated").SetValue(supplier.NumActiveTroops + 5);
            }
            var formation = triggeredByAgent.Team.Formations.FirstOrDefault(f => f.PrimaryClass == FormationClass.Infantry);
            var skeleton = SpawnAgent(triggeredByAgent, supplier, formation, position);
            var pos2 = new Vec3(position.X + 1f, position.Y + 1f);
            var skeleton2 = SpawnAgent(triggeredByAgent, supplier, formation, pos2);
            var pos3 = new Vec3(position.X - 1f, position.Y + 1f);
            var skeleton3 = SpawnAgent(triggeredByAgent, supplier, formation, pos3);
            var pos4 = new Vec3(position.X + 1f, position.Y - 1f);
            var skeleton4 = SpawnAgent(triggeredByAgent, supplier, formation, pos4);
            var pos5 = new Vec3(position.X - 1f, position.Y - 1f);
            var skeleton5 = SpawnAgent(triggeredByAgent, supplier, formation, pos5);

            if (Game.Current.GameType is Campaign)
            {
                var manager = Mission.Current.GetMissionBehaviour<AbilityManagerMissionLogic>();
                skeleton.OnAgentHealthChanged += (agent, oldHealth, newHealth) => CheckState(agent, newHealth);
                skeleton2.OnAgentHealthChanged += (agent, oldHealth, newHealth) => CheckState(agent, newHealth);
                skeleton3.OnAgentHealthChanged += (agent, oldHealth, newHealth) => CheckState(agent, newHealth);
                skeleton4.OnAgentHealthChanged += (agent, oldHealth, newHealth) => CheckState(agent, newHealth);
                skeleton5.OnAgentHealthChanged += (agent, oldHealth, newHealth) => CheckState(agent, newHealth);
                manager.SummonedCreatures.Add(skeleton, supplier as PartyGroupTroopSupplier);
                manager.SummonedCreatures.Add(skeleton2, supplier as PartyGroupTroopSupplier);
                manager.SummonedCreatures.Add(skeleton3, supplier as PartyGroupTroopSupplier);
                manager.SummonedCreatures.Add(skeleton4, supplier as PartyGroupTroopSupplier);
                manager.SummonedCreatures.Add(skeleton5, supplier as PartyGroupTroopSupplier);
            }
        }

        private Agent SpawnAgent(Agent caster, IMissionTroopSupplier supplier, Formation formation, Vec3 position)
        {
            BasicCharacterObject troopCharacter = MBObjectManager.Instance.GetObject<BasicCharacterObject>("tow_summoned_skeleton");
            IAgentOriginBase troopOrigin = null;
            if (Game.Current.GameType is Campaign)
            {
                troopOrigin = new PartyAgentOrigin(MobileParty.MainParty.Party, CharacterObject.FindFirst(x => x.StringId == troopCharacter.StringId), 1, new UniqueTroopDescriptor(1), false);
            }
            else
            {
                troopOrigin = new CustomBattleAgentOrigin((CustomBattleCombatant)caster.Origin.BattleCombatant, troopCharacter, supplier as CustomBattleTroopSupplier, !caster.Team.IsEnemyOf(Mission.Current.PlayerTeam));
            }
            formation = formation == null ? new Formation(caster.Team, 1) : formation;
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
            return troop;
        }
        
        private void CheckState(Agent agent, float newHealth)
        {
            if (newHealth <= 0)
            {
                var manager = Mission.Current.GetMissionBehaviour<AbilityManagerMissionLogic>();
                PartyGroupTroopSupplier supplier = manager.SummonedCreatures.FirstOrDefault(sm => sm.Key == agent).Value;
                if (supplier != null)
                {
                    var num = Traverse.Create(supplier).Field("_numKilled").GetValue<int>();
                    Traverse.Create(supplier).Field("_numKilled").SetValue(num + 1);
                }
            }
        }
    }
}
