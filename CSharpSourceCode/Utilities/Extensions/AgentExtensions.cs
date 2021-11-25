﻿using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.StatusEffects;
using TaleWorlds.CampaignSystem;

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

        public static bool IsUnbreakable(this Agent agent)
        {
            return agent.GetAttributes().Contains("Unbreakable");
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

        public static bool IsSpellCaster(this Agent agent)
        {
            return agent.GetAttributes().Contains("SpellCaster");
        }

        public static bool HasAttribute(this Agent agent, string attributeName)
        {
            return agent.GetAttributes().Contains(attributeName);
        }

        public static void CastCurrentAbility(this Agent agent)
        {
            var abilitycomponent = agent.GetComponent<AbilityComponent>();

            if (abilitycomponent != null)
            {
                if (abilitycomponent.CurrentAbility != null) abilitycomponent.CurrentAbility.TryCast(agent);
            }
        }

        public static Ability GetCurrentAbility(this Agent agent)
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

        public static void SelectPreviousAbility(this Agent agent)
        {
            var abilitycomponent = agent.GetComponent<AbilityComponent>();
            if (abilitycomponent != null)
            {
                abilitycomponent.SelectPreviousAbility();
            }
        }

        public static void SelectAbility(this Agent agent, int abilityindex)
        {
            var abilitycomponent = agent.GetComponent<AbilityComponent>();
            if (abilitycomponent != null)
            {
                abilitycomponent.SelectAbility(abilityindex);
            }
        }

        public static Hero GetHero(this Agent agent)
        {
            if (agent.Character == null) return null;
            Hero hero = null;
            if (Game.Current.GameType is Campaign)
            {
                var list = Hero.FindAll(x => x.StringId == agent.Character.StringId);
                if (list != null && list.Count() > 0)
                {
                    hero = list.First();
                }
            }
            return hero;
        }

        public static List<string> GetAbilities(this Agent agent)
        {
            var hero = agent.GetHero();
            var character = agent.Character;
            if (hero != null)
            {
                return hero.GetExtendedInfo().AllAbilities;
            }
            else if (character != null)
            {
                return agent.Character.GetAbilities();
            }
            else return new List<string>();
        }

        public static Ability GetAbility(this Agent agent, int abilityindex)
        {
            var abilitycomponent = agent.GetComponent<AbilityComponent>();
            if (abilitycomponent != null)
            {
                return abilitycomponent.GetAbility(abilityindex);
            }

            return null;
        }

        public static List<string> GetAttributes(this Agent agent)
        {
            var hero = agent.GetHero();
            var character = agent.Character;
            if (hero != null)
            {
                return hero.GetExtendedInfo().AllAttributes;
            }
            else if (character != null)
            {
                return agent.Character.GetAttributes();
            }
            else return new List<string>();
        }

        /// <summary>
        /// Apply damage to an agent. 
        /// </summary>
        /// <param name="agent">The agent that will be damaged</param>
        /// <param name="damageAmount">How much damage the agent will receive.</param>
        /// <param name="damager">The agent who is applying the damage</param>
        /// <param name="doBlow">A flag that controls whether the unit receives a blow or direct health manipulation</param>
        public static void  ApplyDamage(this Agent agent, int damageAmount, Agent damager = null, bool doBlow = true, bool hasShockWave = false)
        {
            if (agent == null && !agent.IsHuman)
            {
                TOWCommon.Log("ApplyDamage: attempted to apply damage to a null or non-human agent.", LogLevel.Warn);
                return;
            }
            try
            {
                // Registering a blow causes the agent to react/stagger. Manipulate health directly if the damage won't kill the agent.
                if (agent.State == AgentState.Active || agent.State == AgentState.Routed)
                {
                    
                    if (!doBlow && agent.Health > damageAmount + 1)
                    {
                        agent.Health -= damageAmount;
                        TOWCommon.Say(agent.Name+agent.Index+" health:"+ (agent.Health+damageAmount)+ "-"+ damageAmount +"= "+ agent.Health);
                        return;
                    }
                    
                    
                    else if (agent.Health > 1 && !agent.IsFadingOut())
                    {
                        var blow = new Blow(-1);
                        blow.DamageCalculated = true;
                        blow.InflictedDamage = damageAmount;
                        blow.AttackType = AgentAttackType.Kick;
                        blow.BlowFlag = BlowFlags.NoSound;
                        blow.BaseMagnitude = 5;
                        blow.DamageType = DamageTypes.Invalid;
                        blow.VictimBodyPart = BoneBodyPartType.Chest;
                        blow.StrikeType = StrikeType.Invalid;
                        if (hasShockWave)
                        {
                            if (agent.HasMount)
                                blow.BlowFlag = BlowFlags.CanDismount;
                            else
                                blow.BlowFlag = BlowFlags.KnockDown;
                        }
                        if (damager != null)
                        {
                            var checkAgent = Mission.Current.FindAgentWithIndex(damager.Index);
                            if (checkAgent != null && checkAgent.Equals(damager)) blow.OwnerId = damager.Index;
                            //blow.OwnerId = damager.Index;
                        }
                        else
                        {
                            blow.InflictedDamage = 0;
                            blow.SelfInflictedDamage = damageAmount;
                            blow.OwnerId = agent.Index;
                        }

                        
                        agent.RegisterBlow(blow);

                        if (agent.Health < damageAmount)
                        {
                            
                            if (Mission.Current.GetMissionBehaviour<StatusEffectMissionLogic>() != null)
                            {
                                Mission.Current.GetMissionBehaviour<StatusEffectMissionLogic>().RemoveAgent(agent,blow); 
                            }
                            if(!doBlow) TOWCommon.Say(agent.Name+agent.Index+" died of dot");
                            //agent.RegisterBlow(blow);
                            agent.Die(blow);
                        }
                        
                        
                    }
                    
                    
                    
                }
            }
            catch (Exception e)
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

        public static void ApplyStatusEffect(this Agent agent, string effectId, Agent damager = null)
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

        public static void FallDown(this Agent agent)
        {
            agent.SetActionChannel(0, ActionIndexCache.Create("act_strike_fall_back_heavy_back_rise_continue"));
        }

        public static void Appear(this Agent agent)
        {
            agent.AgentVisuals.SetVisible(true);
        }

        public static void Disappear(this Agent agent)
        {
            agent.AgentVisuals.SetVisible(false);
        }

        public static void KillImmediately(this Agent agent, Agent damager=null)
        {
            var blow = new Blow(-1);
            blow.InflictedDamage = 0;
            blow.DamageCalculated = true;
            blow.InflictedDamage = 1;
            blow.AttackType = AgentAttackType.Kick;
            blow.BlowFlag = BlowFlags.NoSound;
            blow.BaseMagnitude = 5;
            blow.DamageType = DamageTypes.Invalid;
            blow.VictimBodyPart = BoneBodyPartType.Chest;
            blow.StrikeType = StrikeType.Invalid;
            agent.Die(blow);
        }
    }
}
