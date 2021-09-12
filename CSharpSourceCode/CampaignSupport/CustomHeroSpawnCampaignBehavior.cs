using Helpers;
using MountAndBlade.CampaignBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport
{
    public class CustomHeroSpawnCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnNewGameCreated));
            CampaignEvents.OnNewGameCreatedPartialFollowUpEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter, int>(this.OnNewGameCreatedPartialFollowUp));
            CampaignEvents.OnNewGameCreatedPartialFollowUpEndEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnNewGameCreatedPartialFollowUpEnd));
            CampaignEvents.OnGovernorChangedEvent.AddNonSerializedListener(this, new Action<Town, Hero, Hero>(this.OnGovernorChanged));
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, new Action<Clan>(this.OnNonBanditClanDailyTick));
            CampaignEvents.HeroComesOfAgeEvent.AddNonSerializedListener(this, new Action<Hero>(this.OnHeroComesOfAge));
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, new Action<Hero>(this.OnHeroDailyTick));
            CampaignEvents.CompanionRemoved.AddNonSerializedListener(this, new Action<Hero>(this.OnCompanionRemoved));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnGameLoaded));
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, new Action(this.WeeklyTick));
        }

        private void OnNewGameCreatedPartialFollowUp(CampaignGameStarter starter, int i)
        {
            if (i == 0)
            {
                MBReadOnlyList<CharacterObject> objectTypeList = (MBReadOnlyList<CharacterObject>)Game.Current.ObjectManager.GetObjectTypeList<CharacterObject>().Where(c => c.IsTOWTemplate());
                IHeroCreationCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<IHeroCreationCampaignBehavior>();
                if (campaignBehavior != null)
                {
                    foreach (CharacterObject characterObject in objectTypeList)
                    {
                        if (characterObject != CharacterObject.PlayerCharacter)
                        {
                            if (characterObject.HeroObject != null && characterObject.HeroObject.IsMinorFactionHero)
                            {
                                campaignBehavior.DeriveSkillsFromTraits(characterObject.HeroObject, characterObject.TemplateCharacter);
                            }
                            else if (characterObject.IsHero && characterObject.Age > (float)Campaign.Current.Models.AgeModel.HeroComesOfAge)
                            {
                                campaignBehavior.DeriveSkillsFromTraits(characterObject.HeroObject, null);
                            }
                        }
                    }
                }
                int heroComesOfAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;
                foreach (Clan clan in Clan.All)
                {
                    foreach (Hero hero in clan.Heroes)
                    {
                        if (hero.Age >= (float)heroComesOfAge && hero.IsAlive && !hero.IsDisabled)
                        {
                            hero.ChangeState(Hero.CharacterStates.Active);
                        }
                    }
                }
            }
            int num = Clan.NonBanditFactions.Count<Clan>();
            int num2 = num / 100 + ((num % 100 > i) ? 1 : 0);
            int num3 = num / 100;
            for (int j = 0; j < i; j++)
            {
                num3 += ((num % 100 > j) ? 1 : 0);
            }
            for (int k = 0; k < num2; k++)
            {
                this.OnNonBanditClanDailyTick(Clan.NonBanditFactions.ElementAt(num3 + k));
            }
        }

        private void OnNewGameCreated(CampaignGameStarter obj)
        {
            this.CreateMinorFactionHeroes();
        }

        private void OnNewGameCreatedPartialFollowUpEnd(CampaignGameStarter starter)
        {
            foreach (Hero hero in Hero.AllAliveHeroes)
            {
                if (hero.IsActive)
                {
                    this.OnHeroDailyTick(hero);
                }
            }
        }

        private void OnHeroComesOfAge(Hero hero)
        {
            if (!hero.IsDisabled && hero.HeroState != Hero.CharacterStates.Active)
            {
                hero.ChangeState(Hero.CharacterStates.Active);
                //TeleportHeroAction.ApplyForCharacter(hero, hero.HomeSettlement);
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnCompanionRemoved(Hero companion)
        {
            if (!companion.IsFugitive && !companion.IsDead)
            {
                Settlement settlement = this.FindASuitableSettlementToTeleportForCompanion(companion);
                if (settlement == null)
                {
                    settlement = SettlementHelper.FindRandomSettlement((Settlement x) => x.IsTown && x.Culture == companion.Culture);
                }
                //TeleportHeroAction.ApplyForCharacter(companion, settlement);
            }
        }

        private void OnHeroDailyTick(Hero hero)
        {
            Settlement settlement = null;
            if (hero.IsFugitive || hero.IsReleased)
            {
                if (!hero.IsSpecial)
                {
                    if (hero.IsPlayerCompanion || hero.IsWanderer)
                    {
                        settlement = this.FindASuitableSettlementToTeleportForCompanion(hero);
                    }
                    else if (MBRandom.RandomFloat < 0.3f || (hero.CurrentSettlement != null && hero.CurrentSettlement.MapFaction.IsAtWarWith(hero.MapFaction)))
                    {
                        settlement = this.FindASuitableSettlementToTeleportForNoble(hero, 0f);
                    }
                }
            }
            else if (hero.IsActive)
            {
                if (hero.CurrentSettlement == null && hero.PartyBelongedTo == null && !hero.IsSpecial)
                {
                    if (MobileParty.MainParty.MemberRoster.Contains(hero.CharacterObject))
                    {
                        MobileParty.MainParty.MemberRoster.RemoveTroop(hero.CharacterObject, 1, default(UniqueTroopDescriptor), 0);
                        MobileParty.MainParty.MemberRoster.AddToCounts(hero.CharacterObject, 1, false, 0, 0, true, -1);
                    }
                    else
                    {
                        settlement = this.FindASuitableSettlementToTeleportForNoble(hero, 0f);
                    }
                }
                else if (this.CanHeroMoveToAnotherSettlement(hero))
                {
                    settlement = (hero.IsWanderer ? this.FindASuitableSettlementToTeleportForCompanion(hero) : this.FindASuitableSettlementToTeleportForNoble(hero, 10f));
                }
            }
            if (settlement != null)
            {
                //TeleportHeroAction.ApplyForCharacter(hero, settlement);
                if (!hero.IsActive)
                {
                    hero.ChangeState(Hero.CharacterStates.Active);
                }
            }
        }

        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
            foreach (Hero hero in Hero.AllAliveHeroes)
            {
                if (hero != null && hero.IsReleased && hero.CurrentSettlement != null && !hero.CurrentSettlement.MapFaction.IsAtWarWith(hero.MapFaction))
                {
                    hero.ChangeState(Hero.CharacterStates.Active);
                }
            }
        }

        private void OnNonBanditClanDailyTick(Clan clan)
        {
            if (!clan.IsEliminated && clan != Clan.PlayerClan && !clan.IsNeutralClan)
            {
                this.ConsiderSpawningLordParties(clan);
            }
        }

        private bool IsTeleportable(Hero h)
        {
            return h.Clan != Clan.PlayerClan && !h.IsTemplate && h.IsAlive && !h.IsNotable && h.CanMoveToSettlement() && !h.IsHumanPlayerCharacter && !h.IsPartyLeader && !h.IsPrisoner && h.HeroState != Hero.CharacterStates.Disabled;
        }

        private bool CanHeroMoveToAnotherSettlement(Hero hero)
        {
            if (this.IsTeleportable(hero) && hero.GovernorOf == null && hero.PartyBelongedTo == null && hero.PartyBelongedToAsPrisoner == null && hero.CharacterObject.Occupation != Occupation.Special && hero.Age >= (float)Campaign.Current.Models.AgeModel.HeroComesOfAge)
            {
                Settlement currentSettlement = hero.CurrentSettlement;
                return ((currentSettlement != null) ? currentSettlement.Town : null) == null || (!hero.CurrentSettlement.Town.HasTournament && !hero.CurrentSettlement.IsUnderSiege);
            }
            return false;
        }

        private Settlement FindASuitableSettlementToTeleportForCompanion(Hero companion)
        {
            List<ValueTuple<Settlement, float>> list = new List<ValueTuple<Settlement, float>>(Settlement.All.Count);
            foreach (Settlement settlement in Settlement.All.Where(s => s.Culture == companion.Culture))
            {
                list.Add(new ValueTuple<Settlement, float>(settlement, this.GetMoveScoreForCompanion(companion, settlement)));
            }
            return (from x in list
                    orderby x.Item2 descending
                    select x).Take(5).GetRandomElementInefficiently<ValueTuple<Settlement, float>>().Item1;
        }

        private Settlement FindASuitableSettlementToTeleportForNoble(Hero hero, float minimumScore)
        {
            List<Settlement> list = (from x in hero.MapFaction.Settlements
                                     where x.IsTown
                                     select x).ToList<Settlement>();
            Settlement settlement;
            if (list.Any<Settlement>())
            {
                settlement = MBRandom.ChooseWeighted<Settlement>(list, delegate (Settlement x)
                {
                    float moveScoreForNoble = this.GetMoveScoreForNoble(hero, x.Town);
                    if (moveScoreForNoble < minimumScore)
                    {
                        return 0f;
                    }
                    return moveScoreForNoble;
                });
            }
            else
            {
                List<Settlement> list2 = new List<Settlement>();
                List<Settlement> list3 = new List<Settlement>();
                foreach (Town town in Town.AllTowns)
                {
                    if (town.MapFaction.IsAtWarWith(hero.MapFaction))
                    {
                        list3.Add(town.Settlement);
                    }
                    else if (town.MapFaction != hero.MapFaction)
                    {
                        list2.Add(town.Settlement);
                    }
                }
                settlement = MBRandom.ChooseWeighted<Settlement>(list2, delegate (Settlement x)
                {
                    float moveScoreForNoble = this.GetMoveScoreForNoble(hero, x.Town);
                    if (moveScoreForNoble < minimumScore)
                    {
                        return 0f;
                    }
                    return moveScoreForNoble;
                });
                if (settlement == null)
                {
                    settlement = MBRandom.ChooseWeighted<Settlement>(list3, delegate (Settlement x)
                    {
                        float moveScoreForNoble = this.GetMoveScoreForNoble(hero, x.Town);
                        if (moveScoreForNoble < minimumScore)
                        {
                            return 0f;
                        }
                        return moveScoreForNoble;
                    });
                }
            }
            return settlement;
        }

        private float GetMoveScoreForCompanion(Hero companion, Settlement settlement)
        {
            float num = 0f;
            if (settlement.IsTown)
            {
                num = 1E-06f;
                if (!FactionManager.IsAtWarAgainstFaction(settlement.Party.MapFaction, companion.MapFaction))
                {
                    num += ((settlement == companion.HomeSettlement) ? 25f : 0f);
                    num += (FactionManager.IsNeutralWithFaction(settlement.Party.MapFaction, companion.MapFaction) ? 50f : 100f);
                    Settlement settlement2 = companion.LastSeenPlace ?? companion.HomeSettlement;
                    Vec2 v = new Vec2((settlement2.Position2D.x + MobileParty.MainParty.Position2D.x) / 2f, (settlement2.Position2D.y + MobileParty.MainParty.Position2D.y) / 2f);
                    float num2 = settlement.Position2D.Distance(v);
                    if (num2 <= 50f)
                    {
                        num += 150f;
                    }
                    else if (num2 <= 100f)
                    {
                        num += 100f;
                    }
                    else if (num2 <= 150f)
                    {
                        num += 50f;
                    }
                    else
                    {
                        num += 10f;
                    }
                    if (num <= 60f)
                    {
                        num = 1f;
                    }
                }
            }
            return num;
        }

        private float GetHeroPartyCommandScore(Hero hero)
        {
            return 3f * (float)hero.GetSkillValue(DefaultSkills.Tactics) + 2f * (float)hero.GetSkillValue(DefaultSkills.Leadership) + (float)hero.GetSkillValue(DefaultSkills.Scouting) + (float)hero.GetSkillValue(DefaultSkills.Steward) + (float)hero.GetSkillValue(DefaultSkills.OneHanded) + (float)hero.GetSkillValue(DefaultSkills.TwoHanded) + (float)hero.GetSkillValue(DefaultSkills.Polearm) + (float)hero.GetSkillValue(DefaultSkills.Riding) + ((hero.Clan.Leader == hero) ? 1000f : 0f);
        }

        private float GetMoveScoreForNoble(Hero hero, Town fief)
        {
            Clan clan = hero.Clan;
            float num = 1E-06f;
            if (!fief.IsUnderSiege && !fief.MapFaction.IsAtWarWith(hero.MapFaction))
            {
                num = (FactionManager.IsAlliedWithFaction(fief.MapFaction, hero.MapFaction) ? 0.01f : 1E-05f);
                if (fief.MapFaction == hero.MapFaction)
                {
                    num += 10f;
                    if (fief.IsTown)
                    {
                        num += 100f;
                    }
                    if (fief.OwnerClan == clan)
                    {
                        num += (fief.IsTown ? 500f : 100f);
                    }
                    if (fief.HasTournament)
                    {
                        num += 400f;
                    }
                }
                foreach (Hero hero2 in fief.Settlement.HeroesWithoutParty)
                {
                    if (clan != null && hero2.Clan == clan)
                    {
                        num += (fief.IsTown ? 100f : 10f);
                    }
                }
                if (fief.Settlement.IsStarving)
                {
                    num *= 0.1f;
                }
                if (hero.CurrentSettlement == fief.Settlement)
                {
                    num *= 3f;
                }
            }
            return num;
        }

        private void ConsiderSpawningLordParties(Clan clan)
        {
            int partyLimitForTier = Campaign.Current.Models.ClanTierModel.GetPartyLimitForTier(clan, clan.Tier);
            int num = clan.WarPartyComponents.Count<WarPartyComponent>();
            if (num >= partyLimitForTier)
            {
                return;
            }
            int num2 = partyLimitForTier - num;
            for (int i = 0; i < num2; i++)
            {
                Hero bestAvailableCommander = this.GetBestAvailableCommander(clan);
                if (bestAvailableCommander == null)
                {
                    break;
                }
                float num3 = this.CalculateScoreToCreateParty(clan);
                if (this.GetHeroPartyCommandScore(bestAvailableCommander) + num3 > 100f)
                {
                    MobileParty mobileParty = this.SpawnLordParty(bestAvailableCommander);
                    if (mobileParty != null)
                    {
                        this.GiveInitialItemsToParty(mobileParty);
                    }
                }
            }
        }

        private float CalculateScoreToCreateParty(Clan clan)
        {
            return (float)(clan.Fiefs.Count * 100 - clan.WarPartyComponents.Count<WarPartyComponent>() * 100) + (float)clan.Gold * 0.01f + (clan.IsMinorFaction ? 200f : 0f) + (clan.WarPartyComponents.Any<WarPartyComponent>() ? 0f : 200f);
        }

        private Hero GetBestAvailableCommander(Clan clan)
        {
            Hero hero = null;
            float num = 0f;
            foreach (Hero hero2 in clan.Heroes)
            {
                if (hero2.IsActive && hero2.IsAlive && hero2.PartyBelongedTo == null && hero2.PartyBelongedToAsPrisoner == null && hero2.CanLeadParty() && hero2.Age > (float)Campaign.Current.Models.AgeModel.HeroComesOfAge && hero2.CharacterObject.Occupation == Occupation.Lord)
                {
                    float heroPartyCommandScore = this.GetHeroPartyCommandScore(hero2);
                    if (heroPartyCommandScore > num)
                    {
                        num = heroPartyCommandScore;
                        hero = hero2;
                    }
                }
            }
            if (hero != null)
            {
                return hero;
            }
            if (clan != Clan.PlayerClan)
            {
                foreach (Hero hero3 in clan.Heroes)
                {
                    if (hero3.IsActive && hero3.IsAlive && hero3.PartyBelongedTo == null && hero3.PartyBelongedToAsPrisoner == null && hero3.Age > (float)Campaign.Current.Models.AgeModel.HeroComesOfAge && hero3.CharacterObject.Occupation == Occupation.Lord)
                    {
                        float heroPartyCommandScore2 = this.GetHeroPartyCommandScore(hero3);
                        if (heroPartyCommandScore2 > num)
                        {
                            num = heroPartyCommandScore2;
                            hero = hero3;
                        }
                    }
                }
            }
            return hero;
        }

        private MobileParty SpawnLordParty(Hero hero)
        {
            Settlement settlement = SettlementHelper.GetBestSettlementToSpawnAround(hero);
            MobileParty result;
            if (settlement != null && settlement.MapFaction == hero.MapFaction)
            {
                result = MobilePartyHelper.SpawnLordParty(hero, settlement);
            }
            else if (hero.MapFaction.InitialPosition.IsValid)
            {
                result = MobilePartyHelper.SpawnLordParty(hero, hero.MapFaction.InitialPosition, 30f);
            }
            else
            {
                foreach (Settlement settlement2 in Settlement.All)
                {
                    if (settlement2.Culture == hero.Culture)
                    {
                        settlement = settlement2;
                        break;
                    }
                }
                if (settlement != null)
                {
                    result = MobilePartyHelper.SpawnLordParty(hero, settlement);
                }
                else
                {
                    result = MobilePartyHelper.SpawnLordParty(hero, Settlement.All.GetRandomElement<Settlement>());
                }
            }
            return result;
        }

        private void GiveInitialItemsToParty(MobileParty heroParty)
        {
            float num = (254f + Campaign.AverageDistanceBetweenTwoTowns * 4.54f) / 2f;
            foreach (Settlement settlement in Campaign.Current.Settlements)
            {
                if (settlement.IsVillage)
                {
                    float num2 = heroParty.Position2D.Distance(settlement.Position2D);
                    if (num2 < num)
                    {
                        foreach (ValueTuple<ItemObject, float> valueTuple in settlement.Village.VillageType.Productions)
                        {
                            ItemObject item = valueTuple.Item1;
                            float item2 = valueTuple.Item2;
                            float num3 = (item.ItemType == ItemObject.ItemTypeEnum.Horse && item.HorseComponent.IsRideable && !item.HorseComponent.IsPackAnimal) ? 7f : (item.IsFood ? 0.1f : 0f);
                            float num4 = ((float)heroParty.MemberRoster.TotalManCount + 2f) / 200f;
                            float num5 = 1f - num2 / num;
                            int num6 = MBRandom.RoundRandomized(num3 * item2 * num5 * num4);
                            if (num6 > 0)
                            {
                                heroParty.ItemRoster.AddToCounts(item, num6);
                            }
                        }
                    }
                }
            }
        }

        private void WeeklyTick()
        {
            this.CreateMinorFactionHeroes();
        }

        private void CreateMinorFactionHeroes()
        {
            foreach (Clan clan in Clan.NonBanditFactions)
            {
                if (clan.IsMinorFaction && clan.Leader != Hero.MainHero)
                {
                    int num = 4;
                    foreach (Hero hero in Hero.AllAliveHeroes)
                    {
                        if ((hero.CharacterObject.Occupation == Occupation.Lord || hero.CharacterObject.Occupation == Occupation.Mercenary || hero.CharacterObject.Occupation == Occupation.Wanderer) && hero.Age >= (float)Campaign.Current.Models.AgeModel.HeroComesOfAge && hero.Clan == clan && !hero.IsTemplate)
                        {
                            num--;
                            if (hero.MapFaction.Leader == null)
                            {
                                if (!hero.MapFaction.IsKingdomFaction)
                                {
                                    (hero.MapFaction as Clan).SetLeader(hero);
                                }
                                else
                                {
                                    (hero.MapFaction as Kingdom).RulingClan = clan;
                                }
                            }
                        }
                    }
                    if (num > 0)
                    {
                        for (int i = 0; i < num; i++)
                        {
                            if (!Campaign.Current.GameStarted || MBRandom.RandomFloat < 0.1f)
                            {
                                int num2;
                                Math.DivRem(i, 4, out num2);
                                string objectName = string.Concat(new object[]
                                {
                                    "spc_",
                                    clan.StringId.ToLower(),
                                    "_leader_",
                                    num2
                                });
                                CharacterObject @object = Game.Current.ObjectManager.GetObject<CharacterObject>(objectName);
                                if (@object != null)
                                {
                                    Hero hero2 = HeroCreator.CreateSpecialHero(@object, null, clan, null, Campaign.Current.GameStarted ? 19 : -1);
                                    hero2.ChangeState(Campaign.Current.GameStarted ? Hero.CharacterStates.Active : Hero.CharacterStates.NotSpawned);
                                    hero2.IsMinorFactionHero = true;
                                    if (hero2.MapFaction.Leader == null)
                                    {
                                        if (!hero2.MapFaction.IsKingdomFaction)
                                        {
                                            (hero2.MapFaction as Clan).SetLeader(hero2);
                                        }
                                        else
                                        {
                                            (hero2.MapFaction as Kingdom).RulingClan = clan;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void OnGovernorChanged(Town fortification, Hero oldGovernor, Hero newGovernor)
        {
            if (oldGovernor != null && oldGovernor.Clan != null)
            {
                foreach (Hero hero in oldGovernor.Clan.Heroes)
                {
                    hero.UpdateHomeSettlement();
                }
            }
            if (newGovernor != null && newGovernor.Clan != null && (oldGovernor == null || newGovernor.Clan != oldGovernor.Clan))
            {
                foreach (Hero hero2 in newGovernor.Clan.Heroes)
                {
                    hero2.UpdateHomeSettlement();
                }
            }
        }

        public const float DefaultHealingPercentage = 0.015f;

        private const float MinimumScoreForSafeSettlement = 10f;
    }
}
