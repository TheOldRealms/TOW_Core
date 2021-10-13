using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport
{
    public class TORUrbanCharactersCampaignBehavior : CampaignBehaviorBase
    {
        public TORUrbanCharactersCampaignBehavior()
        {
            this._companionSettlements = new Dictionary<Settlement, CampaignTime>();
            this._settlementPassedDaysForWeeklyTick = new Dictionary<Settlement, int>();
            this._companions = new List<Hero>();
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedPartialFollowUpEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter, int>(this.OnNewGameCreatedPartialFollowUp));
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnNewGameCreated));
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.OnSettlementEntered));
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, new Action(this.WeeklyTick));
            CampaignEvents.NewCompanionAdded.AddNonSerializedListener(this, new Action<Hero>(this.CompanionAdded));
            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, new Action<Hero, Hero, KillCharacterAction.KillCharacterActionDetail, bool>(this.OnHeroKilled));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnGameLoaded));
            CampaignEvents.OnGameEarlyLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnGameEarlyLoaded));
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, new Action<Hero>(this.DailyTickHero));
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(this.DailyTickSettlement));
        }

        private void OnGameEarlyLoaded(CampaignGameStarter obj)
        {
            foreach (Hero hero in Hero.AllAliveHeroes)
            {
                if (hero.IsNotable && hero.CurrentSettlement == null && hero.PartyBelongedTo == null && hero.PartyBelongedToAsPrisoner == null && hero.IsActive && hero.HomeSettlement != null && hero.CanHaveQuestsOrIssues())
                {
                    EnterSettlementAction.ApplyForCharacterOnly(hero, hero.HomeSettlement);
                }
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<Dictionary<Settlement, CampaignTime>>("companionSettlements", ref this._companionSettlements);
            dataStore.SyncData<Dictionary<Settlement, int>>("_settlementPassedDaysForWeeklyTick", ref this._settlementPassedDaysForWeeklyTick);
            dataStore.SyncData<List<Hero>>("companions", ref this._companions);
        }

        public void OnNewGameCreated(CampaignGameStarter campaignGameStarter)
        {
            this._companionTemplates = new List<CharacterObject>(from x in CharacterObject.Templates
                                                                 where x.Occupation == Occupation.Wanderer && x.IsTOWTemplate()
                                                                 select x);
            this._nextRandomCompanionSpawnDate = CampaignTime.WeeksFromNow(this._randomCompanionSpawnFrequencyInWeeks);
            this.SpawnUrbanCharactersAtGameStart();
        }

        public void SetInitialRelationsBetweenNotablesAndLords()
        {
            foreach (Settlement settlement in Settlement.All)
            {
                for (int i = 0; i < settlement.Notables.Count; i++)
                {
                    Hero hero = settlement.Notables[i];
                    if (hero.IsNotable)
                    {
                        foreach (Hero hero2 in settlement.MapFaction.Lords)
                        {
                            if (hero2 != hero && hero2 == hero2.Clan.Leader && hero2.MapFaction == settlement.MapFaction)
                            {
                                float chanceOfConflict = (float)HeroHelper.NPCPersonalityClashWithNPC(hero, hero2) * 0.01f * 2.5f;
                                float num = MBRandom.RandomFloat;
                                float num2 = Campaign.MapDiagonal;
                                foreach (Settlement settlement2 in hero2.Clan.Settlements)
                                {
                                    float num3 = (settlement == settlement2) ? 0f : settlement2.Position2D.Distance(settlement.Position2D);
                                    if (num3 < num2)
                                    {
                                        num2 = num3;
                                    }
                                }
                                float num4 = (num2 < 100f) ? (1f - num2 / 100f) : 0f;
                                float num5 = num4 * MBRandom.RandomFloat + (1f - num4);
                                if (MBRandom.RandomFloat < 0.2f)
                                {
                                    num5 = 1f / (0.5f + 0.5f * num5);
                                }
                                num *= num5;
                                if (num > 1f)
                                {
                                    num = 1f;
                                }
                                this.DetermineRelation(hero, hero2, num, chanceOfConflict);
                            }
                        }
                        for (int j = i + 1; j < settlement.Notables.Count; j++)
                        {
                            Hero hero3 = settlement.Notables[j];
                            if (hero3.IsNotable)
                            {
                                float chanceOfConflict2 = (float)HeroHelper.NPCPersonalityClashWithNPC(hero, hero) * 0.01f * 2.5f;
                                float randomValue = MBRandom.RandomFloat;
                                if (hero.CharacterObject.Occupation == hero3.CharacterObject.Occupation)
                                {
                                    randomValue = 1f - 0.25f * MBRandom.RandomFloat;
                                }
                                this.DetermineRelation(hero, hero3, randomValue, chanceOfConflict2);
                            }
                        }
                    }
                }
            }
        }

        private void DetermineRelation(Hero hero1, Hero hero2, float randomValue, float chanceOfConflict)
        {
            float num = 0.3f;
            if (randomValue < num)
            {
                int num2 = (int)((num - randomValue) * (num - randomValue) / (num * num) * 100f);
                if (num2 > 0)
                {
                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero1, hero2, num2, true);
                    return;
                }
            }
            else if (randomValue > 1f - chanceOfConflict)
            {
                int num3 = -(int)((randomValue - (1f - chanceOfConflict)) * (randomValue - (1f - chanceOfConflict)) / (chanceOfConflict * chanceOfConflict) * 100f);
                if (num3 < 0)
                {
                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero1, hero2, num3, true);
                }
            }
        }

        public void OnNewGameCreatedPartialFollowUp(CampaignGameStarter starter, int i)
        {
            if (i == 1)
            {
                this.InitCompanions();
                this.SetInitialRelationsBetweenNotablesAndLords();
                int num = 50;
                for (int j = 0; j < num; j++)
                {
                    foreach (Hero hero in Hero.AllAliveHeroes)
                    {
                        if (hero.IsNotable)
                        {
                            this.UpdateNotableSupport(hero);
                        }
                    }
                }
            }
        }

        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
            this._companionTemplates = new List<CharacterObject>(from x in CharacterObject.Templates
                                                                 where x.Occupation == Occupation.Wanderer && x.IsTOWTemplate()
                                                                 select x);
            foreach (Hero hero in Hero.DeadOrDisabledHeroes.ToList<Hero>())
            {
                if ((hero.IsNotable || hero.IsWanderer) && hero.DeathDay.ElapsedDaysUntilNow >= 7f)
                {
                    Traverse.Create(Campaign.Current.CampaignObjectManager).Method("UnregisterDeadHero", hero);
                }
            }
        }

        public void CompanionAdded(Hero hero)
        {
            if (hero.CompanionOf != null && this._companions.Contains(hero))
            {
                this._companions.Remove(hero);
            }
        }

        public void WeeklyTick()
        {
            foreach (KeyValuePair<Settlement, CampaignTime> keyValuePair in new Dictionary<Settlement, CampaignTime>(this._companionSettlements))
            {
                if (keyValuePair.Value.ElapsedWeeksUntilNow > this._companionSpawnCooldownForSettlementInWeeks)
                {
                    this._companionSettlements.Remove(keyValuePair.Key);
                }
            }
            if (this._nextRandomCompanionSpawnDate.IsPast)
            {
                CharacterObject randomElementWithPredicate = this._companionTemplates.GetRandomElementWithPredicate((CharacterObject x) => !this._companions.Contains(x.HeroObject));
                this.CreateCompanion(randomElementWithPredicate ?? this._companionTemplates.GetRandomElement<CharacterObject>());
                this._nextRandomCompanionSpawnDate = CampaignTime.WeeksFromNow(this._randomCompanionSpawnFrequencyInWeeks);
            }
        }

        private void DailyTickSettlement(Settlement settlement)
        {
            if (this._settlementPassedDaysForWeeklyTick.ContainsKey(settlement))
            {
                Dictionary<Settlement, int> settlementPassedDaysForWeeklyTick = this._settlementPassedDaysForWeeklyTick;
                int num = settlementPassedDaysForWeeklyTick[settlement];
                settlementPassedDaysForWeeklyTick[settlement] = num + 1;
                if (this._settlementPassedDaysForWeeklyTick[settlement] == 7)
                {
                    this.SpawnNotablesIfNeeded(settlement);
                    this._settlementPassedDaysForWeeklyTick[settlement] = 0;
                    return;
                }
            }
            else
            {
                this._settlementPassedDaysForWeeklyTick.Add(settlement, 0);
            }
        }

        private void SpawnNotablesIfNeeded(Settlement settlement)
        {
            if (settlement.IsTown || settlement.IsVillage)
            {
                List<Occupation> list = new List<Occupation>();
                if (settlement.IsTown)
                {
                    list = new List<Occupation>
                    {
                        Occupation.GangLeader,
                        Occupation.Artisan,
                        Occupation.Merchant
                    };
                }
                else if (settlement.IsVillage)
                {
                    list = new List<Occupation>
                    {
                        Occupation.RuralNotable,
                        Occupation.Headman
                    };
                }
                float randomFloat = MBRandom.RandomFloat;
                int num = 0;
                foreach (Occupation occupation in list)
                {
                    num += Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement, occupation);
                }
                float num2 = settlement.Notables.Any<Hero>() ? ((float)(num - settlement.Notables.Count) / (float)num) : 1f;
                num2 *= (float)Math.Pow((double)num2, 0.36000001430511475);
                if (randomFloat <= num2)
                {
                    List<Occupation> list2 = new List<Occupation>();
                    foreach (Occupation occupation2 in list)
                    {
                        int num3 = 0;
                        using (List<Hero>.Enumerator enumerator2 = settlement.Notables.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                if (enumerator2.Current.CharacterObject.Occupation == occupation2)
                                {
                                    num3++;
                                }
                            }
                        }
                        int targetNotableCountForSettlement = Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement, occupation2);
                        if (num3 < targetNotableCountForSettlement)
                        {
                            list2.Add(occupation2);
                        }
                    }
                    if (list2.Count > 0)
                    {
                        Hero hero = HeroCreator.CreateHeroAtOccupation(list2.GetRandomElement<Occupation>(), settlement);
                        CheckNotable(settlement, hero);
                        EnterSettlementAction.ApplyForCharacterOnly(hero, settlement);
                    }
                }
            }
        }

        private void DailyTickHero(Hero hero)
        {
            if (hero.IsNotable && hero.CurrentSettlement != null)
            {
                if (MBRandom.RandomFloat < 0.01f)
                {
                    this.UpdateNotableRelations(hero);
                }
                this.UpdateNotableSupport(hero);
                this.BalanceGoldAndPowerOfNotable(hero);
                this.ManageCaravanExpensesOfNotable(hero);
                this.CheckAndMakeNotableDisappear(hero);
            }
        }

        private void UpdateNotableRelations(Hero notable)
        {
            foreach (Clan clan in Clan.All)
            {
                if (clan != Clan.PlayerClan && clan.Leader != null && !clan.IsEliminated)
                {
                    int relation = notable.GetRelation(clan.Leader);
                    if (relation > 0)
                    {
                        float num = (float)relation / 1000f;
                        if (MBRandom.RandomFloat < num)
                        {
                            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(notable, clan.Leader, -20, true);
                        }
                    }
                    else if (relation < 0)
                    {
                        float num2 = (float)(-(float)relation) / 1000f;
                        if (MBRandom.RandomFloat < num2)
                        {
                            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(notable, clan.Leader, 20, true);
                        }
                    }
                }
            }
        }

        private void UpdateNotableSupport(Hero notable)
        {
            if (notable.SupporterOf == null)
            {
                using (IEnumerator<Clan> enumerator = Clan.NonBanditFactions.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Clan clan = enumerator.Current;
                        if (clan.Leader != null)
                        {
                            int relation = notable.GetRelation(clan.Leader);
                            if (relation > 50)
                            {
                                float num = (float)(relation - 50) / 2000f;
                                if (MBRandom.RandomFloat < num)
                                {
                                    notable.SupporterOf = clan;
                                }
                            }
                        }
                    }
                    return;
                }
            }
            int relation2 = notable.GetRelation(notable.SupporterOf.Leader);
            if (relation2 < 0)
            {
                notable.SupporterOf = null;
                return;
            }
            if (relation2 < 50)
            {
                float num2 = (float)(50 - relation2) / 500f;
                if (MBRandom.RandomFloat < num2)
                {
                    notable.SupporterOf = null;
                }
            }
        }

        private void BalanceGoldAndPowerOfNotable(Hero notable)
        {
            if (notable.Gold > 10500)
            {
                int num = (notable.Gold - 10000) / 500;
                GiveGoldAction.ApplyBetweenCharacters(notable, null, num * 500, true);
                notable.AddPower((float)num);
                return;
            }
            if (notable.Gold < 4500 && notable.Power > 0f)
            {
                int num2 = (5000 - notable.Gold) / 500;
                GiveGoldAction.ApplyBetweenCharacters(null, notable, num2 * 500, true);
                notable.AddPower((float)(-(float)num2));
            }
        }

        private void ManageCaravanExpensesOfNotable(Hero notable)
        {
            for (int i = notable.OwnedCaravans.Count - 1; i >= 0; i--)
            {
                CaravanPartyComponent caravanPartyComponent = notable.OwnedCaravans[i];
                int totalWage = caravanPartyComponent.MobileParty.TotalWage;
                if (caravanPartyComponent.MobileParty.PartyTradeGold >= totalWage)
                {
                    caravanPartyComponent.MobileParty.PartyTradeGold -= totalWage;
                }
                else
                {
                    int num = Math.Min(totalWage, notable.Gold);
                    notable.Gold -= num;
                }
                if (caravanPartyComponent.MobileParty.PartyTradeGold < 5000)
                {
                    int num2 = Math.Min(5000 - caravanPartyComponent.MobileParty.PartyTradeGold, notable.Gold);
                    caravanPartyComponent.MobileParty.PartyTradeGold += num2;
                    notable.Gold -= num2;
                }
            }
        }

        private void CheckAndMakeNotableDisappear(Hero notable)
        {
            if (notable.OwnedWorkshops.IsEmpty<Workshop>() && notable.OwnedCaravans.IsEmpty<CaravanPartyComponent>() && notable.OwnedCommonAreas.IsEmpty<CommonAreaPartyComponent>() && notable.CanHaveQuestsOrIssues() && notable.Power < (float)Campaign.Current.Models.NotablePowerModel.NotableDisappearPowerLimit)
            {
                float randomFloat = MBRandom.RandomFloat;
                float notableDisappearProbability = this.GetNotableDisappearProbability(notable);
                if (randomFloat < notableDisappearProbability)
                {
                    KillCharacterAction.ApplyByRemove(notable, false);
                    IssueBase issue = notable.Issue;
                    if (issue == null)
                    {
                        return;
                    }
                    issue.CompleteIssueWithAiLord(notable.CurrentSettlement.OwnerClan.Leader);
                }
            }
        }

        private float GetNotableDisappearProbability(Hero hero)
        {
            return ((float)Campaign.Current.Models.NotablePowerModel.NotableDisappearPowerLimit - hero.Power) / (float)Campaign.Current.Models.NotablePowerModel.NotableDisappearPowerLimit * 0.02f;
        }

        public void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
        {
            if (mobileParty == MobileParty.MainParty && settlement.IsTown && !this._companionSettlements.ContainsKey(settlement) && this._companions.Count > 0)
            {
                Hero wanderer = this._companions.GetRandomElementWithPredicate((Hero h) => h.IsSuitableForSettlement(settlement));
                wanderer.ChangeState(Hero.CharacterStates.Active);
                EnterSettlementAction.ApplyForCharacterOnly(wanderer, settlement);
                this._companionSettlements.Add(settlement, CampaignTime.Now);
                this._companions.Remove(wanderer);
            }
            //if (settlement.IsSuitableForHero(hero))
            //{
            //    PurgeSettlement(mobileParty, settlement, hero);
            //}
        }

        //NEED TO CHECK THE CODE
        private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification)
        {
            if (victim.IsNotable)
            {
                if (victim.Power >= (float)Campaign.Current.Models.NotablePowerModel.NotableDisappearPowerLimit)
                {
                    Hero hero = HeroCreator.CreateRelativeNotableHero(victim);
                    if (victim.CurrentSettlement != null)
                    {
                        this.ChangeDeadNotable(victim, hero, victim.CurrentSettlement);
                    }

                    CheckNotable(hero.CurrentSettlement, hero);

                    using (List<CaravanPartyComponent>.Enumerator enumerator = victim.OwnedCaravans.ToList<CaravanPartyComponent>().GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            CaravanPartyComponent caravanPartyComponent = enumerator.Current;
                            CaravanPartyComponent.TransferCaravanOwnership(caravanPartyComponent.MobileParty, hero);
                        }
                        goto IL_C3;
                    }
                }
                foreach (CaravanPartyComponent caravanPartyComponent2 in victim.OwnedCaravans.ToList<CaravanPartyComponent>())
                {
                    DestroyPartyAction.Apply(null, caravanPartyComponent2.MobileParty);
                }
            }
        IL_C3:
            if (this._companions.Contains(victim))
            {
                this._companions.Remove(victim);
            }
        }

        private void ChangeDeadNotable(Hero deadNotable, Hero newNotable, Settlement notableSettlement)
        {
            EnterSettlementAction.ApplyForCharacterOnly(newNotable, notableSettlement);
            foreach (Hero otherHero in Hero.AllAliveHeroes)
            {
                int relation = deadNotable.GetRelation(otherHero);
                if (relation != 0)
                {
                    newNotable.SetPersonalRelation(otherHero, relation);
                }
            }
            if (deadNotable.Issue != null)
            {
                Campaign.Current.IssueManager.ChangeIssueOwner(deadNotable.Issue, newNotable);
            }
        }

        public void InitCompanions()
        {
            this._companions.Clear();
            this._companionSettlements.Clear();
            foreach (Hero hero in Hero.AllAliveHeroes)
            {
                if (hero.CanBeCompanion && !hero.CharacterObject.IsTOWTemplate())
                {
                    this._companions.Add(hero);
                }
            }
        }

        private void AdjustEquipment(Hero hero)
        {
            this.AdjustEquipmentImp(hero.BattleEquipment);
            this.AdjustEquipmentImp(hero.CivilianEquipment);
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
                        equipment[equipmentIndex] = new EquipmentElement(equipmentElement.Item, @object, null);
                    }
                    else if (equipmentElement.Item.HorseComponent != null)
                    {
                        equipment[equipmentIndex] = new EquipmentElement(equipmentElement.Item, object3, null);
                    }
                    else if (equipmentElement.Item.WeaponComponent != null)
                    {
                        equipment[equipmentIndex] = new EquipmentElement(equipmentElement.Item, object2, null);
                    }
                }
            }
        }

        private void SpawnUrbanCharactersAtGameStart()
        {
            foreach (Settlement settlement in Campaign.Current.Settlements)
            {
                if (settlement.Notables.Count == 0)
                {
                    if (settlement.IsTown)
                    {
                        CreateHeroes(Occupation.Artisan, settlement);
                        CreateHeroes(Occupation.Merchant, settlement);
                        CreateHeroes(Occupation.GangLeader, settlement);
                    }
                    else if (settlement.IsVillage)
                    {
                        CreateHeroes(Occupation.Headman, settlement);
                        CreateHeroes(Occupation.RuralNotable, settlement);
                    }
                }
            }

            int count = this._companionTemplates.Count;
            float num = MathF.Clamp(25f / (float)count, 0.33f, 1f);
            foreach (CharacterObject companionTemplate in this._companionTemplates)
            {
                if (MBRandom.RandomFloat < num)
                {
                    this.CreateCompanion(companionTemplate);
                }
            }
            this._companions.Shuffle<Hero>();
        }

        private void CreateHeroes(Occupation occupation, Settlement settlement)
        {
            int targetNotableCountForSettlement = Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement, occupation);
            for (int i = 0; i < targetNotableCountForSettlement; i++)
            {
                Hero hero = HeroCreator.CreateHeroAtOccupation(occupation, settlement);
                CheckNotable(settlement, hero);
            }
        }

        private static void CheckNotable(Settlement settlement, Hero hero)
        {
            if (settlement.IsEmpireSettlement() && hero.IsVampireNotable())
            {
                hero.TurnIntoHuman();
            }
            else if (settlement.IsVampireSettlement() && hero.IsEmpireNotable())
            {
                hero.TurnIntoVampire();
            }
        }

        private void CreateCompanion(CharacterObject companionTemplate)
        {
            if (companionTemplate == null)
            {
                return;
            }
            Town randomElementWithPredicate = Town.AllTowns.GetRandomElementWithPredicate((Town settlement) => settlement.Settlement.IsSuitableForHero(companionTemplate));
            Settlement settlement2 = (randomElementWithPredicate != null) ? randomElementWithPredicate.Settlement : null;
            if (settlement2 != null)
            {
                List<Settlement> list = new List<Settlement>();
                foreach (Village village in Village.All)
                {
                    if (Campaign.Current.Models.MapDistanceModel.GetDistance(village.Settlement, settlement2) < 30f)
                    {
                        list.Add(village.Settlement);
                    }
                }
                settlement2 = ((list.Count > 0) ? list.GetRandomElement<Settlement>().Village.Bound : settlement2);

                Hero hero = HeroCreator.CreateSpecialHero(companionTemplate, settlement2, null, null, Campaign.Current.Models.AgeModel.HeroComesOfAge + 5 + MBRandom.RandomInt(27));
                var attributes = companionTemplate.GetAttributes();
                foreach (var attribute in attributes)
                {
                    hero.AddAttribute(attribute);
                }
                var abilities = companionTemplate.GetAbilities();
                foreach (var ability in abilities)
                {
                    hero.AddAbility(ability);
                }
                this.AdjustEquipment(hero);
                this._companions.Add(hero);
            }
        }

        public void SpecialCharacterActions()
        {
            foreach (Settlement settlement in Campaign.Current.Settlements)
            {
                if (settlement.IsTown || settlement.IsVillage)
                {
                    List<CharacterObject> list = new List<CharacterObject>();
                    using (List<Hero>.Enumerator enumerator2 = settlement.HeroesWithoutParty.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            Hero specialCharacter = enumerator2.Current;
                            bool flag = false;
                            float randomFloat = MBRandom.RandomFloat;
                            foreach (Hero hero in settlement.HeroesWithoutParty)
                            {
                                if (specialCharacter != hero && !specialCharacter.AwaitingTrial && !hero.AwaitingTrial && specialCharacter.Template == hero.Template && specialCharacter.SpcDaysInLocation <= hero.SpcDaysInLocation)
                                {
                                    flag = true;
                                }
                            }
                            if (settlement.IsTown && specialCharacter.IsPreacher && specialCharacter.GetTraitLevel(DefaultTraits.Generosity) < 1)
                            {
                                flag = true;
                            }
                            if (specialCharacter.IsMerchant && specialCharacter.Power < 60f && settlement.IsTown && settlement.Town.Workshops.All((Workshop x) => x.Owner != specialCharacter) && randomFloat < specialCharacter.Power / 500f)
                            {
                                flag = true;
                            }
                            if (specialCharacter.IsArtisan || specialCharacter.IsRuralNotable)
                            {
                                flag = false;
                            }
                            if (flag)
                            {
                                list.Add(specialCharacter.CharacterObject);
                            }
                        }
                    }
                    foreach (CharacterObject character in list)
                    {
                        this.MoveSpecialCharacter(character, settlement);
                    }
                }
            }
            foreach (Settlement settlement2 in Campaign.Current.Settlements)
            {
                if (settlement2.IsTown)
                {
                    foreach (Hero hero2 in settlement2.HeroesWithoutParty)
                    {
                        hero2.SpcDaysInLocation++;
                    }
                }
            }
        }

        public bool MoveSpecialCharacter(CharacterObject character, Settlement startPoint)
        {
            bool result = false;
            Settlement settlement = null;
            float num = 9999f;
            foreach (Settlement settlement2 in Campaign.Current.Settlements)
            {
                if (settlement2.IsTown && settlement2 != startPoint && settlement2.Culture == character.Culture)
                {
                    float num2 = 10000f;
                    float num3;
                    if (Campaign.Current.Models.MapDistanceModel.GetDistance(settlement2, startPoint, 60f, out num3))
                    {
                        num2 = (num3 + 10f) * (MBRandom.RandomFloat + 0.1f);
                    }
                    if (num2 < num)
                    {
                        settlement = settlement2;
                        num = num2;
                    }
                }
            }
            if (settlement != null)
            {
                this.CharacterChangeLocation(character.HeroObject, startPoint, settlement);
                result = true;
            }
            return result;
        }

        public void CharacterChangeLocation(Hero hero, Settlement startLocation, Settlement endLocation)
        {
            if (startLocation != null)
            {
                LeaveSettlementAction.ApplyForCharacterOnly(hero);
            }
            EnterSettlementAction.ApplyForCharacterOnly(hero, endLocation);
            if (hero != null)
            {
                hero.SpcDaysInLocation = 0;
            }
        }

        private void PurgeSettlement(MobileParty mobileParty, Settlement settlement, Hero hero)
        {
            if (mobileParty != null)
            {
                if (settlement.Notables.Count > 0)
                {
                    for (int i = 0; i < settlement.Notables.Count; i++)
                    {
                        if (settlement.Notables[i] != null)
                        {
                            if (settlement.IsVampireSettlement())
                            {
                                settlement.Notables[i].TurnIntoVampire();
                            }
                            else
                            {
                                if (settlement.Notables[i].IsVampireNotable())
                                {
                                    KillCharacterAction.ApplyByExecution(settlement.Notables[i], hero, true);
                                }
                            }
                        }
                    }
                }
                if (settlement.HeroesWithoutParty.Count > 0)
                {
                    for (int j = 0; j < settlement.HeroesWithoutParty.Count; j++)
                    {
                        if (settlement.HeroesWithoutParty[j] != null)
                        {
                            if (!settlement.IsSuitableForHero(settlement.HeroesWithoutParty[j]))
                            {
                                LeaveSettlementAction.ApplyForCharacterOnly(settlement.HeroesWithoutParty[j]);
                                Settlement newSettlement = Settlement.All.GetRandomElementWithPredicate(s => s.IsTown && s.IsSuitableForHero(settlement.HeroesWithoutParty[j]));
                                EnterSettlementAction.ApplyForCharacterOnly(settlement.HeroesWithoutParty[j], newSettlement);
                            }
                        }
                    }
                }
                if (settlement.MapFaction.Name.Contains("Sylvania"))
                {
                    TOWCommon.Say($"{hero.Name} purged {settlement.Name} of undead");
                }
                else
                {
                    TOWCommon.Say($"{hero.Name} purged {settlement.Name} of humans");
                }
            }
        }


        private const int GoldLimitForNotablesToStartGainingPower = 10000;

        private const int GoldLimitForNotablesToStartLosingPower = 5000;

        private const int GoldNeededToGainOnePower = 500;

        private const int CaravanGoldLowLimit = 5000;

        private const int RemoveUrbanCharacterAfterDays = 7;

        private Dictionary<Settlement, CampaignTime> _companionSettlements;

        private Dictionary<Settlement, int> _settlementPassedDaysForWeeklyTick;

        private List<Hero> _companions;

        private List<CharacterObject> _companionTemplates;

        private CampaignTime _nextRandomCompanionSpawnDate;

        private float _randomCompanionSpawnFrequencyInWeeks = 6f;

        private float _companionSpawnCooldownForSettlementInWeeks = 6f;

        private const float NotableSpawnChance = 0.8f;

        private const float TargetCompanionNumber = 25f;
    }
}
