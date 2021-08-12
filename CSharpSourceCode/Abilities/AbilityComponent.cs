using NLog;
using System;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities.Crosshairs;
using TOW_Core.Battle.CrosshairMissionBehavior;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities
{
    public class AbilityComponent : AgentComponent
    {
        private bool isAbilityModeOn;
        private Ability _currentAbility = null;
        private readonly List<Ability> _knownAbilities = new List<Ability>();
        private int _currentAbilityIndex;

        public bool IsAbilityModeOn { get => isAbilityModeOn; private set => isAbilityModeOn = value; }
        public Ability CurrentAbility 
        { 
            get => _currentAbility;
            set
            {
                _currentAbility = value;
                CurrentAbilityChanged?.Invoke(_currentAbility.Crosshair);
            }
        }
        public List<Ability> KnownAbilities { get => _knownAbilities; }
        public delegate void CurrentAbilityChangedHandler(AbilityCrosshair crosshair);
        public event CurrentAbilityChangedHandler CurrentAbilityChanged;

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
                            _knownAbilities.Add(ability);
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
            if (_knownAbilities.Count > 0 && index >= 0)
            {
                _currentAbilityIndex = Math.Abs(index % _knownAbilities.Count);
                CurrentAbility = _knownAbilities[_currentAbilityIndex];
            }
        }

        public void SelectNextAbility()
        {
            SelectAbility(_currentAbilityIndex + 1);
        }
        public void SelectPreviousAbility()
        {
            SelectAbility(_currentAbilityIndex - 1);
        }

        public Ability[] GetAbilities()
        {
            return _knownAbilities.ToArray();
        }
        public void EnableAbilityMode()
        {
            isAbilityModeOn = true;
        }
        public void DisableAbilityMode()
        {
            isAbilityModeOn = false;
        }
    }
}
