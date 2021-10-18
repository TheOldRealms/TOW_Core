using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.MountAndBlade.View.Screen;
using TOW_Core.Battle.AI.Components;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities
{
    public class AbilityManagerMissionLogic : MissionLogic
    {
        private bool isSpecialMoveCanceled;
        private bool shouldSheathWeapon;
        private bool shouldWieldWeapon;
        private bool isMainAgentChecked;
        private EquipmentIndex mainHand;
        private EquipmentIndex offHand;
        private AbilityComponent _abilityComponent;
        private GameKeyContext keyContext = HotKeyManager.GetCategory("CombatHotKeyCategory");
        private MissionScreen _missionScreen = ((MissionView)Mission.Current.MissionBehaviours.FirstOrDefault(mb => mb is MissionView)).MissionScreen;

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
                    isMainAgentChecked = true;
                    _abilityComponent = Agent.Main.GetComponent<AbilityComponent>();
                }
            }
            if (CanUseAbilities())
            {
                if (_abilityComponent.IsSpellModeOn)
                {
                    if (Input.IsKeyPressed(InputKey.Q))
                    {
                        DisableSpellMode(false);
                    }
                    else if (Input.IsKeyPressed(InputKey.LeftMouseButton))
                    {
                        if (_abilityComponent.CurrentAbility.Crosshair == null || !_abilityComponent.CurrentAbility.Crosshair.IsVisible)
                        {
                            return;
                        }
                        Agent.Main.CastCurrentAbility();
                    }
                    else if (Input.IsKeyPressed(InputKey.MouseScrollUp))
                    {
                        Agent.Main.SelectNextAbility();
                    }
                    else if (Input.IsKeyPressed(InputKey.MouseScrollDown))
                    {
                        Agent.Main.SelectPreviousAbility();
                    }
                }
                else
                {
                    if (Input.IsKeyPressed(InputKey.Q))
                    {
                        EnableSpellMode();
                    }
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
                if (_abilityComponent.SpecialMove != null)
                {
                    if (!Agent.Main.HasMount)
                    {
                        if (_abilityComponent.SpecialMove.IsUsing)
                        {
                            if (Input.IsKeyPressed(InputKey.LeftMouseButton) || Input.IsKeyPressed(InputKey.RightMouseButton))
                            {
                                _abilityComponent.StopSpecialMove();
                            }
                        }
                        else
                        {
                            if (_abilityComponent.IsSpecialMoveAtReady)
                            {
                                if (Input.IsKeyPressed(InputKey.LeftMouseButton) || Input.IsKeyPressed(InputKey.RightMouseButton))
                                {
                                    _abilityComponent.DisableSpecialMoveMode();
                                    isSpecialMoveCanceled = true;
                                }
                                if (Input.IsKeyReleased(InputKey.LeftControl))
                                {
                                    if (isSpecialMoveCanceled)
                                    {
                                        isSpecialMoveCanceled = false;
                                    }
                                    else
                                    {
                                        _abilityComponent.DisableSpecialMoveMode();
                                        _abilityComponent.SpecialMove.TryCast(Agent.Main);
                                    }
                                }
                            }
                            else if (Input.IsKeyDown(InputKey.LeftControl))
                            {
                                if (!isSpecialMoveCanceled)
                                {
                                    _abilityComponent.EnableSpecialMoveMode();
                                }
                            }
                            if (Input.IsKeyReleased(InputKey.LeftControl))
                            {
                                if (isSpecialMoveCanceled)
                                {
                                    isSpecialMoveCanceled = false;
                                }
                                _abilityComponent.DisableSpecialMoveMode();
                            }
                        }
                    }
                    else
                    {
                        _abilityComponent.DisableSpecialMoveMode();
                        isSpecialMoveCanceled = false;
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
                   Agent.Main.IsActive() &&
                   _abilityComponent != null &&
                   _missionScreen != null &&
                   !ScreenManager.GetMouseVisibility();
        }

        private void EnableSpellMode()
        {
            mainHand = Agent.Main.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            offHand = Agent.Main.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            _abilityComponent?.EnableSpellMode();
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
            _abilityComponent?.DisableSpellMode();
            ChangeKeyBindings();

        }

        private void ChangeKeyBindings()
        {
            if (_abilityComponent != null && _abilityComponent.IsSpellModeOn)
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