using NLog;
using System;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.Extensions;
using TOW_Core.Utilities;

namespace TOW_Core.Abilities
{
    public class AbilityComponent : AgentComponent
    {
        private BaseAbility _currentAbility = null;
        private List<BaseAbility> _knownAbilities = new List<BaseAbility>();
        private int _currentAbilityIndex;

        public BaseAbility CurrentAbility { get => _currentAbility; set => _currentAbility = value; }

        public AbilityComponent(Agent agent) : base(agent)
        {
            var abilities = agent.GetAbilities();
            if(abilities.Count > 0)
            {
                foreach (var ability in abilities)
                {
                    try
                    {
                        object instance = Activator.CreateInstance(Type.GetType(ability));
                        if (instance is BaseAbility)
                        {
                            _knownAbilities.Add(instance as BaseAbility);
                        }
                        else
                        {
                            TOWCommon.Log("Attempted to add an ability to agent: " + agent.Character.StringId + ", but it wasn't of type BaseAbility", LogLevel.Warn);
                        }
                    }
                    catch (Exception)
                    {
                        TOWCommon.Log("Failed instantiating ability class: " + ability, LogLevel.Error);
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
                _currentAbilityIndex = index % _knownAbilities.Count;
                CurrentAbility = _knownAbilities[_currentAbilityIndex];
            }
        }

        public void SelectNextAbility()
        {
            SelectAbility(_currentAbilityIndex + 1);
        }
    }
}
