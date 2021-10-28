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
using TOW_Core.Battle.Voices;
using TOW_Core.CampaignSupport;
using TOW_Core.Battle.Map;
using TOW_Core.Battle.ShieldPatterns;
using TOW_Core.CampaignSupport.QuestBattleLocation;
using TOW_Core.Battle.ObjectDataExtensions.CustomBattleMoralModel;
using TOW_Core.Battle.Dismemberment;
using Path = System.IO.Path;
using TOW_Core.CampaignSupport.RaiseDead;
using TOW_Core.CampaignSupport.BattleHistory;
using TOW_Core.Battle.TriggeredEffect;
using TOW_Core.Items;
using TaleWorlds.MountAndBlade.GauntletUI;
using TOW_Core.Battle.CrosshairMissionBehavior;
using TOW_Core.Battle.Grenades;
using TOW_Core.CampaignSupport.ChaosRaidingParty;
using TOW_Core.CampaignSupport.TownBehaviours;
using TOW_Core.Battle.FireArms;
using TOW_Core.CampaignSupport.Models;
using TOW_Core.Battle;
using TOW_Core.CampaignSupport.Assimilation;
using System.IO;

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
                if (Campaign.Current.CampaignBehaviorManager.GetBehavior<BackstoryCampaignBehavior>() != null)
                {
                    Campaign.Current.CampaignBehaviorManager.RemoveBehavior<BackstoryCampaignBehavior>();
                }
                if (Campaign.Current.CampaignBehaviorManager.GetBehavior<UrbanCharactersCampaignBehavior>() != null)
                {
                    Campaign.Current.CampaignBehaviorManager.RemoveBehavior<UrbanCharactersCampaignBehavior>();
                    Campaign.Current.CampaignBehaviorManager.AddBehavior(new TORUrbanCharactersCampaignBehavior());
                }
                if (Campaign.Current.CampaignBehaviorManager.GetBehavior<HeroSpawnCampaignBehavior>() != null)
                {
                    Campaign.Current.CampaignBehaviorManager.RemoveBehavior<HeroSpawnCampaignBehavior>();
                    Campaign.Current.CampaignBehaviorManager.AddBehavior(new TORHeroSpawnCampaignBehavior());
                }
                if (Campaign.Current.CampaignBehaviorManager.GetBehavior<PartyHealCampaignBehavior>() != null)
                {
                    Campaign.Current.CampaignBehaviorManager.RemoveBehavior<PartyHealCampaignBehavior>();
                    Campaign.Current.CampaignBehaviorManager.AddBehavior(new TORPartyHealCampaignBehavior());
                }
            }
        }

        protected override void OnSubModuleLoad()
        {
            Harmony harmony = new Harmony("mod.harmony.theoldworld");
            harmony.PatchAll();
            ConfigureLogging();

            //This has to be here.
            AbilityManager.LoadAbilities();
            CustomVoiceManager.LoadVoices();
            AttributeManager.LoadAttributes();
            LoadStatusEffects();
            LoadSprites();
            LoadShieldPatterns();
            LoadQuestBattleTemplates();
            TriggeredEffectManager.LoadTemplates();
            AbilityFactory.LoadTemplates();
            MagicWeaponEffectManager.LoadXML();

            //ref https://forums.taleworlds.com/index.php?threads/ui-widget-modification.441516/ 
            UIConfig.DoNotUseGeneratedPrefabs = true;
            LoadFontAssets();
        }

        private void LoadQuestBattleTemplates()
        {
            QuestBattleTemplateManager.LoadQuestBattleTemplates();
        }

        private void LoadShieldPatterns()
        {
            ShieldPatternsManager.LoadShieldPatterns();
        }

        public void LoadFontAssets()
        {
            UIResourceManager.SpriteData.SpriteCategories["tow_fonts"].Load(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot);
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

        private void LoadSprites()
        {
            UIResourceManager.SpriteData.SpriteCategories["ui_abilityicons"].Load(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot);
            UIResourceManager.SpriteData.SpriteCategories["ui_hud"].Load(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot);
            UIResourceManager.SpriteData.SpriteCategories["tow_gamemenu_backgrounds"].Load(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot);
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if (game.GameType is CustomGame)
            {
                gameStarterObject.Models.RemoveAllOfType(typeof(CustomBattleMoraleModel));
                gameStarterObject.AddModel(new TOWBattleMoraleModel());
            }
            else if (game.GameType is Campaign)
            {
                CampaignGameStarter starter = gameStarterObject as CampaignGameStarter;

                starter.AddBehavior(new ExtendedInfoManager());
                starter.AddBehavior(new BattleInfoCampaignBehavior());
                starter.AddBehavior(new RaiseDeadCampaignBehavior());
                starter.AddBehavior(new QuestBattleLocationBehaviour());
                starter.AddBehavior(new ChaosRaidingPartyCampaignBehavior());
                starter.AddBehavior(new RaiseDeadInTownBehaviour());
                starter.AddBehavior(new LibraryTownBehaviour());
                starter.AddBehavior(new AssimilationCampaignBehavior());
                //starter.AddBehavior(new PrisonerFateCampaignBehavior());

                starter.AddModel(new QuestBattleLocationMenuModel());
                starter.AddModel(new TowCompanionHiringPriceCalculationModel());
                starter.AddModel(new CustomBattleMoralModel.TOWCampaignBattleMoraleModel());
                starter.AddModel(new TowKingdomPeaceModel());
                starter.AddModel(new TORBanditDensityModel());
                starter.AddModel(new TORMobilePartyFoodConsumptionModel());
                starter.AddModel(new TORPartySizeModel());
                //starter.AddModel(new TORCharacterStatsModel());
                starter.AddModel(new TORPartyWageModel());
                starter.AddModel(new TORPartySpeedCalculatingModel());

                CampaignOptions.IsLifeDeathCycleDisabled = true;
            }
        }

        public override void OnMissionBehaviourInitialize(Mission mission)
        {
            base.OnMissionBehaviourInitialize(mission);
            mission.AddMissionBehaviour(new AttributeSystemMissionLogic());
            mission.AddMissionBehaviour(new StatusEffectMissionLogic());
            mission.AddMissionBehaviour(new ExtendedInfoMissionLogic());
            mission.AddMissionBehaviour(new AbilityManagerMissionLogic());
            mission.AddMissionBehaviour(new AbilityHUDMissionView());
            mission.RemoveMissionBehaviour(mission.GetMissionBehaviour<MissionGauntletCrosshair>());
            mission.AddMissionBehaviour(new CustomCrosshairMissionBehavior());
            mission.AddMissionBehaviour(new MusketFireEffectMissionLogic());
            mission.AddMissionBehaviour(new CustomVoicesMissionBehavior());
            mission.AddMissionBehaviour(new DismembermentMissionLogic());
            mission.AddMissionBehaviour(new MagicWeaponEffectMissionLogic());
            //mission.AddMissionBehaviour(new GrenadesMissionLogic());
            mission.AddMissionBehaviour(new AtmosphereOverrideMissionLogic());
            if (Game.Current.GameType is Campaign)
            {
                mission.AddMissionBehaviour(new BattleInfoMissionLogic());
            }

            //this is a hack, for some reason that is beyond my comprehension, this crashes the game when loading into an arena with a memory violation exception.
            if (!mission.SceneName.Contains("arena")) mission.AddMissionBehaviour(new ShieldPatternsMissionLogic());
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