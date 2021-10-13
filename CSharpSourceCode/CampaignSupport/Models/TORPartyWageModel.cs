using Helpers;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.Models
{
    public class TORPartyWageModel : DefaultPartyWageModel
    {
        public override int GetTroopRecruitmentCost(CharacterObject troop, Hero buyerHero, bool withoutItemCost = false)
        {
            if (troop.IsUndead())
            {
                return 0;
            }
            return base.GetTroopRecruitmentCost(troop, buyerHero, withoutItemCost);
        }

        public override ExplainedNumber GetTotalWage(MobileParty mobileParty, bool includeDescriptions = false)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            int num7 = 0;
            int num8 = 0;
            int num9 = 0;
            int num10 = 0;
            int num11 = 0;
            bool flag = !mobileParty.HasPerk(DefaultPerks.Steward.AidCorps, false);
            int num12 = 0;
            int num13 = 0;
            for (int i = 0; i < mobileParty.MemberRoster.Count; i++)
            {
                TroopRosterElement elementCopyAtIndex = mobileParty.MemberRoster.GetElementCopyAtIndex(i);
                CharacterObject character = elementCopyAtIndex.Character;

                //SHOULD BE CHANGED
                var isAliveHero = character.IsHero && !character.HeroObject.IsUndead();
                var isAliveTroop = !character.IsHero && !character.IsUndead();
                if (isAliveHero || isAliveTroop)
                {

                    int num14 = flag ? elementCopyAtIndex.Number : (elementCopyAtIndex.Number - elementCopyAtIndex.WoundedNumber);
                    if (character.IsHero)
                    {
                        Hero heroObject = elementCopyAtIndex.Character.HeroObject;
                        Clan clan = character.HeroObject.Clan;
                        if (heroObject != ((clan != null) ? clan.Leader : null))
                        {
                            if (mobileParty.Leader != null && mobileParty.Leader.GetPerkValue(DefaultPerks.Steward.PaidInPromise))
                            {
                                num3 += MathF.Round((float)elementCopyAtIndex.Character.TroopWage * (1f + DefaultPerks.Steward.PaidInPromise.PrimaryBonus * 0.01f));
                            }
                            else
                            {
                                num3 += elementCopyAtIndex.Character.TroopWage;
                            }
                        }
                    }
                    else
                    {
                        if (character.Tier < 4)
                        {
                            if (character.Culture.IsBandit)
                            {
                                num9 += elementCopyAtIndex.Character.TroopWage * elementCopyAtIndex.Number;
                            }
                            num += elementCopyAtIndex.Character.TroopWage * num14;
                        }
                        else if (character.Tier == 4)
                        {
                            if (character.Culture.IsBandit)
                            {
                                num10 += elementCopyAtIndex.Character.TroopWage * elementCopyAtIndex.Number;
                            }
                            num2 += elementCopyAtIndex.Character.TroopWage * num14;
                        }
                        else if (character.Tier > 4)
                        {
                            if (character.Culture.IsBandit)
                            {
                                num11 += elementCopyAtIndex.Character.TroopWage * elementCopyAtIndex.Number;
                            }
                            num3 += elementCopyAtIndex.Character.TroopWage * num14;
                        }
                        if (character.IsInfantry)
                        {
                            num4 += num14;
                        }
                        if (character.IsMounted)
                        {
                            num5 += num14;
                        }
                        if (character.Occupation == Occupation.CaravanGuard)
                        {
                            num12 += elementCopyAtIndex.Number;
                        }
                        if (character.Occupation == Occupation.Mercenary)
                        {
                            num13 += elementCopyAtIndex.Number;
                        }
                        if (character.IsRanged)
                        {
                            num6 += num14;
                            if (character.Tier >= 4)
                            {
                                num7 += num14;
                                num8 += elementCopyAtIndex.Character.TroopWage * elementCopyAtIndex.Number;
                            }
                        }
                    }
                }
            }

            ExplainedNumber explainedNumber = new ExplainedNumber(0f, false, null);
            if (mobileParty.LeaderHero != null && mobileParty.LeaderHero.GetPerkValue(DefaultPerks.Roguery.DeepPockets))
            {
                num -= num9;
                num2 -= num10;
                num3 -= num11;
                int num15 = MathF.Round((float)(num9 + num10 + num11));
                explainedNumber.Add((float)num15, null, null);
                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Roguery.DeepPockets, mobileParty.LeaderHero.CharacterObject, false, ref explainedNumber);
            }
            int num16 = num + num2 + num3;
            ExplainedNumber result = new ExplainedNumber((float)num16, includeDescriptions, null);
            ExplainedNumber explainedNumber2 = new ExplainedNumber(1f, false, null);
            if (mobileParty.IsGarrison)
            {
                Settlement currentSettlement = mobileParty.CurrentSettlement;
                if (((currentSettlement != null) ? currentSettlement.Town : null) != null)
                {
                    if (mobileParty.CurrentSettlement.IsTown)
                    {
                        PerkHelper.AddPerkBonusForTown(DefaultPerks.OneHanded.MilitaryTradition, mobileParty.CurrentSettlement.Town, ref result);
                        PerkHelper.AddPerkBonusForTown(DefaultPerks.TwoHanded.Berserker, mobileParty.CurrentSettlement.Town, ref result);
                        PerkHelper.AddPerkBonusForTown(DefaultPerks.Bow.HunterClan, mobileParty.CurrentSettlement.Town, ref result);
                        float troopRatio = (float)num4 / (float)mobileParty.MemberRoster.TotalRegulars;
                        this.CalculatePartialGarrisonWageReduction(troopRatio, mobileParty, DefaultPerks.Polearm.StandardBearer, ref result, true);
                        float troopRatio2 = (float)num5 / (float)mobileParty.MemberRoster.TotalRegulars;
                        this.CalculatePartialGarrisonWageReduction(troopRatio2, mobileParty, DefaultPerks.Riding.CavalryTactics, ref result, true);
                        float troopRatio3 = (float)num6 / (float)mobileParty.MemberRoster.TotalRegulars;
                        this.CalculatePartialGarrisonWageReduction(troopRatio3, mobileParty, DefaultPerks.Crossbow.PeasantLeader, ref result, true);
                    }
                    else if (mobileParty.CurrentSettlement.IsCastle)
                    {
                        PerkHelper.AddPerkBonusForTown(DefaultPerks.Steward.StiffUpperLip, mobileParty.CurrentSettlement.Town, ref result);
                    }
                    PerkHelper.AddPerkBonusForTown(DefaultPerks.Steward.DrillSergant, mobileParty.CurrentSettlement.Town, ref result);
                    if (mobileParty.CurrentSettlement.OwnerClan.Leader.CharacterObject.GetFeatValue(DefaultFeats.Cultural.EmpireWageFeat))
                    {
                        result.AddFactor(DefaultFeats.Cultural.EmpireWageFeat.EffectBonus, GameTexts.FindText("str_culture", null));
                    }
                    foreach (Building building in mobileParty.CurrentSettlement.Town.Buildings)
                    {
                        float buildingEffectAmount = building.GetBuildingEffectAmount(BuildingEffectEnum.GarrisonWageReduce);
                        if (buildingEffectAmount > 0f)
                        {
                            explainedNumber2.AddFactor(-(buildingEffectAmount / 100f), building.Name);
                        }
                    }
                }
            }
            result.Add(explainedNumber.ResultNumber, null, null);
            float value = (mobileParty.LeaderHero != null && mobileParty.LeaderHero.Clan.Kingdom != null && !mobileParty.LeaderHero.Clan.IsUnderMercenaryService && mobileParty.LeaderHero.Clan.Kingdom.ActivePolicies.Contains(DefaultPolicies.MilitaryCoronae)) ? 0.1f : 0f;
            if (mobileParty.HasPerk(DefaultPerks.Crossbow.PickedShots, false) && num7 > 0)
            {
                float value2 = DefaultPerks.Crossbow.PickedShots.PrimaryBonus * (float)num8;
                result.Add(value2, DefaultPerks.Crossbow.PickedShots.Name, null);
            }
            if (mobileParty.HasPerk(DefaultPerks.Trade.SwordForBarter, true))
            {
                float num17 = (float)num12 / (float)mobileParty.MemberRoster.TotalRegulars;
                if (num17 > 0f)
                {
                    float value3 = DefaultPerks.Trade.SwordForBarter.SecondaryBonus * num17;
                    result.AddFactor(value3, DefaultPerks.Trade.SwordForBarter.Name);
                }
            }
            if (mobileParty.HasPerk(DefaultPerks.Steward.Contractors, false))
            {
                float num18 = (float)num13 / (float)mobileParty.MemberRoster.TotalRegulars;
                if (num18 > 0f)
                {
                    float value4 = DefaultPerks.Steward.Contractors.PrimaryBonus * num18;
                    result.AddFactor(value4, DefaultPerks.Steward.Contractors.Name);
                }
            }
            result.AddFactor(value, DefaultPolicies.MilitaryCoronae.Name);
            result.AddFactor(explainedNumber2.ResultNumber - 1f, new TextObject("{=a6FfHHVg}Building Effects", null));
            if (mobileParty.Leader != null && mobileParty.Leader.GetFeatValue(DefaultFeats.Cultural.AseraiNegativeWageFeat))
            {
                result.AddFactor(DefaultFeats.Cultural.AseraiNegativeWageFeat.EffectBonus, _cultureText);
            }
            if (mobileParty.HasPerk(DefaultPerks.Steward.Frugal, false))
            {
                result.AddFactor(DefaultPerks.Steward.Frugal.PrimaryBonus * 0.01f, DefaultPerks.Steward.Frugal.Name);
            }
            if (mobileParty.Army != null && mobileParty.HasPerk(DefaultPerks.Steward.EfficientCampaigner, true))
            {
                result.AddFactor(DefaultPerks.Steward.EfficientCampaigner.SecondaryBonus * 0.01f, DefaultPerks.Steward.EfficientCampaigner.Name);
            }
            if (mobileParty.SiegeEvent != null && mobileParty.SiegeEvent.BesiegerCamp.SiegeParties.Contains(mobileParty.Party) && mobileParty.HasPerk(DefaultPerks.Steward.MasterOfWarcraft, false))
            {
                result.AddFactor(DefaultPerks.Steward.MasterOfWarcraft.PrimaryBonus * 0.01f, DefaultPerks.Steward.MasterOfWarcraft.Name);
            }
            if (mobileParty.EffectiveQuartermaster != null)
            {
                PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Steward.PriceOfLoyalty, mobileParty.EffectiveQuartermaster.CharacterObject, DefaultSkills.Steward, true, ref result, 200);
            }
            return result;
        }

        public override int GetCharacterWage(int tier)
        {
            return base.GetCharacterWage(tier);
        }

        private void CalculatePartialGarrisonWageReduction(float troopRatio, MobileParty mobileParty, PerkObject perk, ref ExplainedNumber garrisonWageReductionMultiplier, bool isSecondaryEffect)
        {
            if (troopRatio > 0f && mobileParty.CurrentSettlement.Town.Governor != null && PerkHelper.GetPerkValueForTown(perk, mobileParty.CurrentSettlement.Town))
            {
                garrisonWageReductionMultiplier.AddFactor(isSecondaryEffect ? (perk.SecondaryBonus * troopRatio * 0.01f) : (perk.PrimaryBonus * troopRatio * 0.01f), perk.Name);
            }
        }


        public override int MaxWage => base.MaxWage;

        private static readonly TextObject _cultureText = GameTexts.FindText("str_culture", null);
    }
}
