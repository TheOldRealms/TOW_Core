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
        private bool shouldSheathWeapon;
        private bool shouldWieldWeapon;
        private bool isMainAgentChecked;
        private EquipmentIndex mainHand;
        private EquipmentIndex offHand;
        private Ability currentAbility;
        private AbilityComponent _abilityComponent;
        private GameKeyContext keyContext = HotKeyManager.GetCategory("CombatHotKeyCategory");
        private MissionScreen _missionScreen;

        public AbilityManagerMissionLogic()
        {
        }

        protected override void OnEndMission()
        {
            BindWeaponKeys();
        }

        public override void OnMissionTick(float dt)
        {
            if (!isMainAgentChecked)
            {
                if (Agent.Main != null)
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
            if (CanUseAbilities())
            {
                if (_abilityComponent != null && _abilityComponent.IsAbilityModeOn)
                {
                    if (Input.IsKeyPressed(InputKey.Q))
                    {
                        if (currentAbility.Crosshair != null)
                            currentAbility.Crosshair.Hide();
                        DisableSpellMode(false);
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
                    if (shouldSheathWeapon)
                    {
                        if (Agent.Main.GetWieldedItemIndex(Agent.HandIndex.MainHand) != EquipmentIndex.None)
                        {
                            Agent.Main.TryToSheathWeaponInHand(Agent.HandIndex.MainHand, Agent.WeaponWieldActionType.WithAnimationUninterruptible);
                        }
                        else if (Agent.Main.GetWieldedItemIndex(Agent.HandIndex.OffHand) != EquipmentIndex.None)
                        {
                            Agent.Main.TryToSheathWeaponInHand(Agent.HandIndex.OffHand, Agent.WeaponWieldActionType.WithAnimation);
                        }
                        else
                        {
                            shouldSheathWeapon = false;
                        }
                    }
                }
                else
                {
                    if (Input.IsKeyPressed(InputKey.Q))
                    {
                        EnableSpellMode();
                    }
                    if (shouldWieldWeapon)
                    {
                        if (Agent.Main.GetWieldedItemIndex(Agent.HandIndex.MainHand) != mainHand)
                        {
                            Agent.Main.TryToWieldWeaponInSlot(mainHand, Agent.WeaponWieldActionType.WithAnimationUninterruptible, false);
                        }
                        else if (Agent.Main.GetWieldedItemIndex(Agent.HandIndex.OffHand) != offHand)
                        {
                            Agent.Main.TryToWieldWeaponInSlot(offHand, Agent.WeaponWieldActionType.WithAnimationUninterruptible, false);
                        }
                        else
                        {
                            shouldWieldWeapon = false;
                        }
                    }
                }
            }
        }

        public override void OnAgentCreated(Agent agent)
        {
            if (IsCastingMission(Mission))
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

        public static bool IsCastingMission(Mission mission)
        {
            return !mission.IsFriendlyMission && mission.CombatType != Mission.MissionCombatType.ArenaCombat && mission.CombatType != Mission.MissionCombatType.NoCombat;
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
            mainHand = Agent.Main.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            offHand = Agent.Main.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            _abilityComponent?.EnableAbilityMode();
            ChangeKeyBindings();
            shouldSheathWeapon = true;
        }

        private void DisableSpellMode(bool isTakingNewWeapon)
        {
            if (isTakingNewWeapon)
            {
                mainHand = EquipmentIndex.None;
                offHand = EquipmentIndex.None;
            }
            else
            {
                shouldWieldWeapon = true;
            }
            _abilityComponent?.DisableAbilityMode();
            ChangeKeyBindings();

        }

        private void ChangeKeyBindings()
        {
            if (_abilityComponent != null && _abilityComponent.IsAbilityModeOn)
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

        public override void OnItemPickup(Agent agent, SpawnedItemEntity item)
        {
            DisableSpellMode(true);
        }
    }
}