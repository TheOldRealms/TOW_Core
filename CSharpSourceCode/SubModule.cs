using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Texts;
using NLog;
using NLog.Targets;
using NLog.Config;
using TOW_Core.Battle.ObjectDataExtensions;
using TOW_Core.Battle.ObjectDataExtensions.CustomMissionLogic;
using TOW_Core.Utilities.Extensions;
using TOW_Core.Utilities;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.Engine.GauntletUI;
using TOW_Core.Abilities;
using TOW_Core.Battle.StatusEffects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TOW_Core.ObjectDataExtensions;
using TOW_Core.CampaignSupport;
using TOW_Core.Battle.Map;
using TOW_Core.Battle.ShieldPatterns;
using TOW_Core.CampaignSupport.QuestBattleLocation;
using TOW_Core.Battle.Dismemberment;
using Path = System.IO.Path;
using TOW_Core.CampaignSupport.RaiseDead;
using TOW_Core.CampaignSupport.BattleHistory;
using TOW_Core.Battle.TriggeredEffect;
using TOW_Core.Items;
using TaleWorlds.MountAndBlade.GauntletUI;
using TOW_Core.Battle.CrosshairMissionBehavior;
using TOW_Core.CampaignSupport.ChaosRaidingParty;
using TOW_Core.Battle.FireArms;
using TOW_Core.CampaignSupport.Models;
using TOW_Core.Battle;
using TOW_Core.Battle.Artillery;
using System.IO;
using System;
using TOW_Core.Battle.Damage;
using TOW_Core.CampaignSupport.TownBehaviours;
using SandBox;
using TOW_Core.Abilities.SpellBook;
using TOW_Core.Battle.AI.AgentBehavior.Components;
using TOW_Core.Battle.AI.AgentBehavior.SupportMissionLogic;
using TOW_Core.Battle.AttributeSystem.CustomBattleMoralModel;
using TOW_Core.Battle.Sound;
using TOW_Core.CampaignSupport.Assimilation;

