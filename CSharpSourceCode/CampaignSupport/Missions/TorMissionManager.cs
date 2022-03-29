using SandBox;
using SandBox.Source.Missions;
using SandBox.Source.Missions.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;

namespace TOW_Core.CampaignSupport.Missions
{
	[MissionManager]
	public static class TorMissionManager
    {
		[MissionMethod]
		//public static Mission OpenGraveyardMission(FlattenedTroopRoster playerTroops, FlattenedTroopRoster enemyTroops)
		public static Mission OpenGraveyardMission()
		{
			var rec = SandBoxMissions.CreateSandBoxMissionInitializerRecord("TOR_graveyard_01_forceatmo");
			return MissionState.OpenNew("Battle", rec, delegate (Mission mission)
			{
				IMissionTroopSupplier[] suppliers = new IMissionTroopSupplier[]
				{
					new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, BattleSideEnum.Defender),
					new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, BattleSideEnum.Attacker)
				};
				List<MissionBehavior> list = new List<MissionBehavior>();
				list.Add(new MissionAgentSpawnLogic(suppliers, BattleSideEnum.Defender, false)); //OK
				list.Add(new BattleSpawnLogic("battle_set")); //OK
				list.Add(new GraveyardFightMissionController()); //OK
				list.Add(new CampaignMissionComponent()); //OK
				list.Add(new BattleAgentLogic()); //OK
				list.Add(new MountAgentLogic()); //OK
				list.Add(new MissionOptionsComponent()); //OK
				list.Add(new BattleEndLogic()); //OK
				list.Add(new MissionCombatantsLogic(MobileParty.MainParty.MapEvent.InvolvedParties, PartyBase.MainParty, MobileParty.MainParty.MapEvent.GetLeaderParty(BattleSideEnum.Defender), MobileParty.MainParty.MapEvent.GetLeaderParty(BattleSideEnum.Attacker), Mission.MissionTeamAITypeEnum.FieldBattle, false));
				list.Add(new BattleObserverMissionLogic()); //OK
				list.Add(new AgentHumanAILogic());
				list.Add(new AgentVictoryLogic());
				list.Add(new MissionAgentPanicHandler());
				list.Add(new BattleMissionAgentInteractionLogic());
				list.Add(new AgentMoraleInteractionLogic());
				list.Add(new AssignPlayerRoleInTeamMissionController(true, false, false, null, FormationClass.General));
				Hero leaderHero = MapEvent.PlayerMapEvent.DefenderSide.LeaderParty.LeaderHero;
				list.Add(new CreateBodyguardMissionBehavior(null, leaderHero != null ? leaderHero.Name : null));
				list.Add(new EquipmentControllerLeaveLogic());
				list.Add(new MissionHardBorderPlacer());
				list.Add(new MissionBoundaryPlacer());
				list.Add(new MissionBoundaryCrossingHandler());
				list.Add(new HighlightsController());
				list.Add(new BattleHighlightsController());
				list.Add(new MissionDebugHandler());
				list.Add(new DeploymentMissionController(false));
				list.Add(new BattleDeploymentHandler(false));
				return list.ToArray();
			}, true, true);
		}
	}

	public class GraveyardFightMissionController : MissionLogic
	{
		public override void OnBehaviorInitialize()
		{
			base.OnBehaviorInitialize();
			_missionAgentSpawnLogic = Mission.GetMissionBehavior<MissionAgentSpawnLogic>();
			_mapEvent = MapEvent.PlayerMapEvent;
		}

		public override void AfterStart()
		{
			int numDefender = MathF.Min(_mapEvent.GetNumberOfInvolvedMen(BattleSideEnum.Defender), 4);
			int numAttacker = _mapEvent.GetNumberOfInvolvedMen(BattleSideEnum.Attacker);
            int defenderInitialSpawn = numDefender;
			int attackerInitialSpawn = numAttacker;
            Mission.DoesMissionRequireCivilianEquipment = false;
			_missionAgentSpawnLogic.SetSpawnHorses(BattleSideEnum.Defender, false);
			_missionAgentSpawnLogic.SetSpawnHorses(BattleSideEnum.Attacker, false);
			_missionAgentSpawnLogic.InitWithSinglePhase(numDefender, numAttacker, defenderInitialSpawn, attackerInitialSpawn, true, true, 1f);
		}

		private MissionAgentSpawnLogic _missionAgentSpawnLogic;
		private MapEvent _mapEvent;
	}
}
