using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.MountAndBlade.View.Screen;
using TOW_Core.Abilities.Crosshairs;
using TOW_Core.Battle.AI.Components;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities
{
    public class AbilityManagerMissionLogic : MissionLogic
    {
        private bool _isSpecialMoveCanceled;
        private bool _shouldSheathWeapon;
        private bool _shouldWieldWeapon;
        private bool _isAbilityUser;
        private EquipmentIndex _mainHand;
        private EquipmentIndex _offHand;
        private AbilityComponent _abilityComponent;
        private GameKeyContext _keyContext = HotKeyManager.GetCategory("CombatHotKeyCategory");
        private Dictionary<Agent, PartyGroupTroopSupplier> _summonedCreatures = new Dictionary<Agent, PartyGroupTroopSupplier>();
        private MissionScreen _missionScreen = ((MissionView)Mission.Current.MissionBehaviors.FirstOrDefault(mb => mb is MissionView)).MissionScreen;

        protected override void OnEndMission()
        {
            BindWeaponKeys();
        }

        public override void OnMissionTick(float dt)
        {
            if (!_isAbilityUser)
            {
                if (Agent.Main != null)
                {
                    _abilityComponent = Agent.Main.GetComponent<AbilityComponent>();
                    if (_abilityComponent != null)
                    {
                        if (Agent.Main.HasAttribute("VampireBodyOverride") && Mission.Current.MissionTeamAIType != Mission.MissionTeamAITypeEnum.NoTeamAI)
                        {
                            _abilityComponent.SetSpecialMove((SpecialMove)AbilityFactory.CreateNew("ShadowStep", Agent.Main));
                        }
                        _abilityComponent.KnownAbilities.Add(AbilityFactory.CreateNew("HurlWeapons", Agent.Main));
                        _abilityComponent.KnownAbilities.Add(AbilityFactory.CreateNew("WordOfPain", Agent.Main));
                        _abilityComponent.InitializeCrosshairs();
                        _isAbilityUser = true;
                    }
                }
                return;
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
                        bool flag = _abilityComponent.CurrentAbility.Crosshair == null ||
                                    !_abilityComponent.CurrentAbility.Crosshair.IsVisible ||
                                    (_abilityComponent.CurrentAbility.Crosshair.CrosshairType == CrosshairType.Targeted &&
                                    ((TargetedCrosshair)_abilityComponent.CurrentAbility.Crosshair).Target == null);
                        if (flag)
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
                if (_shouldSheathWeapon)
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
                        _shouldSheathWeapon = false;
                    }
                }
                if (_shouldWieldWeapon)
                {
                    if (Agent.Main.GetWieldedItemIndex(Agent.HandIndex.MainHand) != _mainHand)
                    {
                        Agent.Main.TryToWieldWeaponInSlot(_mainHand, Agent.WeaponWieldActionType.WithAnimationUninterruptible, false);
                    }
                    else if (Agent.Main.GetWieldedItemIndex(Agent.HandIndex.OffHand) != _offHand)
                    {
                        Agent.Main.TryToWieldWeaponInSlot(_offHand, Agent.WeaponWieldActionType.WithAnimationUninterruptible, false);
                    }
                    else
                    {
                        _shouldWieldWeapon = false;
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
                                    _isSpecialMoveCanceled = true;
                                }
                                if (Input.IsKeyReleased(InputKey.LeftControl))
                                {
                                    if (_isSpecialMoveCanceled)
                                    {
                                        _isSpecialMoveCanceled = false;
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
                                if (!_isSpecialMoveCanceled)
                                {
                                    _abilityComponent.EnableSpecialMoveMode();
                                }
                            }
                            if (Input.IsKeyReleased(InputKey.LeftControl))
                            {
                                if (_isSpecialMoveCanceled)
                                {
                                    _isSpecialMoveCanceled = false;
                                }
                                _abilityComponent.DisableSpecialMoveMode();
                            }
                        }
                    }
                    else
                    {
                        _abilityComponent.DisableSpecialMoveMode();
                        _isSpecialMoveCanceled = false;
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
            _mainHand = Agent.Main.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            _offHand = Agent.Main.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            _abilityComponent?.EnableSpellMode();
            ChangeKeyBindings();
            _shouldSheathWeapon = true;
        }

        private void DisableSpellMode(bool isTakingNewWeapon)
        {
            if (isTakingNewWeapon)
            {
                _mainHand = EquipmentIndex.None;
                _offHand = EquipmentIndex.None;
            }
            else
            {
                _shouldWieldWeapon = true;
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
            _keyContext.GetGameKey(11).KeyboardKey.ChangeKey(InputKey.MouseScrollUp);
            _keyContext.GetGameKey(12).KeyboardKey.ChangeKey(InputKey.MouseScrollDown);
            _keyContext.GetGameKey(18).KeyboardKey.ChangeKey(InputKey.Numpad1);
            _keyContext.GetGameKey(19).KeyboardKey.ChangeKey(InputKey.Numpad2);
            _keyContext.GetGameKey(20).KeyboardKey.ChangeKey(InputKey.Numpad3);
            _keyContext.GetGameKey(21).KeyboardKey.ChangeKey(InputKey.Numpad4);
        }

        private void UnbindWeaponKeys()
        {
            _keyContext.GetGameKey(11).KeyboardKey.ChangeKey(InputKey.Invalid);
            _keyContext.GetGameKey(12).KeyboardKey.ChangeKey(InputKey.Invalid);
            _keyContext.GetGameKey(18).KeyboardKey.ChangeKey(InputKey.Invalid);
            _keyContext.GetGameKey(19).KeyboardKey.ChangeKey(InputKey.Invalid);
            _keyContext.GetGameKey(20).KeyboardKey.ChangeKey(InputKey.Invalid);
            _keyContext.GetGameKey(21).KeyboardKey.ChangeKey(InputKey.Invalid);
        }

        public override void OnItemPickup(Agent agent, SpawnedItemEntity item)
        {
            DisableSpellMode(true);
        }

        public void AddSummonedCreature(Agent creature, PartyGroupTroopSupplier supplier)
        {
            creature.OnAgentHealthChanged += (agent, oldHealth, newHealth) => CheckState(agent, newHealth);
            _summonedCreatures.Add(creature, supplier);
        }

        private void CheckState(Agent agent, float newHealth)
        {
            if (newHealth <= 0)
            {
                PartyGroupTroopSupplier supplier = _summonedCreatures.FirstOrDefault(sm => sm.Key == agent).Value;
                if (supplier != null)
                {
                    var num = Traverse.Create(supplier).Field("_numKilled").GetValue<int>();
                    Traverse.Create(supplier).Field("_numKilled").SetValue(num + 1);
                }
            }
        }
    }
}