using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TOW_Core.CampaignSupport.Models
{
    public class TORPartySpeedCalculatingModel : DefaultPartySpeedCalculatingModel
    {
        public override ExplainedNumber CalculateFinalSpeed(MobileParty mobileParty, ExplainedNumber finalSpeed)
        {
            if (mobileParty.IsCustomParty && !((CustomPartyComponent)mobileParty.PartyComponent).CustomPartyBaseSpeed.ApproximatelyEqualsTo(0f, 1E-05f))
            {
                finalSpeed = new ExplainedNumber(((CustomPartyComponent)mobileParty.PartyComponent).CustomPartyBaseSpeed, false, null);
            }
            PartyBase party = mobileParty.Party;
            TerrainType faceTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(mobileParty.CurrentNavigationFace);
            Hero effectiveScout = mobileParty.EffectiveScout;
            if (faceTerrainType == TerrainType.Forest)
            {
                float num = 0f;
                if (effectiveScout != null && effectiveScout.GetPerkValue(DefaultPerks.Scouting.ForestKin))
                {
                    for (int i = 0; i < mobileParty.MemberRoster.Count; i++)
                    {
                        if (mobileParty.MemberRoster.GetCharacterAtIndex(i).DefaultFormationClass.Equals(FormationClass.Infantry))
                        {
                            num += (float)mobileParty.MemberRoster.GetElementNumber(i);
                        }
                    }
                }
                float num2 = (num / (float)mobileParty.MemberRoster.Count > 0.75f) ? -0.15f : -0.3f;
                finalSpeed.AddFactor(num2, _movingInForest);
                if (PartyBaseHelper.HasFeat(mobileParty.Party, DefaultCulturalFeats.BattanianForestSpeedFeat))
                {
                    float value = DefaultCulturalFeats.BattanianForestSpeedFeat.EffectBonus * -num2;
                    finalSpeed.AddFactor(value, _culture);
                }
            }
            else if (faceTerrainType == TerrainType.Water || faceTerrainType == TerrainType.River || faceTerrainType == TerrainType.Bridge || faceTerrainType == TerrainType.ShallowRiver)
            {
                finalSpeed.AddFactor(-0.3f, _fordEffect);
            }
            else if (faceTerrainType == TerrainType.Desert || faceTerrainType == TerrainType.Dune)
            {
                if (!PartyBaseHelper.HasFeat(mobileParty.Party, DefaultCulturalFeats.AseraiDesertFeat))
                {
                    finalSpeed.AddFactor(-0.1f, _desert);
                }
                if (effectiveScout != null && effectiveScout.GetPerkValue(DefaultPerks.Scouting.DesertBorn))
                {
                    finalSpeed.AddFactor(DefaultPerks.Scouting.DesertBorn.PrimaryBonus, DefaultPerks.Scouting.DesertBorn.Name);
                }
            }
            else if ((faceTerrainType == TerrainType.Plain || faceTerrainType == TerrainType.Steppe) && effectiveScout != null && effectiveScout.GetPerkValue(DefaultPerks.Scouting.Pathfinder))
            {
                finalSpeed.AddFactor(DefaultPerks.Scouting.Pathfinder.PrimaryBonus, DefaultPerks.Scouting.Pathfinder.Name);
            }
            if (Campaign.Current.Models.MapWeatherModel.GetIsSnowTerrainInPos(mobileParty.Position2D.ToVec3(0f)))
            {
                finalSpeed.AddFactor(-0.1f, _snow);
            }
            if (Campaign.Current.IsNight && mobileParty.Party.Culture.StringId != "khuzait")
            {
                finalSpeed.AddFactor(-0.25f, _night);
                if (effectiveScout != null && effectiveScout.GetPerkValue(DefaultPerks.Scouting.NightRunner))
                {
                    finalSpeed.AddFactor(DefaultPerks.Scouting.NightRunner.PrimaryBonus, DefaultPerks.Scouting.NightRunner.Name);
                }
            }
            else if (effectiveScout != null && effectiveScout.GetPerkValue(DefaultPerks.Scouting.DayTraveler))
            {
                finalSpeed.AddFactor(DefaultPerks.Scouting.DayTraveler.PrimaryBonus, DefaultPerks.Scouting.DayTraveler.Name);
            }
            if (party.LeaderHero != null)
            {
                PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Scouting.UncannyInsight, party.LeaderHero.CharacterObject, DefaultSkills.Scouting, true, ref finalSpeed, 200);
            }
            if (effectiveScout != null)
            {
                if (effectiveScout.GetPerkValue(DefaultPerks.Scouting.ForcedMarch) && mobileParty.Morale > 75f)
                {
                    finalSpeed.AddFactor(DefaultPerks.Scouting.ForcedMarch.PrimaryBonus, DefaultPerks.Scouting.ForcedMarch.Name);
                }
                if (mobileParty.DefaultBehavior == AiBehavior.EngageParty)
                {
                    MobileParty targetParty = mobileParty.TargetParty;
                    if (targetParty != null && targetParty.MapFaction.IsAtWarWith(mobileParty.MapFaction) && effectiveScout.GetPerkValue(DefaultPerks.Scouting.Tracker))
                    {
                        finalSpeed.AddFactor(DefaultPerks.Scouting.Tracker.SecondaryBonus, DefaultPerks.Scouting.Tracker.Name);
                    }
                }
            }
            if(mobileParty != null && mobileParty.Party.Culture.StringId == "khuzait")
            {
                finalSpeed.Add(0.5f, new TextObject("Vampire bonus"));
            }
            Army army = mobileParty.Army;
            if (((army != null) ? army.LeaderParty : null) != null && mobileParty.Army.LeaderParty != mobileParty && mobileParty.AttachedTo != mobileParty.Army.LeaderParty && mobileParty.Army.LeaderParty.HasPerk(DefaultPerks.Tactics.CallToArms, false))
            {
                finalSpeed.AddFactor(DefaultPerks.Tactics.CallToArms.PrimaryBonus, DefaultPerks.Tactics.CallToArms.Name);
            }
            finalSpeed.LimitMin(1f);
            return finalSpeed;
        }


        private readonly TextObject _movingInForest = new TextObject("{=rTFaZCdY}Forest", null);
        private readonly TextObject _fordEffect = new TextObject("{=NT5fwUuJ}Fording", null);
        private readonly TextObject _night = new TextObject("{=fAxjyMt5}Night", null);
        private readonly TextObject _snow = new TextObject("{=vLjgcdgB}Snow", null);
        private readonly TextObject _desert = new TextObject("{=ecUwABe2}Desert", null);
        private readonly TextObject _culture = GameTexts.FindText("str_culture", null);
    }
}
