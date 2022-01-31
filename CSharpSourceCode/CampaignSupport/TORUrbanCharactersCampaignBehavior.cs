using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.ObjectSystem;
using TOW_Core.CampaignSupport.Assimilation;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport
{
    public class TORUrbanCharactersCampaignBehavior : CampaignBehaviorBase
    {
        public TORUrbanCharactersCampaignBehavior()
        {
            _companionSettlements = new Dictionary<Settlement, CampaignTime>();
            _settlementPassedDaysForWeeklyTick = new Dictionary<Settlement, int>();
            _companions = new List<Hero>();
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedPartialFollowUpEvent.AddNonSerializedListener(this, OnNewGameCreatedPartialFollowUp);
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, WeeklyTick);
            CampaignEvents.NewCompanionAdded.AddNonSerializedListener(this, CompanionAdded);
            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.OnGameEarlyLoadedEvent.AddNonSerializedListener(this, OnGameEarlyLoaded);
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, DailyTickHero);
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, DailyTickSettlement);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("companionSettlements", ref _companionSettlements);
            dataStore.SyncData("_settlementPassedDaysForWeeklyTick", ref _settlementPassedDaysForWeeklyTick);
            dataStore.SyncData("companions", ref _companions);
        }


        public void OnNewGameCreated(CampaignGameStarter campaignGameStarter)
        {
            InitializeWandererList();
            InitCompanions();
            _nextRandomCompanionSpawnDate = CampaignTime.WeeksFromNow(_randomCompanionSpawnFrequencyInWeeks);
            SpawnUrbanCharactersAtGameStart();
        }

        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
            InitializeWandererList();
            foreach (Hero hero in Hero.DeadOrDisabledHeroes.ToList())
            {
                if (hero.IsDead && ((hero.IsNotable && hero.DeathDay.ElapsedDaysUntilNow >= 7f) || (hero.IsWanderer && hero.DeathDay.ElapsedDaysUntilNow >= 70f)))
                {
                    Traverse.Create(Campaign.Current.CampaignObjectManager).Method("UnregisterDeadHero", hero);
                }
            }
        }

        private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification)
        {
            if (victim.IsNotable)
            {
                var comp = victim.CurrentSettlement.GetComponent<AssimilationComponent>();
                if (victim.Power >= (float)Campaign.Current.Models.NotablePowerModel.NotableDisappearPowerLimit &&
                                    comp != null &&
                                    comp.IsAssimilationComplete)
                {
                    Hero hero = HeroCreator.CreateRelativeNotableHero(victim);
                    if (victim.CurrentSettlement != null)
                    {
                        ChangeDeadNotable(victim, hero, victim.CurrentSettlement);
                    }
                    CheckNotable(hero.CurrentSettlement, hero);

                    foreach (CaravanPartyComponent item in victim.OwnedCaravans.ToList())
                    {
                        CaravanPartyComponent.TransferCaravanOwnership(item.MobileParty, hero, hero.CurrentSettlement);
                    }
                }
                else
                {
                    foreach (CaravanPartyComponent item2 in victim.OwnedCaravans.ToList())
                    {
                        DestroyPartyAction.Apply(null, item2.MobileParty);
                    }
                }
            }

            if (_companions.Contains(victim))
            {
                _companions.Remove(victim);
            }
        }

        public void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
        {
            if (mobileParty != MobileParty.MainParty || !settlement.IsTown || _companionSettlements.ContainsKey(settlement) || _companions.Count <= 0)
            {
                return;
            }

            Hero wanderer = _companions.GetRandomElementWithPredicate((Hero h) => h.IsSuitableForSettlement(settlement));
            if(wanderer != null)
            {
                wanderer.ChangeState(Hero.CharacterStates.Active);
                EnterSettlementAction.ApplyForCharacterOnly(wanderer, settlement);
                _companionSettlements.Add(settlement, CampaignTime.Now);
                _companions.Remove(wanderer);
            }
        }

        private void OnGameEarlyLoaded(CampaignGameStarter obj)
        {
            foreach (Hero allAliveHero in Hero.AllAliveHeroes)
            {
                if (allAliveHero.IsNotable && allAliveHero.CurrentSettlement == null && allAliveHero.PartyBelongedTo == null && allAliveHero.PartyBelongedToAsPrisoner == null && allAliveHero.IsActive && allAliveHero.HomeSettlement != null && allAliveHero.CanHaveQuestsOrIssues())
                {
                    EnterSettlementAction.ApplyForCharacterOnly(allAliveHero, allAliveHero.HomeSettlement);
                }
            }
        }

        public void OnNewGameCreatedPartialFollowUp(CampaignGameStarter starter, int i)
        {
            if (i != 1)
            {
                return;
            }

            SetInitialRelationsBetweenNotablesAndLords();
            int num = 50;
            for (int j = 0; j < num; j++)
            {
                foreach (Hero allAliveHero in Hero.AllAliveHeroes)
                {
                    if (allAliveHero.IsNotable)
                    {
                        UpdateNotableSupport(allAliveHero);
                    }
                }
            }
        }

        public void WeeklyTick()
        {
            foreach (KeyValuePair<Settlement, CampaignTime> item in new Dictionary<Settlement, CampaignTime>(_companionSettlements))
            {
                if (item.Value.ElapsedWeeksUntilNow > _companionSpawnCooldownForSettlementInWeeks)
                {
                    _companionSettlements.Remove(item.Key);
                }
            }

            if (_nextRandomCompanionSpawnDate.IsPast)
            {
                CharacterObject randomElementWithPredicate = _companionTemplates.GetRandomElementWithPredicate((CharacterObject x) => !_companions.Contains(x.HeroObject));
                CreateCompanion(randomElementWithPredicate ?? _companionTemplates.GetRandomElement());
                _nextRandomCompanionSpawnDate = CampaignTime.WeeksFromNow(_randomCompanionSpawnFrequencyInWeeks);
            }
        }

        private void DailyTickSettlement(Settlement settlement)
        {
            if (_settlementPassedDaysForWeeklyTick.ContainsKey(settlement))
            {
                _settlementPassedDaysForWeeklyTick[settlement]++;
                if (_settlementPassedDaysForWeeklyTick[settlement] == 7)
                {
                    SpawnNotablesIfNeeded(settlement);
                    _settlementPassedDaysForWeeklyTick[settlement] = 0;
                }
            }
            else
            {
                _settlementPassedDaysForWeeklyTick.Add(settlement, 0);
            }
        }

        private void DailyTickHero(Hero hero)
        {
            if (hero.IsNotable && hero.CurrentSettlement != null)
            {
                if (MBRandom.RandomFloat < 0.01f)
                {
                    UpdateNotableRelations(hero);
                }

                UpdateNotableSupport(hero);
                BalanceGoldAndPowerOfNotable(hero);
                ManageCaravanExpensesOfNotable(hero);
                CheckAndMakeNotableDisappear(hero);
            }
        }

        public void CompanionAdded(Hero hero)
        {
            if (hero.CompanionOf != null && _companions.Contains(hero))
            {
                _companions.Remove(hero);
            }
        }


        private void InitializeWandererList()
        {
            _companionTemplates = new List<CharacterObject>();
            foreach (CultureObject objectType in MBObjectManager.Instance.GetObjectTypeList<CultureObject>())
            {
                if (objectType.IsMainCulture)
                {
                    _companionTemplates.AddRange(objectType.NotableAndWandererTemplates.WhereQ((CharacterObject x) => x.Occupation == Occupation.Wanderer && x.IsTOWTemplate()));
                }
            }
        }

        private void SpawnUrbanCharactersAtGameStart()
        {
            foreach (Settlement settlement in Settlement.All)
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

            int count = _companionTemplates.Count;
            foreach (CharacterObject companionTemplate in _companionTemplates)
            {
                CreateCompanion(companionTemplate);
            }
            _companions.Shuffle();
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
                float num = 0;
                foreach (Occupation occupation in _townOccupations)
                {
                    num += Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement, occupation);
                }
                float randomFloat = MBRandom.RandomFloat;
                float num2 = settlement.Notables.Any<Hero>() ? ((num - settlement.Notables.Count) / num) : 1f;
                num2 *= MathF.Pow(num2, 0.36f);
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

        private void CreateHeroes(Occupation occupation, Settlement settlement)
        {
            Hero hero = HeroCreator.CreateHeroAtOccupation(occupation, settlement);
            CheckNotable(settlement, hero);
            //int targetNotableCountForSettlement = Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement, occupation);
            //else
            //{
            //    for (int i = 0; i < targetNotableCountForSettlement; i++)
            //    {
            //        Hero hero = HeroCreator.CreateHeroAtOccupation(occupation, settlement);
            //        CheckNotable(settlement, hero);
            //    }
            //}
        }

        private void CheckNotable(Settlement settlement, Hero hero)
        {
            if (settlement.IsEmpireSettlement())
            {
                hero.TurnIntoHuman();
            }
            else if (settlement.IsVampireSettlement())
            {
                hero.TurnIntoVampire();
            }
        }

        public void InitCompanions()
        {
            _companions.Clear();
            _companionSettlements.Clear();
            foreach (Hero aliveHero in Hero.AllAliveHeroes)
            {
                if (aliveHero.CanBeCompanion && !aliveHero.IsTemplate && !aliveHero.CharacterObject.IsTOWTemplate())
                {
                    _companions.Add(aliveHero);
                }
            }
        }

        private void CreateCompanion(CharacterObject companionTemplate)
        {
            if (companionTemplate == null)
            {
                Utilities.TOWCommon.Log("Create companion failed. Companion template is null.", NLog.LogLevel.Error);
                return;
            }

            Settlement settlement2 = Town.AllTowns.GetRandomElementWithPredicate((Town settlement) => settlement.Settlement.IsSuitableForHero(companionTemplate))?.Settlement;
            if (settlement2 != null)
            {
                List<Settlement> list = new List<Settlement>();

                if (_allVillages == null)
                {
                    _allVillages = Traverse.Create(Campaign.Current).Field("_villages").GetValue<List<Village>>();
                }

                for (int i = 0; i < _allVillages.Count; i++)
                {
                    Village village = _allVillages[i];
                    if (Campaign.Current.Models.MapDistanceModel.GetDistance(village.Settlement, settlement2) < 30f)
                    {
                        list.Add(village.Settlement);
                    }
                }

                settlement2 = ((list.Count > 0) ? list.GetRandomElement().Village.Bound : settlement2);
            }
            else
            {
                settlement2 = Town.AllTowns.GetRandomElement().Settlement;
            }

            Hero hero = HeroCreator.CreateSpecialHero(companionTemplate, settlement2, null, null, Campaign.Current.Models.AgeModel.HeroComesOfAge + 5 + MBRandom.RandomInt(27));
            foreach(var attribute in companionTemplate.GetAttributes())
            {
                if(!hero.HasAttribute(attribute)) hero.AddAttribute(attribute);
            }
            foreach(var ability in companionTemplate.GetAbilities())
            {
                if(!hero.HasAbility(ability)) hero.AddAbility(ability);
            }
            if(companionTemplate.IsVampire() && !hero.IsVampire())
            {
                hero.AddAttribute("VampireBodyOverride");
            }
            AdjustEquipment(hero);
            _companions.Add(hero);
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
                    if (Campaign.Current.Models.MapDistanceModel.GetDistance(settlement2, startPoint, 60f, out float distance))
                    {
                        num2 = (distance + 10f) * (MBRandom.RandomFloat + 0.1f);
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
                CharacterChangeLocation(character.HeroObject, startPoint, settlement);
                result = true;
            }

            return result;
        }

        private void DetermineRelation(Hero hero1, Hero hero2, float randomValue, float chanceOfConflict)
        {
            float num = 0.3f;
            if (randomValue < num)
            {
                int num2 = (int)((num - randomValue) * (num - randomValue) / (num * num) * 100f);
                if (num2 > 0)
                {
                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero1, hero2, num2);
                }
            }
            else if (randomValue > 1f - chanceOfConflict)
            {
                int num3 = -(int)((randomValue - (1f - chanceOfConflict)) * (randomValue - (1f - chanceOfConflict)) / (chanceOfConflict * chanceOfConflict) * 100f);
                if (num3 < 0)
                {
                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero1, hero2, num3);
                }
            }
        }

        public void SetInitialRelationsBetweenNotablesAndLords()
        {
            foreach (Settlement item in Settlement.All)
            {
                for (int i = 0; i < item.Notables.Count; i++)
                {
                    Hero hero = item.Notables[i];
                    if (!hero.IsNotable)
                    {
                        continue;
                    }

                    foreach (Hero lord in item.MapFaction.Lords)
                    {
                        if (lord == hero || lord != lord.Clan.Leader || lord.MapFaction != item.MapFaction)
                        {
                            continue;
                        }

                        float chanceOfConflict = (float)HeroHelper.NPCPersonalityClashWithNPC(hero, lord) * 0.01f * 2.5f;
                        float randomFloat = MBRandom.RandomFloat;
                        float num = Campaign.MapDiagonal;
                        foreach (Settlement settlement in lord.Clan.Settlements)
                        {
                            float num2 = (item == settlement) ? 0f : settlement.Position2D.Distance(item.Position2D);
                            if (num2 < num)
                            {
                                num = num2;
                            }
                        }

                        float num3 = (num < 100f) ? (1f - num / 100f) : 0f;
                        float num4 = num3 * MBRandom.RandomFloat + (1f - num3);
                        if (MBRandom.RandomFloat < 0.2f)
                        {
                            num4 = 1f / (0.5f + 0.5f * num4);
                        }

                        randomFloat *= num4;
                        if (randomFloat > 1f)
                        {
                            randomFloat = 1f;
                        }

                        DetermineRelation(hero, lord, randomFloat, chanceOfConflict);
                    }

                    for (int j = i + 1; j < item.Notables.Count; j++)
                    {
                        Hero hero2 = item.Notables[j];
                        if (hero2.IsNotable)
                        {
                            float chanceOfConflict2 = (float)HeroHelper.NPCPersonalityClashWithNPC(hero, hero) * 0.01f * 2.5f;
                            float randomValue = MBRandom.RandomFloat;
                            if (hero.CharacterObject.Occupation == hero2.CharacterObject.Occupation)
                            {
                                randomValue = 1f - 0.25f * MBRandom.RandomFloat;
                            }

                            DetermineRelation(hero, hero2, randomValue, chanceOfConflict2);
                        }
                    }
                }
            }
        }

        private void UpdateNotableRelations(Hero notable)
        {
            foreach (Clan item in Clan.All)
            {
                if (item == Clan.PlayerClan || item.Leader == null || item.IsEliminated)
                {
                    continue;
                }

                int relation = notable.GetRelation(item.Leader);
                if (relation > 0)
                {
                    float num = (float)relation / 1000f;
                    if (MBRandom.RandomFloat < num)
                    {
                        ChangeRelationAction.ApplyRelationChangeBetweenHeroes(notable, item.Leader, -20);
                    }
                }
                else if (relation < 0)
                {
                    float num2 = (float)(-relation) / 1000f;
                    if (MBRandom.RandomFloat < num2)
                    {
                        ChangeRelationAction.ApplyRelationChangeBetweenHeroes(notable, item.Leader, 20);
                    }
                }
            }
        }

        private void UpdateNotableSupport(Hero notable)
        {
            if (notable.SupporterOf == null)
            {
                foreach (Clan nonBanditFaction in Clan.NonBanditFactions)
                {
                    if (nonBanditFaction.Leader == null)
                    {
                        continue;
                    }

                    int relation = notable.GetRelation(nonBanditFaction.Leader);
                    if (relation > 50)
                    {
                        float num = (float)(relation - 50) / 2000f;
                        if (MBRandom.RandomFloat < num)
                        {
                            notable.SupporterOf = nonBanditFaction;
                        }
                    }
                }

                return;
            }

            int relation2 = notable.GetRelation(notable.SupporterOf.Leader);
            if (relation2 < 0)
            {
                notable.SupporterOf = null;
            }
            else if (relation2 < 50)
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
                GiveGoldAction.ApplyBetweenCharacters(notable, null, num * 500, disableNotification: true);
                notable.AddPower(num);
            }
            else if (notable.Gold < 4500 && notable.Power > 0f)
            {
                int num2 = (5000 - notable.Gold) / 500;
                GiveGoldAction.ApplyBetweenCharacters(null, notable, num2 * 500, disableNotification: true);
                notable.AddPower(-num2);
            }
        }

        private void CheckAndMakeNotableDisappear(Hero notable)
        {
            if (notable.OwnedWorkshops.IsEmpty() && notable.OwnedCaravans.IsEmpty() && notable.OwnedCommonAreas.IsEmpty() && notable.CanDie(KillCharacterAction.KillCharacterActionDetail.Lost) && notable.CanHaveQuestsOrIssues() && notable.Power < (float)Campaign.Current.Models.NotablePowerModel.NotableDisappearPowerLimit)
            {
                float randomFloat = MBRandom.RandomFloat;
                float notableDisappearProbability = GetNotableDisappearProbability(notable);
                if (randomFloat < notableDisappearProbability)
                {
                    KillCharacterAction.ApplyByRemove(notable);
                    notable.Issue?.CompleteIssueWithAiLord(notable.CurrentSettlement.OwnerClan.Leader);
                }
            }
        }

        private void ManageCaravanExpensesOfNotable(Hero notable)
        {
            for (int num = notable.OwnedCaravans.Count - 1; num >= 0; num--)
            {
                CaravanPartyComponent caravanPartyComponent = notable.OwnedCaravans[num];
                int totalWage = caravanPartyComponent.MobileParty.TotalWage;
                if (caravanPartyComponent.MobileParty.PartyTradeGold >= totalWage)
                {
                    caravanPartyComponent.MobileParty.PartyTradeGold -= totalWage;
                }
                else
                {
                    int num2 = MathF.Min(totalWage, notable.Gold);
                    notable.Gold -= num2;
                }

                if (caravanPartyComponent.MobileParty.PartyTradeGold < 5000)
                {
                    int num3 = MathF.Min(5000 - caravanPartyComponent.MobileParty.PartyTradeGold, notable.Gold);
                    caravanPartyComponent.MobileParty.PartyTradeGold += num3;
                    notable.Gold -= num3;
                }
            }
        }

        private float GetNotableDisappearProbability(Hero hero)
        {
            return ((float)Campaign.Current.Models.NotablePowerModel.NotableDisappearPowerLimit - hero.Power) / (float)Campaign.Current.Models.NotablePowerModel.NotableDisappearPowerLimit * 0.02f;
        }

        private void ChangeDeadNotable(Hero deadNotable, Hero newNotable, Settlement notableSettlement)
        {
            EnterSettlementAction.ApplyForCharacterOnly(newNotable, notableSettlement);
            foreach (Hero allAliveHero in Hero.AllAliveHeroes)
            {
                int relation = deadNotable.GetRelation(allAliveHero);
                if (relation != 0)
                {
                    newNotable.SetPersonalRelation(allAliveHero, relation);
                }
            }

            if (deadNotable.Issue != null)
            {
                Campaign.Current.IssueManager.ChangeIssueOwner(deadNotable.Issue, newNotable);
            }
        }

        private void AdjustEquipment(Hero hero)
        {
            AdjustEquipmentImp(hero.BattleEquipment);
            AdjustEquipmentImp(hero.CivilianEquipment);
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
                        equipment[equipmentIndex] = new EquipmentElement(equipmentElement.Item, @object);
                    }
                    else if (equipmentElement.Item.HorseComponent != null)
                    {
                        equipment[equipmentIndex] = new EquipmentElement(equipmentElement.Item, object3);
                    }
                    else if (equipmentElement.Item.WeaponComponent != null)
                    {
                        equipment[equipmentIndex] = new EquipmentElement(equipmentElement.Item, object2);
                    }
                }
            }
        }

        public void SpecialCharacterActions()
        {
            foreach (Settlement settlement in Campaign.Current.Settlements)
            {
                if (!settlement.IsTown && !settlement.IsVillage)
                {
                    continue;
                }

                List<CharacterObject> list = new List<CharacterObject>();
                foreach (Hero specialCharacter in settlement.HeroesWithoutParty)
                {
                    bool flag = false;
                    float randomFloat = MBRandom.RandomFloat;
                    foreach (Hero item in settlement.HeroesWithoutParty)
                    {
                        if (specialCharacter != item && !specialCharacter.AwaitingTrial && !item.AwaitingTrial && specialCharacter.Template == item.Template && specialCharacter.SpcDaysInLocation <= item.SpcDaysInLocation)
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

                foreach (CharacterObject item2 in list)
                {
                    MoveSpecialCharacter(item2, settlement);
                }
            }

            foreach (Settlement settlement2 in Campaign.Current.Settlements)
            {
                if (!settlement2.IsTown)
                {
                    continue;
                }

                foreach (Hero item3 in settlement2.HeroesWithoutParty)
                {
                    item3.SpcDaysInLocation++;
                }
            }
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



        private List<Village> _allVillages;
        private readonly List<Occupation> _townOccupations = new List<Occupation>() { Occupation.GangLeader, Occupation.Artisan, Occupation.Merchant };
        private readonly List<Occupation> _villageOccupations = new List<Occupation>() { Occupation.RuralNotable, Occupation.Headman };
        private const int GoldLimitForNotablesToStartGainingPower = 10000;
        private const int GoldLimitForNotablesToStartLosingPower = 5000;
        private const int GoldNeededToGainOnePower = 500;
        private const int CaravanGoldLowLimit = 5000;
        private const int RemoveNotableCharacterAfterDays = 7;
        private const int RemoveWandererCharacterAfterDays = 70;
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