using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle;
using TOW_Core.Abilities.Crosshairs;
using TOW_Core.Abilities.Scripts;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities
{
    public class AbilityComponent : AgentComponent
    {
        public AbilityComponent(Agent agent) : base(agent)
        {
            var abilities = agent.GetAbilities();
            if (abilities.Count > 0)
            {
                foreach (var item in abilities)
                {
                    try
                    {
                        var ability = AbilityFactory.CreateNew(item, agent);
                        if (ability != null)
                        {
                            ability.OnCastStart += OnCastStart;
                            ability.OnCastComplete += OnCastComplete;
                            if (ability is SpecialMove)
                            {
                                _specialMove = (SpecialMove)ability;
                            }
                            else
                            {
                                _knownAbilities.Add(ability);
                                //in custom battle the is no inventory checking for artillery, but checking for known ability
                                if (Game.Current.GameType is CustomGame && ability is ArtilleryDeployer)
                                {
                                    ((ArtilleryDeployer)ability).ArtilleryDeployed += () => _maxArtilleryAmount--;
                                    ((ArtilleryDeployer)ability).SetAmount(5);
                                    ((ArtilleryDeployer)ability).SetAbilityComponent(this);
                                    _maxArtilleryAmount = 10;
                                }
                            }
                        }
                        else
                        {
                            TOWCommon.Log("Attempted to add an ability to agent: " + agent.Character.StringId + ", but it wasn't of type BaseAbility", LogLevel.Warn);
                        }
                    }
                    catch (Exception)
                    {
                        TOWCommon.Log("Failed instantiating ability class: " + item, LogLevel.Error);
                    }
                }
                if (Agent.IsVampire() && _specialMove == null) _specialMove = (SpecialMove)AbilityFactory.CreateNew("ShadowStep", Agent);
            }
            if (agent.CanPlaceArtillery())
            {
                var artilleryRoster = agent.GetHero().PartyBelongedTo.ItemRoster.Where(item => item.EquipmentElement.Item.StringId.Contains("artillery")).ToArray();
                if (artilleryRoster.Length > 0)
                {
                    for (int i = 0; i < artilleryRoster.Length; i++)
                    {
                        var artillery = artilleryRoster[i];
                        var ability = (ArtilleryDeployer)AbilityFactory.CreateNew(artillery.EquipmentElement.Item.PrefabName, agent);
                        if (ability != null)
                        {
                            ability.OnCastStart += OnCastStart;
                            ability.OnCastComplete += OnCastComplete;
                            ability.ArtilleryDeployed += () => _maxArtilleryAmount--;
                            ability.SetAmount(artillery.Amount);
                            ability.SetAbilityComponent(this);
                            _knownAbilities.Add(ability);
                        }
                    }
                    _maxArtilleryAmount = agent.Character.GetSkillValue(DefaultSkills.Engineering) / 50;
                }
            }
            if (_knownAbilities.Count > 0)
            {
                SelectAbility(0);
            }
        }

        private void OnCastStart()
        {
            if (Agent == Agent.Main)
            {
                var manager = Mission.Current.GetMissionBehavior<AbilityManagerMissionLogic>();
                if (manager != null)
                {
                    manager.OnCastStart();
                }
            }
        }

        private void OnCastComplete()
        {
            if(Agent == Agent.Main)
            {
                var manager = Mission.Current.GetMissionBehavior<AbilityManagerMissionLogic>();
                if(manager != null)
                {
                    manager.OnCastComplete();
                }
            }
        }

        public void InitializeCrosshairs()
        {
            foreach (var ability in KnownAbilities)
            {
                AbilityCrosshair crosshair = AbilityFactory.InitializeCrosshair(ability.Template, Agent.Main);
                ability.SetCrosshair(crosshair);
            }
            SelectAbility(0);
        }

        public void SelectAbility(int index)
        {
            if (_knownAbilities.Count > 0)
            {
                CurrentAbility = _knownAbilities[index];
            }
        }

        public void SelectNextAbility()
        {
            if (_currentAbilityIndex < _knownAbilities.Count - 1)
            {
                _currentAbilityIndex++;
            }
            else
            {
                _currentAbilityIndex = 0;
            }
            SelectAbility(_currentAbilityIndex);
        }

        public void SelectPreviousAbility()
        {
            if (_currentAbilityIndex > 0)
            {
                _currentAbilityIndex--;
            }
            else
            {
                _currentAbilityIndex = _knownAbilities.Count - 1;
            }
            SelectAbility(_currentAbilityIndex);
        }

        public void StopSpecialMove()
        {
            ((ShadowStepScript)SpecialMove.AbilityScript)?.Stop();
        }

        public List<AbilityTemplate> GetKnownAbilityTemplates()
        {
            return _knownAbilities.ConvertAll(ability => ability.Template);
        }

        public Ability GetAbility(int index)
        {
            if (_knownAbilities.Count > 0 && index >= 0)
            {
                return _knownAbilities[index % _knownAbilities.Count];
            }

            return null;
        }

        private Ability _currentAbility = null;
        private SpecialMove _specialMove = null;
        private readonly List<Ability> _knownAbilities = new List<Ability>();
        private int _currentAbilityIndex;
        public Ability CurrentAbility
        {
            get => _currentAbility;
            set
            {
                _currentAbility = value;
                CurrentAbilityChanged?.Invoke(_currentAbility.Crosshair);
            }
        }
        public SpecialMove SpecialMove { get => _specialMove; private set => _specialMove = value; }
        public List<Ability> KnownAbilities { get => _knownAbilities; }
        public delegate void CurrentAbilityChangedHandler(AbilityCrosshair crosshair);
        public event CurrentAbilityChangedHandler CurrentAbilityChanged;
        private int _maxArtilleryAmount;
        public int MaxArtilleryAmount
        {
            get => _maxArtilleryAmount;
        }
    }
}