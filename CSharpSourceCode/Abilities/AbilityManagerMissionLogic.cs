using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.MountAndBlade.View.Screen;
using TOW_Core.Abilities.Crosshairs;
using TOW_Core.Battle.AI.Components;
using TOW_Core.Battle.CrosshairMissionBehavior;
using TOW_Core.Battle.Crosshairs;
using TOW_Core.Items;
using TOW_Core.Quests;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities
{
    public class AbilityManagerMissionLogic : MissionLogic
    {
        private bool _shouldSheathWeapon;
        private bool _shouldWieldWeapon;
        private bool _hasInitializedForMainAgent;
        private AbilityModeState _currentState = AbilityModeState.Off;
        private EquipmentIndex _mainHand;
        private EquipmentIndex _offHand;
        private AbilityComponent _abilityComponent;
        private GameKeyContext _keyContext = HotKeyManager.GetCategory("CombatHotKeyCategory");
        private static readonly ActionIndexCache _idleAnimation = ActionIndexCache.Create("act_spellcasting_idle");
        private ParticleSystem[] _psys = null;
        private readonly string _castingStanceParticleName = "psys_spellcasting_stance";
        private SummonedCombatant _defenderSummoningCombatant;
        private SummonedCombatant _attackerSummoningCombatant;
        private readonly float DamagePortionForChargingSpecialMove = 0.25f;
        private Dictionary<Team, int> _artillerySlots = new Dictionary<Team, int>();

        public AbilityModeState CurrentState => _currentState;

        public int GetArtillerySlotsLeftForTeam(Team team)
        {
            int slotsLeft = 0;
            _artillerySlots.TryGetValue(team, out slotsLeft);
            return slotsLeft;
        }

        public override void OnFormationUnitsSpawned(Team team)
        {
            if(team.Side == BattleSideEnum.Attacker && _attackerSummoningCombatant == null)
            {
                var culture = team.Leader == null ? team.TeamAgents.FirstOrDefault().Character.Culture : team.Leader.Character.Culture;
                _attackerSummoningCombatant = new SummonedCombatant(team, culture);
            }
            else if (team.Side == BattleSideEnum.Defender && _defenderSummoningCombatant == null)
            {
                var culture = team.Leader == null ? team.TeamAgents.FirstOrDefault().Character.Culture : team.Leader.Character.Culture;
                _defenderSummoningCombatant = new SummonedCombatant(team, culture);
            }
            RefreshMaxArtilleryCountForTeam(team);
        }

        private void RefreshMaxArtilleryCountForTeam(Team team)
        {
            if (_artillerySlots.ContainsKey(team))
            {
                _artillerySlots[team] = 0;
                foreach(var agent in team.TeamAgents)
                {
                    if (agent.CanPlaceArtillery())
                    {
                        _artillerySlots[team] += agent.GetPlaceableArtilleryCount();
                    }
                }
            }
            else
            {
                _artillerySlots.Add(team, 0);
                RefreshMaxArtilleryCountForTeam(team);
            }
        }

        protected override void OnEndMission()
        {
            BindWeaponKeys();
        }

        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, int damage, in MissionWeapon affectorWeapon)
        {
            var comp = affectorAgent.GetComponent<AbilityComponent>();
            if(comp != null)
            {
                if(comp.SpecialMove != null) comp.SpecialMove.AddCharge(damage * DamagePortionForChargingSpecialMove);
            }
        }

        public override void OnMissionTick(float dt)
        {
            if (!_hasInitializedForMainAgent)
            {
                if(Agent.Main != null)
                {
                    SetUpCastStanceParticles();
                    _hasInitializedForMainAgent = true;
                }
            }
            else if (IsAbilityModeAvailableForMainAgent())
            {
                HandleInput();

                UpdateWieldedItems();

                HandleAnimations();
            }
        }

        private void HandleAnimations()
        {
            if(CurrentState != AbilityModeState.Off)
            {
                var action = Agent.Main.GetCurrentAction(1);
                if(CurrentState == AbilityModeState.Idle && action != _idleAnimation)
                {
                    Agent.Main.SetActionChannel(1, _idleAnimation);
                }
            }
        }

        internal void OnCastComplete(Ability ability, Agent agent)
        {
            if(ability is ItemBoundAbility && ability.Template.AbilityEffectType == AbilityEffectType.ArtilleryPlacement)
            {
                if (_artillerySlots.ContainsKey(agent.Team))
                {
                    _artillerySlots[agent.Team]--;
                }
            }
            if(agent == Agent.Main)
            {
                if (CurrentState == AbilityModeState.Casting) _currentState = AbilityModeState.Idle;
                if (Game.Current.GameType is Campaign)
                {
                    var quest = AdvanceSpellCastingLevelQuest.GetCurrentActiveIfExists();
                    if (quest != null)
                    {
                        quest.IncrementCast();
                    }
                }
            }
        }

        internal void OnCastStart(Ability ability, Agent agent) 
        {
            if(agent == Agent.Main)
            {
                if (CurrentState == AbilityModeState.Idle) _currentState = AbilityModeState.Casting;
            }
        }

        private void UpdateWieldedItems()
        {
            if (_currentState == AbilityModeState.Idle && _shouldSheathWeapon)
            {
                if (Agent.Main.GetWieldedItemIndex(Agent.HandIndex.MainHand) != EquipmentIndex.None)
                {
                    Agent.Main.TryToSheathWeaponInHand(Agent.HandIndex.MainHand, Agent.WeaponWieldActionType.WithAnimation);
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
            if (_currentState == AbilityModeState.Off && _shouldWieldWeapon)
            {
                if (Agent.Main.GetWieldedItemIndex(Agent.HandIndex.MainHand) != _mainHand)
                {
                    Agent.Main.TryToWieldWeaponInSlot(_mainHand, Agent.WeaponWieldActionType.WithAnimation, false);
                }
                else if (Agent.Main.GetWieldedItemIndex(Agent.HandIndex.OffHand) != _offHand)
                {
                    Agent.Main.TryToWieldWeaponInSlot(_offHand, Agent.WeaponWieldActionType.WithAnimation, false);
                }
                else
                {
                    _shouldWieldWeapon = false;
                }
            }
        }

        private void HandleInput()
        {
            //Turning ability mode on/off
            if (Input.IsKeyPressed(InputKey.Q))
            {
                switch (_currentState)
                {
                    case AbilityModeState.Off:
                        EnableAbilityMode();
                        break;
                    case AbilityModeState.Idle:
                        DisableAbilityMode(false);
                        break;
                    default:
                        break;
                }
            }
            else if (Input.IsKeyPressed(InputKey.LeftMouseButton))
            {
                bool flag = _abilityComponent.CurrentAbility.Crosshair == null ||
                            !_abilityComponent.CurrentAbility.Crosshair.IsVisible ||
                            _currentState != AbilityModeState.Idle ||
                            (_abilityComponent.CurrentAbility.Crosshair.CrosshairType == CrosshairType.SingleTarget &&
                            !((SingleTargetCrosshair)_abilityComponent.CurrentAbility.Crosshair).IsTargetLocked);
                if (!flag)
                {
                    Agent.Main.CastCurrentAbility();
                }
                if(_abilityComponent.SpecialMove != null && _abilityComponent.SpecialMove.IsUsing) _abilityComponent.StopSpecialMove();
            }
            else if (Input.IsKeyPressed(InputKey.RightMouseButton))
            {
                if (_abilityComponent.SpecialMove != null && _abilityComponent.SpecialMove.IsUsing) _abilityComponent.StopSpecialMove();
            }
            else if (Input.IsKeyPressed(InputKey.MouseScrollUp) && _currentState != AbilityModeState.Off)
            {
                Agent.Main.SelectNextAbility();
            }
            else if (Input.IsKeyPressed(InputKey.MouseScrollDown) && _currentState != AbilityModeState.Off)
            {
                Agent.Main.SelectPreviousAbility();
            }
            else if (Input.IsKeyPressed(InputKey.LeftControl) && _abilityComponent != null && _abilityComponent.SpecialMove != null)
            {
                if (_currentState == AbilityModeState.Off && _abilityComponent.SpecialMove.IsCharged && IsCurrentCrossHairCompatible())
                {
                    _abilityComponent.SpecialMove.TryCast(Agent.Main);
                }
            }
        }

        private bool IsCurrentCrossHairCompatible()
        {
            var behaviour = Mission.Current.GetMissionBehavior<CustomCrosshairMissionBehavior>();
            if (behaviour == null) return true;
            else
            {
                if (behaviour.CurrentCrosshair is SniperScope) return !behaviour.CurrentCrosshair.IsVisible;
                else return true;
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

        private bool IsAbilityModeAvailableForMainAgent()
        {
            return Agent.Main != null &&
                   Agent.Main.IsActive() &&
                   !ScreenManager.GetMouseVisibility() &&
                   IsCastingMission(Mission) &&
                   _abilityComponent != null;
                   
        }

        private void EnableAbilityMode()
        {
            _mainHand = Agent.Main.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            _offHand = Agent.Main.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            _shouldSheathWeapon = true;
            _currentState = AbilityModeState.Idle;
            ChangeKeyBindings();
            var traitcomp = Agent.Main.GetComponent<ItemTraitAgentComponent>();
            if (traitcomp != null)
            {
                traitcomp.EnableAllParticles(false);
            }
            EnableCastStanceParticles(true);
        }

        private void DisableAbilityMode(bool isTakingNewWeapon)
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
            _currentState = AbilityModeState.Off;
            ChangeKeyBindings();
            var traitcomp = Agent.Main.GetComponent<ItemTraitAgentComponent>();
            if (traitcomp != null)
            {
                traitcomp.EnableAllParticles(true);
            }
            EnableCastStanceParticles(false);
        }
        private void EnableCastStanceParticles(bool enable)
        {
            if(_psys != null)
            {
                foreach (var psys in _psys)
                {
                    if(psys != null)
                    {
                        psys.SetEnable(enable);
                    }
                }
            }
        }

        private void ChangeKeyBindings()
        {
            if (_abilityComponent != null && _currentState != AbilityModeState.Off)
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
            if(agent == Agent.Main) DisableAbilityMode(true);
        }

        public SummonedCombatant GetSummoningCombatant(Team team)
        {
            if (team.Side == BattleSideEnum.Attacker) return _attackerSummoningCombatant;
            else if (team.Side == BattleSideEnum.Defender) return _defenderSummoningCombatant;
            else return null;
        }

        protected override void OnAgentControllerChanged(Agent agent, Agent.ControllerType oldController)
        {
            if (agent.Controller == Agent.ControllerType.Player)
            {
                _hasInitializedForMainAgent = false;
            }
        }

        private void SetUpCastStanceParticles()
        {
            _abilityComponent = Agent.Main.GetComponent<AbilityComponent>();
            if (_abilityComponent != null)
            {
                _psys = new ParticleSystem[2];
                GameEntity entity;
                _psys[0] = TOWParticleSystem.ApplyParticleToAgentBone(Agent.Main, _castingStanceParticleName, Game.Current.HumanMonster.MainHandItemBoneIndex, out entity);
                _psys[1] = TOWParticleSystem.ApplyParticleToAgentBone(Agent.Main, _castingStanceParticleName, Game.Current.HumanMonster.OffHandItemBoneIndex, out entity);
                EnableCastStanceParticles(false);
            }
        }
    }

    public enum AbilityModeState
    {
        Off,
        Idle,
        Casting
    }
}