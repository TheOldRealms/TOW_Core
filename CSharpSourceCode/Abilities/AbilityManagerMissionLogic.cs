using TaleWorlds.Core;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screen;
using TOW_Core.Battle.AI.Components;
using TOW_Core.Battle.CrosshairMissionBehavior;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities
{
    public class AbilityManagerMissionLogic : MissionLogic
    {
        private bool isMainAgentChecked;
        private EquipmentIndex mainHand;
        private EquipmentIndex offHand;
        private Ability currentAbility;
        private AbilityComponent _abilityComponent;
        private GameKeyContext keyContext = HotKeyManager.GetCategory("CombatHotKeyCategory");
        System.Timers.Timer sheathTimer = new System.Timers.Timer(1000);
        private MissionScreen _missionScreen;

        public AbilityManagerMissionLogic()
        {
        }

        public override void OnBehaviourInitialize()
        {
            base.OnBehaviourInitialize();
            sheathTimer.AutoReset = false;
            sheathTimer.Elapsed += (s, e) =>
                                       Agent.Main.TryToSheathWeaponInHand(Agent.HandIndex.OffHand,
                                           Agent.WeaponWieldActionType.WithAnimationUninterruptible);
        }

        protected override void OnEndMission()
        {
            base.OnEndMission();
            BindWeaponKeys();
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (CanUseAbilities())
            {
                if (_abilityComponent.IsAbilityModeOn)
                {
                    if (Input.IsKeyPressed(InputKey.Q))
                    {
                        if (currentAbility.Crosshair != null)
                            currentAbility.Crosshair.Hide();
                        DisableSpellMode();
                    }
                    else if (Input.IsKeyPressed(InputKey.LeftMouseButton))
                    {
                        if (currentAbility.Crosshair.IsVisible)
                            Agent.Main.CastCurrentAbility();
                    }
                    else if (Input.IsKeyPressed(InputKey.MouseScrollUp))
                    {
                        if (currentAbility.Crosshair != null)
                            currentAbility.Crosshair.Hide();
                        Agent.Main.SelectNextAbility();
                        currentAbility = Agent.Main.GetCurrentAbility();
                    }
                    else if (Input.IsKeyPressed(InputKey.MouseScrollDown))
                    {
                        if (currentAbility.Crosshair != null)
                            currentAbility.Crosshair.Hide();
                        Agent.Main.SelectPreviousAbility();
                        currentAbility = Agent.Main.GetCurrentAbility();
                    }
                }
                else
                {
                    if (Input.IsKeyPressed(InputKey.Q))
                    {
                        EnableSpellMode();
                    }
                }
            }
            else if (!isMainAgentChecked && Agent.Main != null)
            {
                _abilityComponent = Agent.Main.GetComponent<AbilityComponent>();
                if (_abilityComponent != null)
                {
                    currentAbility = _abilityComponent.CurrentAbility;
                    var crosshairMB = Mission.Current.GetMissionBehaviour<CustomCrosshairMissionBehavior>();
                    if (crosshairMB != null)
                    {
                        _missionScreen = crosshairMB.MissionScreen;
                        foreach (var ability in _abilityComponent.KnownAbilities)
                        {
                            if (ability.Crosshair != null)
                            {
                                ability.Crosshair.SetMissionScreen(_missionScreen);
                                ability.Crosshair.Initialize();
                            }
                        }
                        isMainAgentChecked = true;
                    }
                }
            }
        }

        public override void OnAgentCreated(Agent agent)
        {
            base.OnAgentCreated(agent);
            if (!Mission.IsFriendlyMission && Mission.CombatType != Mission.MissionCombatType.ArenaCombat && Mission.CombatType != Mission.MissionCombatType.NoCombat)
            {
                if (agent.IsAbilityUser())
                {
                    agent.AddComponent(new AbilityComponent(agent));
                    if (agent.IsAIControlled)
                    {
                        agent.AddComponent(new WizardAIComponent(agent));
                    }
                }
            }
        }

        private bool CanUseAbilities()
        {
            return Agent.Main != null &&
                   Agent.Main.State == AgentState.Active &&
                   _abilityComponent != null &&
                   _missionScreen != null &&
                   !ScreenManager.GetMouseVisibility();
        }

        private void EnableSpellMode()
        {
            _abilityComponent.EnableAbilityMode();
            ChangeKeyBindings();
            mainHand = Agent.Main.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            offHand = Agent.Main.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            Agent.Main.TryToSheathWeaponInHand(Agent.HandIndex.MainHand, Agent.WeaponWieldActionType.WithAnimationUninterruptible);
            sheathTimer.Start();
        }

        private void DisableSpellMode()
        {
            _abilityComponent.DisableAbilityMode();
            ChangeKeyBindings();
            Agent.Main.TryToWieldWeaponInSlot(mainHand, Agent.WeaponWieldActionType.WithAnimationUninterruptible, false);
            Agent.Main.TryToWieldWeaponInSlot(offHand, Agent.WeaponWieldActionType.WithAnimationUninterruptible, false);
        }

        private void ChangeKeyBindings()
        {
            if (_abilityComponent.IsAbilityModeOn)
            {
                UnbindWeaponKeys();
            }
            else
            {
                BindWeaponKeys();
            }
        }

        private void BindWeaponKeys()
        {
            keyContext.GetGameKey(11).KeyboardKey.ChangeKey(InputKey.MouseScrollUp);
            keyContext.GetGameKey(12).KeyboardKey.ChangeKey(InputKey.MouseScrollDown);
            keyContext.GetGameKey(18).KeyboardKey.ChangeKey(InputKey.Numpad1);
            keyContext.GetGameKey(19).KeyboardKey.ChangeKey(InputKey.Numpad2);
            keyContext.GetGameKey(20).KeyboardKey.ChangeKey(InputKey.Numpad3);
            keyContext.GetGameKey(21).KeyboardKey.ChangeKey(InputKey.Numpad4);
        }

        private void UnbindWeaponKeys()
        {
            keyContext.GetGameKey(11).KeyboardKey.ChangeKey(InputKey.Invalid);
            keyContext.GetGameKey(12).KeyboardKey.ChangeKey(InputKey.Invalid);
            keyContext.GetGameKey(18).KeyboardKey.ChangeKey(InputKey.Invalid);
            keyContext.GetGameKey(19).KeyboardKey.ChangeKey(InputKey.Invalid);
            keyContext.GetGameKey(20).KeyboardKey.ChangeKey(InputKey.Invalid);
            keyContext.GetGameKey(21).KeyboardKey.ChangeKey(InputKey.Invalid);
        }
    }
}