namespace TOW_Core
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            TOWCommon.Say("TOW Core loaded.");
        }

        public override void OnGameInitializationFinished(Game game)
        {
            base.OnGameInitializationFinished(game);
            if (game.GameType is Campaign)
            {
                Campaign.Current.CampaignBehaviorManager.RemoveBehavior<BackstoryCampaignBehavior>();
                Campaign.Current.CampaignBehaviorManager.RemoveBehavior<PartyHealCampaignBehavior>();
                Campaign.Current.CampaignBehaviorManager.AddBehavior(new TORPartyHealCampaignBehavior());
            }
        }

        protected override void OnSubModuleLoad()
        {

            Harmony harmony = new Harmony("mod.harmony.theoldworld");
            harmony.PatchAll();
            ConfigureLogging();

            //This has to be here.
            ExtendedInfoManager.Load();
            LoadStatusEffects();
            LoadShieldPatterns();
            LoadQuestBattleTemplates();
            TriggeredEffectManager.LoadTemplates();
            AbilityFactory.LoadTemplates();
            ExtendedItemObjectManager.LoadXML();


            //ref https://forums.taleworlds.com/index.php?threads/ui-widget-modification.441516/ 
            UIConfig.DoNotUseGeneratedPrefabs = true;
        }

        private void LoadQuestBattleTemplates()
        {
            QuestBattleTemplateManager.LoadQuestBattleTemplates();
        }

        private void LoadShieldPatterns()
        {
            ShieldPatternsManager.LoadShieldPatterns();
        }

        /// <summary>
        /// This is the main game start.
        /// </summary>
        /// <param name="game"></param>
        public override void BeginGameStart(Game game)
        {
            TOWTextManager.LoadAdditionalTexts();
            TOWTextManager.LoadTextOverrides();

            if (game.GameType.GetType() == typeof(Campaign))
            {
                if (game.ObjectManager != null)
                {
                    game.ObjectManager.RegisterType<QuestBattleComponent>("QuestBattleComponent", "QuestBattleComponents", 1U, true);
                }
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            gameStarterObject.AddModel(new TORDamageParticleModel());
            if (game.GameType is CustomGame)
            {
                gameStarterObject.Models.RemoveAllOfType(typeof(CustomBattleMoraleModel));
                gameStarterObject.AddModel(new TORBattleMoraleModel());
                gameStarterObject.AddModel(new TORCustomBattleAgentStatCalculateModel());
            }
            else if (game.GameType is Campaign)
            {
                CampaignGameStarter starter = gameStarterObject as CampaignGameStarter;

                starter.AddBehavior(ExtendedInfoManager.Instance);
                starter.AddBehavior(new SpellBookMapIconCampaignBehaviour());
                starter.AddBehavior(new BattleInfoCampaignBehavior());
                starter.AddBehavior(new RaiseDeadCampaignBehavior());
                starter.AddBehavior(new QuestBattleLocationBehaviour());
                starter.AddBehavior(new ChaosRaidingPartyCampaignBehavior());
                starter.AddBehavior(new RaiseDeadInTownBehaviour());
                starter.AddBehavior(new SpellTrainerInTownBehaviour());
                starter.AddBehavior(new MasterEngineerTownBehaviour());
                starter.AddBehavior(new AssimilationCampaignBehavior());
                //starter.AddBehavior(new PrisonerFateCampaignBehavior());
                starter.AddBehavior(new TORWanderersCampaignBehavior());

                starter.AddModel(new QuestBattleLocationMenuModel());
                starter.AddModel(new TORCompanionHiringPriceCalculationModel());
                starter.AddModel(new TORCampaignBattleMoraleModel());
                //starter.AddModel(new TowKingdomPeaceModel());
                starter.AddModel(new TORBanditDensityModel());
                starter.AddModel(new TORMobilePartyFoodConsumptionModel());
                starter.AddModel(new TORPartySizeModel());
                starter.AddModel(new TORCharacterStatsModel());
                starter.AddModel(new TORPartyWageModel());
                starter.AddModel(new TORPartySpeedCalculatingModel());
                starter.AddModel(new TORPrisonerRecruitmentCalculationModel());
                starter.AddModel(new TORMarriageModel());
                starter.AddModel(new TORAgentStatCalculateModel());
                starter.AddModel(new TORCombatXpModel());

                CampaignOptions.IsLifeDeathCycleDisabled = true;
            }
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            mission.RemoveMissionBehavior(mission.GetMissionBehavior<MissionGauntletCrosshair>());

            mission.AddMissionBehavior(new AttributeSystemMissionLogic());
            mission.AddMissionBehavior(new StatusEffectMissionLogic());
            mission.AddMissionBehavior(new TestingMissionLogic());
            mission.AddMissionBehavior(new ExtendedInfoMissionLogic());
            mission.AddMissionBehavior(new AbilityManagerMissionLogic());
            mission.AddMissionBehavior(new AbilityHUDMissionView());
            mission.AddMissionBehavior(new CustomCrosshairMissionBehavior());
            mission.AddMissionBehavior(new BlackPowderWeaponMissionLogic());
            mission.AddMissionBehavior(new CustomVoicesMissionBehavior());
            mission.AddMissionBehavior(new DismembermentMissionLogic());
            mission.AddMissionBehavior(new WeaponEffectMissionLogic());
            mission.AddMissionBehavior(new AtmosphereOverrideMissionLogic());
            mission.AddMissionBehavior(new ArtilleryViewController());
            mission.AddMissionBehavior(new CustomAgentSoundMissionLogic());
            mission.AddMissionBehavior(new PowerfulSingleAgentTrackerMissionLogic());
            if (Game.Current.GameType is Campaign)
            {
                if (mission.GetMissionBehavior<BattleAgentLogic>() != null)
                {
                    mission.RemoveMissionBehavior(mission.GetMissionBehavior<BattleAgentLogic>());
                    mission.AddMissionBehavior(new TORBattleAgentLogic());
                }
                mission.AddMissionBehavior(new BattleInfoMissionLogic());
            }

            //this is a hack, for some reason that is beyond my comprehension, this crashes the game when loading into an arena with a memory violation exception.
            if (!mission.SceneName.Contains("arena")) mission.AddMissionBehavior(new ShieldPatternsMissionLogic());
        }

        private void LoadStatusEffects()
        {
            StatusEffectManager effectManager = new StatusEffectManager();
            effectManager.LoadStatusEffects();
        }

        private static void ConfigureLogging()
        {
            var path = Path.Combine(BasePath.Name, "Modules/TOW_Core/Logs/${LogHome}${date:format=yyyy}/${date:format=MMMM}/${date:format=dd}/TOW_log${shortdate}.txt");
            var config = new LoggingConfiguration();

            // Log debug/exception info to the log file
            var logfile = new FileTarget("logfile") { FileName = path };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Log info and higher to the VS debugger
            var logdebugger = new DebuggerTarget("logdebugger");
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logdebugger);

            LogManager.Configuration = config;
        }
    }
}