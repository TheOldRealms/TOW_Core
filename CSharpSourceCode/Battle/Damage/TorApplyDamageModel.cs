using System;
using System.Windows.Forms.VisualStyles;
using Helpers;
using JetBrains.Annotations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;
using TaleWorlds.TwoDimension;
using TOW_Core.Items;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.Damage
{
    public class TorAgentApplyDamageModel : AgentApplyDamageModel
    {
      
      private float _minimumMagicTriggerDamage = 15f;
      public override float CalculateDamage(
        ref AttackInformation attackInformation,
        ref AttackCollisionData collisionData,
        in MissionWeapon weapon,
        float baseDamage)
      {

        float additionalDamage =0;
        float combinedArmorValue = 0;
        float combinedArmorWeight = 0;
        bool human = attackInformation.IsVictimAgentHuman;
        IAgentOriginBase attackerAgentOrigin = attackInformation.AttackerAgentOrigin;
        Formation attackerFormation = attackInformation.AttackerFormation;
        BasicCharacterObject attackerAgentCharacter = attackInformation.AttackerAgentCharacter;
        BasicCharacterObject captainCharacter1 = attackInformation.AttackerCaptainCharacter;
        
        IAgentOriginBase victimAgentOrigin = attackInformation.VictimAgentOrigin;
        BasicCharacterObject victimAgentCharacter = attackInformation.VictimAgentCharacter;
        BasicCharacterObject captainCharacter2 = attackInformation.VictimCaptainCharacter;
        WeaponComponentData currentUsageItem1 = attackInformation.VictimMainHandWeapon.CurrentUsageItem;

        //access weapon traits of a mission weapon
        var traits = weapon.Item.GetTraits();
        
        
        
        
        
        
        foreach (var trait in traits)
        {
          TOWCommon.Say(trait.ItemTraitName);
        }

        //attacker data 
        if (attackerAgentCharacter != null)
        {
          var body = attackerAgentCharacter.Equipment.GetEquipmentFromSlot(EquipmentIndex.Body);
          var head = attackerAgentCharacter.Equipment.GetEquipmentFromSlot(EquipmentIndex.Head);
          var legs = attackerAgentCharacter.Equipment.GetEquipmentFromSlot(EquipmentIndex.Leg);
          var hands = attackerAgentCharacter.Equipment.GetEquipmentFromSlot(EquipmentIndex.Gloves);
          var shoulder = attackerAgentCharacter.Equipment.GetEquipmentFromSlot(EquipmentIndex.Cape);
          
          /*TorArmorItem bodyArmor = TORItemBaseManager.GetArmorFromDataBase(body.Item.ToString());
          TorArmorItem headArmor = head.Item != null
            ? TORItemBaseManager.GetArmorFromDataBase(head.Item.ToString())
            : new TorArmorItem();
          TorArmorItem legsArmor = legs.Item != null
            ? TORItemBaseManager.GetArmorFromDataBase(legs.Item.ToString())
            : new TorArmorItem();
          TorArmorItem armArmor = hands.Item != null
            ? TORItemBaseManager.GetArmorFromDataBase(hands.Item.ToString())
            : new TorArmorItem();
          TorArmorItem shoulderArmor = shoulder.Item != null
            ? TORItemBaseManager.GetArmorFromDataBase(shoulder.Item.ToString())
            : new TorArmorItem();*/
        }
        

        var victimShieldItem = attackInformation.VictimShield;
        

        //defender data 
        
        if (victimAgentCharacter != null&& attackInformation.IsVictimAgentHuman)
        {

          var body = victimAgentCharacter.Equipment.GetEquipmentFromSlot(EquipmentIndex.Head);
          var head = victimAgentCharacter.Equipment.GetEquipmentFromSlot(EquipmentIndex.Body);
          var shoulder = victimAgentCharacter.Equipment.GetEquipmentFromSlot(EquipmentIndex.Cape);
          var hands = victimAgentCharacter.Equipment.GetEquipmentFromSlot(EquipmentIndex.Gloves);
          var legs = victimAgentCharacter.Equipment.GetEquipmentFromSlot(EquipmentIndex.Leg);

          /*combinedArmorWeight += body.Weight;
          combinedArmorWeight += head.Weight;
          combinedArmorWeight += shoulder.Weight;
          combinedArmorWeight += hands.Weight;
          combinedArmorWeight += legs.Weight;*/
          
          
          /*
          var bodyAdditionalProperties = TryGetAdditionalTorArmorValues(body);
          var headAdditionalProperties = TryGetAdditionalTorArmorValues(head);
          var shoulderAdditionalProperties = TryGetAdditionalTorArmorValues(shoulder);
          var handsAdditionalProperties = TryGetAdditionalTorArmorValues(hands);
          var legsAdditionalProperties = TryGetAdditionalTorArmorValues(legs);
          */

          
          /*
          combinedArmorValue += bodyAdditionalProperties.ArmorValue;
          combinedArmorValue += headAdditionalProperties.ArmorValue;
          combinedArmorValue += shoulderAdditionalProperties.ArmorValue;
          combinedArmorValue += handsAdditionalProperties.ArmorValue;
          combinedArmorValue += legsAdditionalProperties.ArmorValue;
          */


          /*
          if (attackInformation.VictimShield.Item != null)
          {
            var shield = TryGetAdditionalTorArmorValues(attackInformation.VictimShield.ToString());
          }*/


          //todo retrieve global armor data
            // maybe some attributes are acting globally, and therefore should still have a effect though not hit
            
            
            /*if(!(collisionData.AttackBlockedWithShield|| collisionData.CollidedWithShieldOnBack)) 
           //   additionalDamage=   CalculateDamageForBodyPart(collisionData.VictimHitBodyPart, weaponItem, headAdditionalProperties, collisionData);
            else
            {
              CalculateShieldDamage()
            }*/

        }
        
        
        //get Base Armor values
      // additionalDamage = weaponItem.FlatDamage;

        // recalculate base damage with new weapon
        //Attack information
        //float defenceEfficiency= CalculateMagicDefenseEffiency(combinedArmorValue, victimAgentCharacter.Equipment.GetTotalWeightOfArmor(true));
        
        
        /*float damageEfficiency = CalculateAddtionalMagicDamageEffiency(baseDamage, 15);
        float damageValue = damageEfficiency * additionalDamage;

        float defenceEffiency;
        float absorbValue = 0;
        if (attackInformation.IsVictimAgentHuman)
        {
          float defenceEfficiency = CalculateMagicDefenseEffiency(damageValue,50f);
          absorbValue = defenceEfficiency * combinedArmorValue;
        }
        
        
        
        if ((baseDamage + damageValue)>_minimumMagicTriggerDamage)
          return baseDamage+(damageValue)-absorbValue;*/

        TOWCommon.Say(weapon.Item.StringId+ " "+  baseDamage);
        
        
        return baseDamage;

        //return Mathf.Max(baseDamage - absorbValue, 0);

      }

      public override float CalculateEffectiveMissileSpeed(Agent attackerAgent, WeaponComponentData missileWeapon,
        ref Vec3 missileStartDirection, float missileStartSpeed)
      {
        return missileStartSpeed;
      }

      private float CalculateMagicDefenseEffiency(float damage, float weight)
      { 
        var result = (MathF.Log((damage*5)/((weight*100)*5))*50f)/ 100;
        return Mathf.Max(result, 0);
      }
      private float CalculateAddtionalMagicDamageEffiency(float damage, float threshold)
      {
        if (damage < threshold)
          return 0f;
        
        return (MathF.Log(5*damage) * 16)/100 ;
      }

      /*
      private TorWeaponItem TryGetAdditionalTorWeaponValues(MissionWeapon? item)
      {
        if (item.Value.Item != null)
        {
          TorWeaponItem TorItem = TORItemBaseManager.GetWeaponFromDataBase(item.Value.Item.ToString());
          return TorItem;
        }
        else
        {
          return new TorWeaponItem();
        }
      }
      
      private TorArmorItem TryGetAdditionalTorArmorValues(EquipmentElement? item)
      {
        if (item.Value.Item != null)
        {
          TorArmorItem TorItem = TORItemBaseManager.GetArmorFromDataBase(item.Value.Item.ToString());
          return TorItem;
        }
        return new TorArmorItem();

      }
      */
      
      
      /*private TorArmorItem TryGetAdditionalTorArmorValues([CanBeNull] string item)
      {
        if (!string.IsNullOrEmpty(item))
        {
          TorArmorItem TorItem = TORItemBaseManager.GetArmorFromDataBase(item);
          return TorItem;
        }
        return new TorArmorItem();
      }

      private float CalculateDamageForBodyPart(BoneBodyPartType bodyPart, TorWeaponItem torWeapon, TorArmorItem torArmor, AttackCollisionData collisionData)
      {
        var damage = Math.Max(torWeapon.FlatDamage - torArmor.ArmorValue, 0);

        return (float) damage;
      }

      public override float CalculateEffectiveMissileSpeed(Agent attackerAgent, WeaponComponentData missileWeapon,
          ref Vec3 missileStartDirection, float missileStartSpeed)
      {
        throw new NotImplementedException();
      }*/
      

    public override void DecideMissileWeaponFlags(
      Agent attackerAgent,
      MissionWeapon missileWeapon,
      ref WeaponFlags missileWeaponFlags)
    {
      if (!(attackerAgent?.Character is CharacterObject character) || missileWeapon.CurrentUsageItem.WeaponClass != WeaponClass.Javelin || !character.GetPerkValue(DefaultPerks.Throwing.Impale))
        return;
      missileWeaponFlags |= WeaponFlags.CanPenetrateShield;
    }

    public override float CalculateDismountChanceBonus(
      Agent attackerAgent,
      WeaponComponentData weapon)
    {
      CharacterObject character = attackerAgent?.Character as CharacterObject;
      float num = 0.0f;
      if (character != null && weapon != null)
      {
        if (weapon.RelevantSkill == DefaultSkills.Polearm && character.GetPerkValue(DefaultPerks.Polearm.Braced))
          num += DefaultPerks.Polearm.Braced.PrimaryBonus / 100f;
        else if (weapon.RelevantSkill == DefaultSkills.Crossbow && weapon.IsConsumable && character.GetPerkValue(DefaultPerks.Crossbow.HammerBolts))
          num += DefaultPerks.Crossbow.HammerBolts.PrimaryBonus / 100f;
        else if (weapon.RelevantSkill == DefaultSkills.Throwing && weapon.IsConsumable && character.GetPerkValue(DefaultPerks.Throwing.KnockOff))
          num += DefaultPerks.Throwing.KnockOff.PrimaryBonus / 100f;
      }
      return num;
    }

    public override float CalculateKnockBackChanceBonus(
      Agent attackerAgent,
      WeaponComponentData weapon)
    {
      CharacterObject character = attackerAgent?.Character as CharacterObject;
      float num = 0.0f;
      if (character != null && weapon != null && weapon.RelevantSkill == DefaultSkills.Polearm && character.GetPerkValue(DefaultPerks.Polearm.KeepAtBay))
        num += DefaultPerks.Polearm.KeepAtBay.PrimaryBonus / 100f;
      return num;
    }

    public override float CalculateKnockDownChanceBonus(
      Agent attackerAgent,
      WeaponComponentData weapon)
    {
      CharacterObject character = attackerAgent?.Character as CharacterObject;
      float num = 0.0f;
      if (character != null && weapon != null)
      {
        if (weapon.RelevantSkill == DefaultSkills.Polearm && character.GetPerkValue(DefaultPerks.Polearm.HardKnock))
          num += DefaultPerks.Polearm.HardKnock.PrimaryBonus / 100f;
        else if (weapon.RelevantSkill == DefaultSkills.TwoHanded && character.GetPerkValue(DefaultPerks.TwoHanded.ShowOfStrength))
          num += DefaultPerks.TwoHanded.ShowOfStrength.PrimaryBonus / 100f;
      }
      return num;
    }

    public override void CalculateCollisionStunMultipliers(
      Agent attackerAgent,
      Agent defenderAgent,
      bool isAlternativeAttack,
      CombatCollisionResult collisionResult,
      WeaponComponentData attackerWeapon,
      WeaponComponentData defenderWeapon,
      out float attackerStunMultiplier,
      out float defenderStunMultiplier)
    {
      float val2_1 = 1f;
      float val2_2 = 1f;
      if (attackerAgent.Character is CharacterObject character && (collisionResult == CombatCollisionResult.Blocked || collisionResult == CombatCollisionResult.Parried) && character.GetPerkValue(DefaultPerks.Athletics.MightyBlow))
        val2_1 += val2_1 * DefaultPerks.Athletics.MightyBlow.PrimaryBonus;
      defenderStunMultiplier = Math.Max(0.0f, val2_1);
      attackerStunMultiplier = Math.Max(0.0f, val2_2);
    }

    public override float CalculateStaggerThresholdMultiplier(Agent defenderAgent)
    {
      float val1 = 1f;
      CharacterObject character = defenderAgent.Character as CharacterObject;
      CharacterObject captainCharacter = defenderAgent.Formation?.Captain?.Character as CharacterObject;
      if (character != null)
      {
        if (captainCharacter == character)
          captainCharacter = (CharacterObject) null;
        ExplainedNumber bonuses = new ExplainedNumber(1f);
        if (defenderAgent.HasMount)
          PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Riding.DauntlessSteed, character, true, ref bonuses);
        else
          PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Athletics.Spartan, character, true, ref bonuses);
        if (!defenderAgent.WieldedWeapon.IsEmpty && defenderAgent.WieldedWeapon.CurrentUsageItem.WeaponClass == WeaponClass.Crossbow && defenderAgent.GetCurrentActionType(1) == Agent.ActionCodeType.Reload)
        {
          PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Crossbow.DeftHands, character, true, ref bonuses);
          if (captainCharacter != null)
            PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Crossbow.DeftHands, captainCharacter, ref bonuses);
        }
        val1 = bonuses.ResultNumber;
      }
      return Math.Max(val1, 0.0f);
    }

    public override float CalculatePassiveAttackDamage(
      BasicCharacterObject attackerCharacter,
      ref AttackCollisionData collisionData,
      float baseDamage)
    {
      if (attackerCharacter is CharacterObject characterObject && collisionData.AttackBlockedWithShield && characterObject.GetPerkValue(DefaultPerks.Polearm.UnstoppableForce))
        baseDamage += baseDamage * DefaultPerks.Polearm.UnstoppableForce.PrimaryBonus;
      return baseDamage;
    }

    public override MeleeCollisionReaction DecidePassiveAttackCollisionReaction(
      Agent attacker,
      Agent defender,
      bool isFatalHit)
    {
      MeleeCollisionReaction collisionReaction = MeleeCollisionReaction.Bounced;
      if (isFatalHit && attacker.HasMount)
      {
        float num = 0.05f;
        if (attacker.Character is CharacterObject character2 && character2.GetPerkValue(DefaultPerks.Polearm.Skewer))
          num += DefaultPerks.Polearm.Skewer.PrimaryBonus;
        if ((double) MBRandom.RandomFloat < (double) num)
          collisionReaction = MeleeCollisionReaction.SlicedThrough;
      }
      return collisionReaction;
    }

    public override float CalculateShieldDamage(float baseDamage) => baseDamage;

    public override float GetDamageMultiplierForBodyPart(
      BoneBodyPartType bodyPart,
      DamageTypes type,
      bool isHuman)
    {
      float num = 1f;
      switch (bodyPart)
      {
        case BoneBodyPartType.None:
          num = 1f;
          break;
        case BoneBodyPartType.Head:
          switch (type)
          {
            case DamageTypes.Invalid:
              num = 2f;
              break;
            case DamageTypes.Cut:
              num = 1.2f;
              break;
            case DamageTypes.Pierce:
              num = !isHuman ? 1.2f : 2f;
              break;
            case DamageTypes.Blunt:
              num = 1.2f;
              break;
          }
          break;
        case BoneBodyPartType.Neck:
          switch (type)
          {
            case DamageTypes.Invalid:
              num = 2f;
              break;
            case DamageTypes.Cut:
              num = 1.2f;
              break;
            case DamageTypes.Pierce:
              num = !isHuman ? 1.2f : 2f;
              break;
            case DamageTypes.Blunt:
              num = 1.2f;
              break;
          }
          break;
        case BoneBodyPartType.Chest:
        case BoneBodyPartType.Abdomen:
        case BoneBodyPartType.ShoulderLeft:
        case BoneBodyPartType.ShoulderRight:
        case BoneBodyPartType.ArmLeft:
        case BoneBodyPartType.ArmRight:
          num = isHuman ? 1f : 0.8f;
          break;
        case BoneBodyPartType.Legs:
          num = 0.8f;
          break;
      }
      return num;
    }

    public override bool DecideCrushedThrough(Agent attackerAgent, Agent defenderAgent, float totalAttackEnergy,
      Agent.UsageDirection attackDirection, StrikeType strikeType, WeaponComponentData defendItem, bool isPassiveUsageHit)
    {
      
      if (strikeType == StrikeType.Thrust)
      {
        
      }
      
      return false;
    }

    public override bool CanWeaponIgnoreFriendlyFireChecks(WeaponComponentData weapon) => weapon != null && weapon.IsConsumable && weapon.WeaponFlags.HasAnyFlag<WeaponFlags>(WeaponFlags.CanPenetrateShield) && weapon.WeaponFlags.HasAnyFlag<WeaponFlags>(WeaponFlags.MultiplePenetration);
    }
}