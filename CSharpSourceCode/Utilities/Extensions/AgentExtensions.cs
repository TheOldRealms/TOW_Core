using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.ObjectDataExtensions;
using TOW_Core.Battle.StatusEffects;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Utilities.Extensions
{
    public static class AgentExtensions
    {
        /// <summary>
        /// Maps all character IDs to a list of attributes for that character. For example, <"skeleton_warrior" <=> {"Expendable", "Undead"}>
        /// </summary>
        
        public static bool IsExpendable(this Agent agent)
        {
            return agent.GetAttributes().Contains("Expendable");
        }
        
        public static bool IsHuman(this Agent agent)
        {
            return agent.GetAttributes().Contains("Human");
        }
        
        public static bool IsUndead(this Agent agent)
        {
            return agent.GetAttributes().Contains("Undead");
        }

        public static bool IsAbilityUser(this Agent agent)
        {
            return agent.GetAttributes().Contains("AbilityUser");
        }

        public static bool HasAttribute(this Agent agent, string attributeName)
        {
            return agent.GetAttributes().Contains(attributeName);
        }

        public static void CastCurrentAbility(this Agent agent)
        {
            var abilitycomponent = agent.GetComponent<AbilityComponent>();
           
            if(abilitycomponent != null)
            {
                
                
                if(abilitycomponent.CurrentAbility != null) abilitycomponent.CurrentAbility.Use(agent);
            }
        }

        public static BaseAbility GetCurrentAbility(this Agent agent)
        {
            var abilitycomponent = agent.GetComponent<AbilityComponent>();
            if (abilitycomponent != null)
            {
                return abilitycomponent.CurrentAbility;
            }
            else return null;
        }

        public static void SelectNextAbility(this Agent agent)
        {
            var abilitycomponent = agent.GetComponent<AbilityComponent>();
            if (abilitycomponent != null)
            {
                abilitycomponent.SelectNextAbility();
            }
        }

        public static void AddAbility(this Agent agent, string ability)
        {
            AbilityManager.AddAbility(agent,ability);

            AbilityComponent abilityComponent = new AbilityComponent(agent);

            agent.AddComponent(abilityComponent);
        }

        public static void SelectAbility(this Agent agent, int abilityindex)
        {
            var abilitycomponent = agent.GetComponent<AbilityComponent>();
            if (abilitycomponent != null)
            {
                abilitycomponent.SelectAbility(abilityindex);
            }
        }

        public static List<string> GetAbilities(this Agent agent)
        {
            return AbilityManager.GetAbilitesForCharacter(agent.Character.StringId);
        }

        public static List<string> GetAttributes(this Agent agent)
        {
            if (agent != null && agent.Character != null)
            {
                string characterName = agent.Character.StringId;

                List<string> attributeList;
                if (CharacterIDToAttributeMap.TryGetValue(characterName, out attributeList))
                {
                    return attributeList;
                }
            }
            return new List<string>();
        }

        public static void AddAttribute(this Agent agent, string attribute)
        {
            var characterName = agent.Character.StringId;

            if (!CharacterIDToAttributeMap.ContainsKey(characterName))
            {
                List<string> attributeList= new List<string>();
                attributeList.Add(attribute);
                CharacterIDToAttributeMap.Add(characterName, attributeList);
                return;
            }

            if (CharacterIDToAttributeMap[characterName].Contains(attribute))
            {
                return;
            }
            
            CharacterIDToAttributeMap[characterName].Add(attribute);
            
            
        }

        public static void RemoveComponentIfNotNull(this Agent agent, AgentComponent component)
        {
            if (component != null)
            {
                agent.RemoveComponent(component);
            }
        }

        /// <summary>
        /// Apply damage to an agent. 
        /// </summary>
        /// <param name="agent">The agent that will be damaged</param>
        /// <param name="damageAmount">How much damage the agent will receive.</param>
        /// <param name="damager">The agent who is applying the damage</param>
        /// <param name="causeStagger">A flag that controls whether the unit receives a blow or direct health manipulation</param>
        public static void ApplyDamage(this Agent agent, int damageAmount, Agent damager = null, bool causeStagger = true)
        {
            if (agent == null)
            {
                TOWCommon.Log("ApplyDamage: attempted to apply damage to a null agent.", LogLevel.Warn);
                return;
            }
            try
            {
                // Registering a blow causes the agent to react/stagger. Manipulate health directly if the damage won't kill the agent.
                if (!causeStagger && agent.Health > damageAmount)
                {
                    agent.Health -= damageAmount;
                }
                else
                {
                    bool agentIsActive = agent.State == AgentState.Active;
                    bool agentIsRouted = agent.State == AgentState.Routed;
                    if (agentIsActive || agentIsRouted)
                    {
                        var blow = new Blow();
                        blow.InflictedDamage = damageAmount;
                        blow.DefenderStunPeriod = 0;
                        if (damager != null) blow.OwnerId = damager.Index;
                        agent.RegisterBlow(blow);
                    }
                }
            }
            catch(Exception e)
            {
                TOWCommon.Log("ApplyDamage: attempted to damage agent, but: " + e.Message, LogLevel.Error);
            }
        }

        /// <summary>
        /// Apply healing to an agent.
        /// </summary>
        /// <param name="agent">The agent that will be healed</param>
        /// <param name="healingAmount">How much healing the agent will receive</param>
        public static void Heal(this Agent agent, float healingAmount)
        {
            //Cap healing at the agent's max hit points
            agent.Health = Math.Min(agent.Health + healingAmount, agent.HealthLimit);
        }

        public static void ApplyStatusEffect(this Agent agent, string effectId)
        {
            agent.GetComponent<StatusEffectComponent>().RunStatusEffect(effectId);
        }

        #region voice
        public static void SetAgentVoiceByClassName(this Agent agent, string className)
        {
            int num = SkinVoiceManager.GetVoiceDefinitionCountWithMonsterSoundAndCollisionInfoClassName(className);
            int[] array = new int[num];
            SkinVoiceManager.GetVoiceDefinitionListWithMonsterSoundAndCollisionInfoClassName(className, array);
            MBAgentVisuals mbagentVisuals = (agent != null) ? agent.AgentVisuals : null;
            if (mbagentVisuals != null && array.Length > 0)
            {
                int index = TOWMath.GetRandomInt(0, array.Length);
                int seed = MBRandom.RandomInt();
                int pitchModifier = Math.Abs(seed);
                float voicePitch = (float)pitchModifier * 4.656613E-10f;
                mbagentVisuals.SetVoiceDefinitionIndex(array[index], voicePitch);
            }
        }
        #endregion
    }
}
