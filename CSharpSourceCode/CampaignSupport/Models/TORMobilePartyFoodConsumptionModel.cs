using Helpers;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Core;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.Models
{
    public class TORMobilePartyFoodConsumptionModel : DefaultMobilePartyFoodConsumptionModel
    {
        public override ExplainedNumber CalculateDailyFoodConsumptionf(MobileParty party, bool includeDescription = false)
        {
            int num = 0;
            for (int i = 0; i < party.MemberRoster.Count; i++)
            {
                var bandit = party.MemberRoster.GetCharacterAtIndex(i);
                if (bandit.Culture.IsBandit && !bandit.IsUndead())
                {
                    num += party.MemberRoster.GetElementNumber(i);
                }
            }
            for (int j = 0; j < party.PrisonRoster.Count; j++)
            {
                var banditPrisoner = party.PrisonRoster.GetCharacterAtIndex(j);
                if (banditPrisoner.Culture.IsBandit && !banditPrisoner.IsUndead())
                {
                    num += party.PrisonRoster.GetElementNumber(j);
                }
            }
            int num2 = party.Party.NumberOfAllMembers - GetUndeadMemberCount(party);
            num2 += party.Party.NumberOfPrisoners > 0 ? (party.Party.NumberOfPrisoners - GetUndeadPrisonerCount(party)) / 2 : 0;
            if (num2 > 0)
            {
                if (party.Leader != null && party.Leader.GetPerkValue(DefaultPerks.Roguery.Promises))
                {
                    num2 += (int)((float)num * DefaultPerks.Roguery.Promises.PrimaryBonus * 0.01f);
                }
                if (num2 < 1)
                {
                    if (party.LeaderHero != null && party.LeaderHero.IsUndead())
                    {
                        return new ExplainedNumber(-0.001f);
                    }
                    else
                    {
                        num2 = 1;
                    }
                }
            }
            else
            {
                return new ExplainedNumber(-0.001f);
            }

            float baseNumber = -(float)num2 / 20f;
            ExplainedNumber result = new ExplainedNumber(baseNumber, includeDescription, null);
            this.CalculatePerkEffects(party, ref result);
            return result;
        }

        private void CalculatePerkEffects(MobileParty party, ref ExplainedNumber result)
        {
            PerkHelper.AddPerkBonusForParty(DefaultPerks.Athletics.Spartan, party, false, ref result);
            PerkHelper.AddPerkBonusForParty(DefaultPerks.Steward.Spartan, party, true, ref result);
            if (party.EffectiveQuartermaster != null)
            {
                PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Steward.PriceOfLoyalty, party.EffectiveQuartermaster.CharacterObject, DefaultSkills.Steward, true, ref result, 200);
            }
            TerrainType faceTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(party.CurrentNavigationFace);
            if (faceTerrainType == TerrainType.Forest || faceTerrainType == TerrainType.Steppe)
            {
                PerkHelper.AddPerkBonusForParty(DefaultPerks.Scouting.Foragers, party, true, ref result);
            }
            if (party.IsGarrison && party.CurrentSettlement != null && party.CurrentSettlement.Town.IsUnderSiege)
            {
                PerkHelper.AddPerkBonusForTown(DefaultPerks.Athletics.StrongLegs, party.CurrentSettlement.Town, ref result);
            }
            if (party.Army != null)
            {
                PerkHelper.AddPerkBonusForParty(DefaultPerks.Steward.StiffUpperLip, party, true, ref result);
            }
            SiegeEvent siegeEvent = party.SiegeEvent;
            if (((siegeEvent != null) ? siegeEvent.BesiegerCamp : null) != null && party.SiegeEvent.BesiegerCamp.SiegeParties.Contains(party.Party) && party.HasPerk(DefaultPerks.Steward.SoundReserves, true))
            {
                PerkHelper.AddPerkBonusForParty(DefaultPerks.Steward.SoundReserves, party, false, ref result);
            }
        }

        private int GetUndeadMemberCount(MobileParty party)
        {
            var totalUnded = 0;
            foreach (var tr in party.MemberRoster.GetTroopRoster())
            {
                if ((tr.Character.HeroObject != null && tr.Character.HeroObject.IsUndead()) || tr.Character.IsUndead())
                {
                    totalUnded += tr.Number;
                }
            }
            return totalUnded;
        }

        private int GetUndeadPrisonerCount(MobileParty party)
        {
            var totalUnded = 0;
            foreach (var tr in party.PrisonRoster.GetTroopRoster())
            {
                if ((tr.Character.HeroObject != null && tr.Character.HeroObject.IsUndead()) || tr.Character.IsUndead())
                {
                    totalUnded += tr.Number;
                }
            }
            return totalUnded;
        }
    }
}
