﻿using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using TOW_Core.ObjectDataExtensions;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport
{
    public class TORWanderersCampaignBehavior : CampaignBehaviorBase
    {
        public override void SyncData(IDataStore dataStore)
        {
        }

        public override void RegisterEvents()
        {
            CampaignEvents.AfterSettlementEntered.AddNonSerializedListener(this, OnAfterSettlementEntered);
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, CheckPlayerCurrentSettlement);
        }

        private void CheckPlayerCurrentSettlement()
        {
            var playerSettlement = MobileParty.MainParty.CurrentSettlement;
            if (playerSettlement != null && playerSettlement.IsTown)
            {
                ReplaceEnemyWanderersIfExist(playerSettlement);
            }
        }

        private void OnAfterSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
        {
            if (!settlement.IsTown || mobileParty == null || !mobileParty.IsMainParty)
            {
                return;
            }

            //check for unsuitable wanderers
            ReplaceEnemyWanderersIfExist(settlement);
        }

        private void ReplaceEnemyWanderersIfExist(Settlement settlement)
        {
            for (int i = 0; i < settlement.HeroesWithoutParty.Count; i++)
            {
                var wanderer = settlement.HeroesWithoutParty[i];
                if (wanderer != null && wanderer.Occupation == Occupation.Wanderer && wanderer.Culture != settlement.Culture)
                {
                    //look for empty suitable settlement to move unsuitable wanderer
                    var suitableTown = (from x in Town.AllTowns
                                        where x.Settlement.Culture == wanderer.Culture
                                        orderby x.Settlement.HeroesWithoutParty.Count ascending
                                        select x).FirstOrDefault().Settlement;
                    if (suitableTown != null)
                    {
                        EnterSettlementAction.ApplyForCharacterOnly(wanderer, suitableTown);
                    }
                    else
                    {
                        LeaveSettlementAction.ApplyForCharacterOnly(wanderer);
                        wanderer.ChangeState(Hero.CharacterStates.NotSpawned);
                    }
                }
            }

            if (settlement.HeroesWithoutParty.Where(h => h.Occupation == Occupation.Wanderer).Count() == 0)
            {
                //create suitable wanderer
                CharacterObject template = settlement.Culture.NotableAndWandererTemplates.Where(h => h.Occupation == Occupation.Wanderer).GetRandomElementInefficiently();
                if (template != null)
                {
                    Hero newWanderer = HeroCreator.CreateSpecialHero(template, settlement, null, null, HeroConstants.VAMPIRE_MAX_AGE + MBRandom.RandomInt(27));
                    AdjustEquipmentImp(newWanderer.BattleEquipment);
                    AdjustEquipmentImp(newWanderer.CivilianEquipment);
                    newWanderer.ChangeState(Hero.CharacterStates.Active);
                    EnterSettlementAction.ApplyForCharacterOnly(newWanderer, settlement);
                }
            }
        }

        private void AdjustEquipmentImp(Equipment equipment)
        {
            ItemModifier @object = MBObjectManager.Instance.GetObject<ItemModifier>("companion_armor");
            ItemModifier object2 = MBObjectManager.Instance.GetObject<ItemModifier>("companion_weapon");
            ItemModifier object3 = MBObjectManager.Instance.GetObject<ItemModifier>("companion_horse");
            for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumEquipmentSetSlots; equipmentIndex++)
            {
                EquipmentElement equipmentElement = equipment[equipmentIndex];
                if (equipmentElement.Item != null)
                {
                    if (equipmentElement.Item.ArmorComponent != null)
                    {
                        equipment[equipmentIndex] = new EquipmentElement(equipmentElement.Item, @object, null, false);
                    }
                    else if (equipmentElement.Item.HorseComponent != null)
                    {
                        equipment[equipmentIndex] = new EquipmentElement(equipmentElement.Item, object3, null, false);
                    }
                    else if (equipmentElement.Item.WeaponComponent != null)
                    {
                        equipment[equipmentIndex] = new EquipmentElement(equipmentElement.Item, object2, null, false);
                    }
                }
            }
        }
    }
}
