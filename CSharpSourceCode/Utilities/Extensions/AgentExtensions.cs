using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.StatusEffects;
using TaleWorlds.CampaignSystem;
using TOW_Core.Battle.Damage;
using TOW_Core.Items;
using TOW_Core.ObjectDataExtensions;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.CustomBattle;

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

        public static bool IsVampire(this Agent agent)
        {
            return agent.GetAttributes().Contains("VampireBodyOverride");
        }

        public static bool IsAbilityUser(this Agent agent)
        {
            return agent.GetAttributes().Contains("AbilityUser");
        }

        public static bool IsSpellCaster(this Agent agent)
        {
            return agent.GetAttributes().Contains("SpellCaster");
        }

        public static bool CanPlaceArtillery(this Agent agent)
        {
            return agent.GetAttributes().Contains("CanPlaceArtillery");
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
        
        /// <summary>
        /// Get Extented Character properties, based on dynamic properties like temporary weapon enhancements
        /// and status effects and static properties  like equipment and unit information.
        /// 
        /// </summary>
        /// <param name="agent">The agent we are accessing. Note that Units are handled differently from Heroes.
        /// Hero equipment information and gets read out for Heroes specifically resides in the 'tow_extendeditemproperties.xml'
        /// Unit information gets read out from the tow_extendedunitproperties.xml
        /// </param>
        /// <paramref name="agent"/>
        /// <param name="mask">Which properties needs to be accessed? be mindful about which information is really required for your specific situation. Not used properties are returned as empty arrays</param>
        ///<returns>A struct containing 3 arrays, each containing the  properties:  proportions, damage amplifications and resistances.</returns>
        public static AgentPropertyContainer GetProperties(this Agent agent, PropertyMask mask=PropertyMask.All)
        {
            if (agent.IsMount)
                return new AgentPropertyContainer();
            
            float[] damageProportions = new float[(int) DamageType.All + 1];
            float[] damageAmplifications = new float[(int) DamageType.All + 1];
            float[] damageResistances = new float[(int) DamageType.All + 1];
            float[] additionalDamagePercentages = new float[(int) DamageType.All + 1];
            
            #region Unit
            if (!agent.IsHero)
            {
                if (mask ==PropertyMask.Attack|| mask== PropertyMask.All)
                {
                    var unitDamageProportion = agent.Character.GetUnitDamageProportions();
                    foreach (var proportionTuple in unitDamageProportion)
                    {
                        damageProportions[(int)proportionTuple.DamageType] = proportionTuple.Percent;
                    }
                    var offenseProperties = agent.Character.GetAttackProperties();
                    
                    //add all offense properties of the Unit
                    foreach (var property in offenseProperties)
                    {
                        damageAmplifications[(int) property.AmplifiedDamageType] += property.DamageAmplifier;
                    }
                    //add temporary effects like buffs to attack bonuses on items
                    List<ItemTrait> dynamicTraits = agent.GetComponent<ItemTraitAgentComponent>()
                        .GetDynamicTraits(agent.WieldedWeapon.Item);
                    foreach (var dynamicTrait in dynamicTraits)
                    {
                        var attackProperty = dynamicTrait.AmplifierTuple;
                       
                        if (attackProperty != null)
                        {
                            damageAmplifications[(int) attackProperty.AmplifiedDamageType] += attackProperty.DamageAmplifier;
                        }
                        var additionalDamageProperty = dynamicTrait.AdditionalDamageTuple;
                        if (additionalDamageProperty != null)
                        {
                            additionalDamagePercentages[(int)additionalDamageProperty.DamageType] += additionalDamageProperty.Percent;
                        }
                    }
                    var statusEffectAmplifiers = agent.GetComponent<StatusEffectComponent>().GetAmplifiers();
                    for (int i = 0; i < damageAmplifications.Length; i++)
                    {
                        damageAmplifications[i] += statusEffectAmplifiers[i];
                    }
                }
                if(mask == PropertyMask.Defense|| mask== PropertyMask.All)      
                {
                    //add all offense properties of the Unit
                    var defenseProperties = agent.Character.GetDefenseProperties();
                
                    foreach (var property in defenseProperties)
                    {
                        damageResistances[(int) property.ResistedDamageType] += property.ReductionPercent;
                    }
                
                    //add temporary effects like buffs to defense bonuses
                    List<ItemTrait> dynamicTraits = agent.GetComponent<ItemTraitAgentComponent>()
                        .GetDynamicTraits(agent.WieldedWeapon.Item);
                
                    foreach (var dynamicTrait in dynamicTraits)
                    {
                        var defenseProperty = dynamicTrait.ResistanceTuple;
                        if (defenseProperty != null)
                        {
                            damageResistances[(int) defenseProperty.ResistedDamageType] += defenseProperty.ReductionPercent;
                        }
                    }

                    //statuseffects
                    var statusEffectResistances = agent.GetComponent<StatusEffectComponent>().GetResistances();

                    for (int i = 0; i < damageResistances.Length; i++)
                    {
                        damageResistances[i] += statusEffectResistances[i];
                    }
                }
            }
            #endregion
            
            #region Character
             
            else
            {
                if (mask ==PropertyMask.Attack|| mask== PropertyMask.All)
                {
                    //Hero item level attributes 
                    List<ItemTrait> itemTraits= new List<ItemTrait>();
                    List<ItemObject> items;
                    // get all equipment Pieces
                    items = agent.Character.GetCharacterEquipment(EquipmentIndex.ArmorItemBeginSlot);
                    foreach (var item in items)
                    {
                        if(item.HasTrait())
                            itemTraits.AddRange(item.GetTraits(agent));
                    }
                    //equipment amplifiers  , also implies dynamic traits
                    foreach (var itemTrait in itemTraits)
                    {
                        var property = itemTrait.AmplifierTuple;
                        if(property==null) 
                            continue;
                        damageAmplifications[(int) property.AmplifiedDamageType] += property.DamageAmplifier;

                        var additionalDamageProperty = itemTrait.AdditionalDamageTuple;
                        if (additionalDamageProperty != null)
                        {
                            additionalDamagePercentages[(int)additionalDamageProperty.DamageType] += additionalDamageProperty.Percent;
                        }
                        
                    }
                    
                    var statusEffectAmplifiers = agent.GetComponent<StatusEffectComponent>().GetAmplifiers();

                    for (int i = 0; i < damageAmplifications.Length; i++)
                    {
                        damageAmplifications[i] += statusEffectAmplifiers[i];
                    }
                
                    //weapon properties
                    if (agent.WieldedWeapon.Item != null)
                    {
                        var weaponProperty = agent.WieldedWeapon.Item.GetTorSpecificData().DamageProportions;
                        if (weaponProperty != null)
                        {
                            foreach (var tuple in weaponProperty)
                            {
                                damageProportions[(int)tuple.DamageType] = tuple.Percent;
                            }
                        }
                        
                    }
                }
                if( mask == PropertyMask.Defense|| mask== PropertyMask.All)
                {
                    //Hero item level attributes 

                    List<ItemTrait> itemTraits= new List<ItemTrait>();
                    List<ItemObject> items;
                    
                    items = agent.Character.GetCharacterEquipment();
                    foreach (var item in items) 
                    {
                        if(item.HasTrait())
                            itemTraits.AddRange(item.GetTraits(agent));
                    }
                    
                    //equipment resistances , also implies dynamic traits
                    foreach (var itemTrait in itemTraits)
                    {
                        var defenseProperty = itemTrait.ResistanceTuple;
                        if(defenseProperty==null) 
                            continue;
                        damageResistances[(int)defenseProperty.ResistedDamageType] += defenseProperty.ReductionPercent;
                    }

                    //statuseffects
                    var statusEffectResistances = agent.GetComponent<StatusEffectComponent>().GetResistances();

                    for (int i = 0; i < damageResistances.Length; i++)
                    {
                        damageResistances[i] += statusEffectResistances[i];
                    }
                }
            }
            #endregion

            return new AgentPropertyContainer(damageProportions, damageAmplifications, damageResistances, additionalDamagePercentages);
            

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
                hero = Hero.FindFirst(x => x.StringId == agent.Character.StringId);
            }
            return hero;
        }

        public static int GetPlaceableArtilleryCount(this Agent agent)
        {
            int count = 0;
            if (agent.CanPlaceArtillery())
            {
                if(Game.Current.GameType is Campaign && agent.GetHero() != null)
                {
                    count = agent.GetHero().GetPlaceableArtilleryCount();
                }
                else if(Game.Current.GameType is CustomGame)
                {
                    count = 5;
                }
            }
            return count;
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
        /// <param name="doBlow">A mask that controls whether the unit receives a blow or direct health manipulation</param>
        public static void ApplyDamage(this Agent agent, int damageAmount, Agent damager = null, bool doBlow = true, bool hasShockWave = false, Vec3 impactPosition = new Vec3())
        {
            if (agent == null && !agent.IsHuman)
            {
                TOWCommon.Log("ApplyDamage: attempted to apply damage to a null or non-human agent.", LogLevel.Warn);
                return;
            }
            try
            {
                // Registering a blow causes the agent to react/stagger. Manipulate health directly if the damage won't kill the agent.
                if (agent.State == AgentState.Active|| agent.State == AgentState.Routed)
                {
                    if (!doBlow && agent.Health > damageAmount )
                    {
                        agent.Health-= damageAmount;
                        return;
                    }

                    if (agent.IsFadingOut())
                        return;
                    
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
                        if (checkAgent != null && checkAgent.Equals(damager))
                        {
                            blow.OwnerId = damager.Index;
                            blow.Position = impactPosition;
                            blow.Direction = agent.Position - impactPosition;
                            blow.Direction.Normalize();
                            blow.SwingDirection = blow.Direction;
                        }
                    }
                    else
                    {
                        blow.InflictedDamage = 0;
                        blow.SelfInflictedDamage = damageAmount;
                        blow.OwnerId = agent.Index;
                    }
                    
                    if (agent.Health  <= damageAmount&&!doBlow)
                    {
                        agent.Die(blow);
                        return;
                    }
                    agent.RegisterBlow(blow);
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

        public static void ApplyStatusEffect(this Agent agent, string effectId, Agent applierAgent)
        {
            var comp = agent.GetComponent<StatusEffectComponent>();
            if(comp != null) comp.RunStatusEffect(effectId, applierAgent);
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
            agent.AgentVisuals?.SetVisible(true);
        }

        public static void Disappear(this Agent agent)
        {
            agent.AgentVisuals?.SetVisible(false);
        }
    }
}