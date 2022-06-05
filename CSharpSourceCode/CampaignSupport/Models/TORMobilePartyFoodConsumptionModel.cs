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
            int bandits = 0;
            for (int i = 0; i < party.MemberRoster.Count; i++)
            {
                var troop = party.MemberRoster.GetCharacterAtIndex(i);
                if (troop.Culture.IsBandit && !troop.IsUndead() && !troop.IsVampire())
                {
                    bandits += party.MemberRoster.GetElementNumber(i);
                }
            }
            for (int j = 0; j < party.PrisonRoster.Count; j++)
            {
                var troop = party.PrisonRoster.GetCharacterAtIndex(j);
                if (troop.Culture.IsBandit && !troop.IsUndead() && !troop.IsVampire())
                {
                    bandits += party.PrisonRoster.GetElementNumber(j);
                }
            }
            int eatingMembers = party.Party.NumberOfAllMembers - GetNonStarvingMemberCount(party);
            eatingMembers += party.Party.NumberOfPrisoners > 0 ? (party.Party.NumberOfPrisoners - GetNonStarvingPrisonerCount(party)) / 2 : 0;
            if (eatingMembers > 0)
            {
                if (party.LeaderHero != null && party.LeaderHero.GetPerkValue(DefaultPerks.Roguery.Promises))
                {
                    eatingMembers += (int)((float)bandits * DefaultPerks.Roguery.Promises.PrimaryBonus * 0.01f);
                }
            }
            else
            {
                return new ExplainedNumber(0);
            }

            float baseNumber = -(float)eatingMembers / 20f;
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

        private int GetNonStarvingMemberCount(MobileParty party)
        {
            var totalNonStarvingMembers = 0;
            foreach (var tr in party.MemberRoster.GetTroopRoster())
            {
                bool nonStarvingCondition = (tr.Character.HeroObject != null && (tr.Character.HeroObject.IsUndead() || tr.Character.HeroObject.IsVampire())) ||
                                            (tr.Character.IsUndead() || tr.Character.IsVampire());
                if (nonStarvingCondition)
                {
                    totalNonStarvingMembers += tr.Number;
                }
            }
            return totalNonStarvingMembers;
        }

        private int GetNonStarvingPrisonerCount(MobileParty party)
        {
            var totalNonStarvingPrisoners = 0;
            foreach (var tr in party.PrisonRoster.GetTroopRoster())
            {
                bool nonStarvingCondition = (tr.Character.HeroObject != null && (tr.Character.HeroObject.IsUndead() || tr.Character.HeroObject.IsVampire())) ||
                                            (tr.Character.IsUndead() || tr.Character.IsVampire());
                if (nonStarvingCondition)
                {
                    totalNonStarvingPrisoners += tr.Number;
                }
            }
            return totalNonStarvingPrisoners;
        }
    }
}
