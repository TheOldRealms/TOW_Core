using NLog;
using System;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities.Crosshairs;
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
                            else if (ability is SpecialMove)
                            {
                                specialMove = (SpecialMove)ability;
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
            isAbilityModeOn = true;
        }

        public void DisableSpellMode()
        {
            isAbilityModeOn = false;
        }

        public void EnableSpecialMoveMode()
        {
            isMovingAbilityReady = true;
            SpecialMove.Crosshair.Show();
        }

        public void DisableSpecialMoveMode()
        {
            isMovingAbilityReady = false;
            SpecialMove.Crosshair.Hide();
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


        private bool isAbilityModeOn;
        private bool isMovingAbilityReady;
        private Ability _currentAbility = null;
        private SpecialMove specialMove = null;
        private readonly List<Ability> _knownAbilities = new List<Ability>();
        private int _currentAbilityIndex;

        public bool IsSpellModeOn { get => isAbilityModeOn; private set => isAbilityModeOn = value; }
        public bool IsSpecialMoveAtReady { get => isMovingAbilityReady; private set => isMovingAbilityReady = value; }
        public Ability CurrentAbility
        {
            get => _currentAbility;
            set
            {
                _currentAbility = value;
                CurrentAbilityChanged?.Invoke(_currentAbility.Crosshair);
            }
        }
        public SpecialMove SpecialMove { get => specialMove; private set => specialMove = value; }
        public List<Ability> KnownAbilities { get => _knownAbilities; }
        public delegate void CurrentAbilityChangedHandler(AbilityCrosshair crosshair);
        public event CurrentAbilityChangedHandler CurrentAbilityChanged;
    }
}