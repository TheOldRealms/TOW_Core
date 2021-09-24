using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace TOW_Core.Battle.TriggeredEffect.Scripts
{
    public class SummonSkeleton : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent)
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
            BasicCharacterObject troopCharacter = MBObjectManager.Instance.GetObject<BasicCharacterObject>("tow_skeleton_recruit");
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
            MatrixFrame frame = new MatrixFrame(Mat3.Identity, position);
            Formation formation = null;
            if (caster.Team.Formations.Count() > 0)
            {
                formation = caster.Team.Formations.Where(x => x.FormationIndex.GetName() == troopCharacter.DefaultFormationClass.GetName()).FirstOrDefault();
            }
            AgentBuildData buildData = new AgentBuildData(troopCharacter).
                Team(caster.Team).
                Formation(formation).
                ClothingColor1(caster.Team.Color).
                ClothingColor2(caster.Team.Color2).
                TroopOrigin(troopOrigin).
                CivilianEquipment(Mission.Current.DoesMissionRequireCivilianEquipment).
                IsReinforcement(true).
                SpawnOnInitialPoint(true).
                InitialFrame(frame);
            Agent troop = Mission.Current.SpawnAgent(buildData, false, 1);
            troop.SetWatchState(Agent.WatchState.Alarmed);
        }
    }
}
