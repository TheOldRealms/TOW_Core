using NLog;
using System;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities
{
    public class AbilityComponent : AgentComponent
    {
        private Ability _currentAbility = null;
        private readonly List<Ability> _knownAbilities = new List<Ability>();
        private int _currentAbilityIndex;

        public Ability CurrentAbility { get => _currentAbility; set => _currentAbility = value; }

        public AbilityComponent(Agent agent) : base(agent)
        {
            var abilities = agent.GetAbilities();
            if(abilities.Count > 0)
            {
                foreach (var item in abilities)
                {
                    try
                    {
                        var ability = AbilityFactory.CreateNew(item);
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
