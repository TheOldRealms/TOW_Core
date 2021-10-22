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
    public class SummonScript : ITriggeredScript
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
            var data = GetAgentBuildData(triggeredByAgent, supplier);

            var skeleton = SpawnAgent(data, position);
            var pos2 = new Vec3(position.X + 1f, position.Y + 1f);
            var skeleton2 = SpawnAgent(data, pos2);
            var pos3 = new Vec3(position.X - 1f, position.Y + 1f);
            var skeleton3 = SpawnAgent(data, pos3);
            var pos4 = new Vec3(position.X + 1f, position.Y - 1f);
            var skeleton4 = SpawnAgent(data, pos4);
            var pos5 = new Vec3(position.X - 1f, position.Y - 1f);
            var skeleton5 = SpawnAgent(data, pos5);

            if (Game.Current.GameType is Campaign)
            {
                var manager = Mission.Current.GetMissionBehaviour<AbilityManagerMissionLogic>();
                manager.AddSummonedCreature(skeleton, supplier as PartyGroupTroopSupplier);
                manager.AddSummonedCreature(skeleton2, supplier as PartyGroupTroopSupplier);
                manager.AddSummonedCreature(skeleton3, supplier as PartyGroupTroopSupplier);
                manager.AddSummonedCreature(skeleton4, supplier as PartyGroupTroopSupplier);
                manager.AddSummonedCreature(skeleton5, supplier as PartyGroupTroopSupplier);
            }
        }

        private AgentBuildData GetAgentBuildData(Agent caster, IMissionTroopSupplier supplier)
        {
            BasicCharacterObject troopCharacter = MBObjectManager.Instance.GetObject<BasicCharacterObject>(CreatureName);
            IAgentOriginBase troopOrigin = null;
            if (Game.Current.GameType is Campaign)
            {
                troopOrigin = new PartyAgentOrigin(MobileParty.MainParty.Party, CharacterObject.FindFirst(x => x.StringId == troopCharacter.StringId), 1, new UniqueTroopDescriptor(1), false);
            }
            else
            {
                troopOrigin = new CustomBattleAgentOrigin((CustomBattleCombatant)caster.Origin.BattleCombatant, troopCharacter, supplier as CustomBattleTroopSupplier, !caster.Team.IsEnemyOf(Mission.Current.PlayerTeam));
            }
            var formation = caster.Team.Formations.FirstOrDefault(f => f.PrimaryClass == FormationClass.Infantry);
            if (formation == null)
            {
                formation = caster.Formation;
            }
            AgentBuildData buildData = new AgentBuildData(troopCharacter).
                Team(caster.Team).
                Formation(formation).
                ClothingColor1(caster.Team.Color).
                ClothingColor2(caster.Team.Color2).
                Equipment(troopCharacter.GetFirstEquipment(false)).
                TroopOrigin(troopOrigin).
                IsReinforcement(true).
                InitialDirection(Vec2.Forward);
            return buildData;
        }

        private Agent SpawnAgent(AgentBuildData buildData, Vec3 position)
        {
            Agent troop = Mission.Current.SpawnAgent(buildData, false, 1);
            troop.TeleportToPosition(position);
            troop.FadeIn();
            troop.SetWatchState(Agent.WatchState.Alarmed);
            return troop;
        }


        public string CreatureName { get; internal set; }
    }
}
