using NLog;
using System;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
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
                            if (ability is Spell || ability is Prayer)
                            {
                                _knownAbilities.Add(ability);
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

                if (_knownAbilities.Count > 0)
                {
                    SelectAbility(0);
                }
            }
            if (agent.HasAttribute("VampireBodyOverride"))
            {
                _specialMove = (SpecialMove)AbilityFactory.CreateNew("ShadowStep", agent);
            }
        }

        internal void InitializeCrosshairs()
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

        public Ability[] GetAbilities()
        {
            return _knownAbilities.ToArray();
        }

        public void EnableSpellMode()
        {
            _isAbilityModeOn = true;
        }

        public void DisableSpellMode()
        {
            if (_currentAbility != null && _currentAbility.Crosshair != null)
            {
                _currentAbility.Crosshair.Hide();
            }
            _isAbilityModeOn = false;
        }

        public void EnableSpecialMoveMode()
        {
            _isMovingAbilityReady = true;
        }

        public void DisableSpecialMoveMode()
        {
            _isMovingAbilityReady = false;
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


        private bool _isAbilityModeOn;
        private bool _isMovingAbilityReady;
        private Ability _currentAbility = null;
        private SpecialMove _specialMove = null;
        private readonly List<Ability> _knownAbilities = new List<Ability>();
        private int _currentAbilityIndex;

        public bool IsSpellModeOn { get => _isAbilityModeOn; private set => _isAbilityModeOn = value; }
        public bool IsSpecialMoveAtReady { get => _isMovingAbilityReady; private set => _isMovingAbilityReady = value; }
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
    }
}