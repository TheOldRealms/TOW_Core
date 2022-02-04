using System.Collections.Generic;
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
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents)
        {
            var data = GetAgentBuildData(triggeredByAgent);

            var skeleton = SpawnAgent(data, position);
            var pos2 = new Vec3(position.X + 1f, position.Y + 1f);
            var skeleton2 = SpawnAgent(data, pos2);
            var pos3 = new Vec3(position.X - 1f, position.Y + 1f);
            var skeleton3 = SpawnAgent(data, pos3);
            var pos4 = new Vec3(position.X + 1f, position.Y - 1f);
            var skeleton4 = SpawnAgent(data, pos4);
            var pos5 = new Vec3(position.X - 1f, position.Y - 1f);
            var skeleton5 = SpawnAgent(data, pos5);
        }

        private AgentBuildData GetAgentBuildData(Agent caster)
        {
            BasicCharacterObject troopCharacter = MBObjectManager.Instance.GetObject<BasicCharacterObject>(CreatureName);
            
            IAgentOriginBase troopOrigin = new SummonedAgentOrigin(caster, troopCharacter);
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
