using HarmonyLib;
using System.IO;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Texts;
using TOW_Core.CustomBattles;
using NLog;
using NLog.Targets;
using NLog.Config;
using TOW_Core.Battle.AttributeSystem;
using TOW_Core.Battle.AttributeSystem.CustomMissionLogic;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;
using TOW_Core.Utilities.Extensions;
using TOW_Core.Utilities;
using TOW_Core.Battle.AttributeSystem.CustomBattleMoralModel;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.GauntletUI;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.TwoDimension;
using TOW_Core.Abilities;
using TOW_Core.Battle.StatusEffects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using System;
using TOW_Core.Battle.Voices;
using TOW_Core.CampaignSupport;
using TOW_Core.Battle.ShieldPatterns;
using TOW_Core.Battle.Dismemberment;
using TaleWorlds.InputSystem;
using System.Reflection;

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
            var path = Path.Combine(BasePath.Name, "Modules/TOW_Core/ModuleData/managed_modded_parameters.xml");
            if (File.Exists(path))
            {
                TaleWorlds.Core.ManagedParameters.Instance.Initialize(path);
            }
        }

        protected override void OnSubModuleLoad()
        {
            Harmony harmony = new Harmony("mod.harmony.theoldworld");
            harmony.PatchAll();
            ConfigureLogging();

            //This has to be here.
            AbilityManager.LoadAbilities();
            LoadAttributes();
            LoadStatusEffects();
            LoadSprites();
            LoadVoices();
            LoadShieldPatterns();

            //ref https://forums.taleworlds.com/index.php?threads/ui-widget-modification.441516/ 
            UIConfig.DoNotUseGeneratedPrefabs = true;
            LoadFontAssets();
            LoadDismembermentSettings();
        }
        protected override void OnApplicationTick(float dt)
        {
            if (Game.Current != null)
                if (Game.Current.CurrentState <= 0)
                {
                    if (Mission.Current == null || Mission.Current.Scene == null)
                    {
                        if (Dismemberment.IgnoredAgents.Count > 0)
                            Dismemberment.IgnoredAgents.Clear();
                    }
                    else
                    {
                        Agent mainAgent = Agent.Main;
                        if (mainAgent != null)
                        {
                            foreach (PotentialVictim potentialDismembermentVictim in Dismemberment.PotentialVictims.ToArray())
                            {
                                bool isInactiveAndKilled = !potentialDismembermentVictim.agent.IsActive() && potentialDismembermentVictim.agent.State == AgentState.Killed;
                                if (isInactiveAndKilled)
                                {
                                    if (potentialDismembermentVictim.attacker == mainAgent)
                                    {
                                        if (Dismemberment.IsSlowMotionOn)
                                        {
                                            Dismemberment.SlowMotionTimer = MBCommon.TimeType.Mission.GetTime() + 0.5f;
                                            Mission.Current.Scene.SlowMotionMode = true;
                                        }
                                    }
                                    potentialDismembermentVictim.attacker.SetWantsToYell();
                                    Dismemberment.DismemberHead(potentialDismembermentVictim.agent);
                                    Dismemberment.PotentialVictims.Remove(potentialDismembermentVictim);
                                }
                                if (MBCommon.TimeType.Mission.GetTime() >= potentialDismembermentVictim.timer)
                                {
                                    if (potentialDismembermentVictim.agent.State != AgentState.Killed)
                                    {
                                        Dismemberment.PotentialVictims.Remove(potentialDismembermentVictim);
                                        return;
                                    }
                                    if (potentialDismembermentVictim.agent.State == AgentState.Unconscious)
                                    {
                                        Dismemberment.PotentialVictims.Remove(potentialDismembermentVictim);
                                        return;
                                    }
                                }
                            }
                            if (Dismemberment.IsSlowMotionOn && Dismemberment.SlowMotionTimer > 0f)
                                if (MBCommon.TimeType.Mission.GetTime() >= Dismemberment.SlowMotionTimer)
                                {
                                    Dismemberment.SlowMotionTimer = 0f;
                                    Mission.Current.Scene.SlowMotionMode = false;
                                }
                            if (Dismemberment.IsDebugModeOn && Input.IsKeyPressed(InputKey.O))
                                Dismemberment.SpawnAgent(mainAgent);
                        }
                    }
                }
        }
        private void LoadDismembermentSettings()
        {
            string fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\..\\"));
            string[] settingsStrings = File.ReadAllText(fullPath + "/Settings.cfg").Split('\n');

            foreach (String setting in settingsStrings)
            {
                if (setting.Contains("Chance"))
                    Dismemberment.RandChance = int.Parse(setting.Split('=')[1]);
                else if (setting.Contains("SlowMotion"))
                    Dismemberment.IsSlowMotionOn = bool.Parse(setting.Split('=')[1]);
                else if (setting.Contains("Debug"))
                    Dismemberment.IsDebugModeOn = bool.Parse(setting.Split('=')[1]);
            }
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
            if (game.GameType.GetType() == typeof(CustomGame))
            {
                CustomBattleTroopManager.LoadCustomBattleTroops();
            }
        }

        private void LoadSprites()
        {
            UIResourceManager.SpriteData.SpriteCategories["tow_spritesheet"].Load(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot);
            UIResourceManager.SpriteData.SpriteCategories["tow_gamemenu_backgrounds"].Load(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot);
		}

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if(game.GameType is CustomGame)
            {
                gameStarterObject.Models.RemoveAllOfType(typeof(CustomBattleMoraleModel));
                gameStarterObject.AddModel(new TOWBattleMoraleModel());
            }
            else if(game.GameType is Campaign)
            {
                CampaignGameStarter starter = gameStarterObject as CampaignGameStarter;
                starter.CampaignBehaviors.RemoveAllOfType(typeof(BackstoryCampaignBehavior));
                starter.Models.RemoveAllOfType(typeof(CompanionHiringPriceCalculationModel));
                starter.AddModel(new TowCompanionHiringPriceCalculationModel());
            }
        }

        public override void OnMissionBehaviourInitialize(Mission mission)
        {
            base.OnMissionBehaviourInitialize(mission);
            mission.AddMissionBehaviour(new AttributeSystemMissionLogic());
            mission.AddMissionBehaviour(new StatusEffectMissionLogic());
            mission.AddMissionBehaviour(new Abilities.AbilityManagerMissionLogic());
            mission.AddMissionBehaviour(new Abilities.AbilityHUDMissionView());
            mission.AddMissionBehaviour(new Battle.FireArms.MusketFireEffectMissionLogic());
            mission.AddMissionBehaviour(new CustomVoicesMissionBehavior());
            mission.AddMissionBehaviour(new ShieldPatternsMissionLogic());
        }

        private void LoadAttributes()
        {
            AttributeManager attributeManager = new AttributeManager();
            attributeManager.LoadAttributes();
        }

        private void LoadVoices()
        {
            CustomVoiceManager voiceManager = new CustomVoiceManager();
            voiceManager.LoadVoices();
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