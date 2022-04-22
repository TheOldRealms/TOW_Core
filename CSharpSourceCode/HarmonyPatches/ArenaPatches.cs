using HarmonyLib;
using SandBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.Source.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class ArenaPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FightTournamentGame), "GetParticipantCharacters")]
        public static bool PatchDeadLoopForVCTroops(FightTournamentGame __instance, ref List<CharacterObject> __result, Settlement settlement, bool includePlayer = true, bool includeHeroes = true)
        {
            List<CharacterObject> list = new List<CharacterObject>();
            if (includePlayer)
            {
                list.Add(CharacterObject.PlayerCharacter);
            }
            if (includeHeroes)
            {
                int num = 0;
                while (num < settlement.Parties.Count && list.Count < __instance.MaximumParticipantCount)
                {
                    Hero leaderHero = settlement.Parties[num].LeaderHero;
                    if (CanNpcJoinTournament(leaderHero, list) && leaderHero.IsNoble)
                    {
                        list.Add(leaderHero.CharacterObject);
                    }
                    num++;
                }
            }
            if (includeHeroes)
            {
                int num = 0;
                while (num < settlement.HeroesWithoutParty.Count && list.Count < __instance.MaximumParticipantCount)
                {
                    Hero hero = settlement.HeroesWithoutParty[num];
                    if (CanNpcJoinTournament(hero, list) && (hero.IsNoble || hero.IsWanderer || hero.CompanionOf != null))
                    {
                        list.Add(hero.CharacterObject);
                    }
                    num++;
                }
            }
            if (includeHeroes)
            {
                int num = 0;
                while (num < settlement.Parties.Count && list.Count < __instance.MaximumParticipantCount)
                {
                    foreach (TroopRosterElement troopRosterElement in settlement.Parties[num].MemberRoster.GetTroopRoster())
                    {
                        if (list.Count >= __instance.MaximumParticipantCount)
                        {
                            break;
                        }
                        CharacterObject character = troopRosterElement.Character;
                        if (character.IsHero && character.HeroObject.Clan == Clan.PlayerClan && CanNpcJoinTournament(character.HeroObject, list))
                        {
                            list.Add(character);
                        }
                    }
                    num++;
                }
            }
            if (list.Count < __instance.MaximumParticipantCount)
            {
                while (list.Count < __instance.MaximumParticipantCount)
                {
                    list.Add(GetRandomHighTierTroopOfCulture(settlement.Culture));
                }
            }
            __result = list;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ArenaPracticeFightMissionController), "GetParticipantCharacters")]
        public static bool PatchDeadLoopForVCArena(ArenaPracticeFightMissionController __instance, ref List<CharacterObject> __result, Settlement settlement, int maxParticipantCount)
        {
            List<CharacterObject> list = new List<CharacterObject>();
            while (list.Count < maxParticipantCount)
            {
                list.Add(GetRandomMilitiaTroopOfCulture(settlement.Culture));
            }
            __result = list;
            return false;
        }

        private static CharacterObject GetRandomHighTierTroopOfCulture(CultureObject culture)
        {
            var list = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>();
            return list.GetRandomElementWithPredicate(x => x.Culture == culture && x.Tier > 3 && x.IsTOWTemplate() && x.Occupation == Occupation.Soldier);
        }

        private static CharacterObject GetRandomMilitiaTroopOfCulture(CultureObject culture)
        {
            var list = new List<CharacterObject>();
            var troop = culture.BasicTroop;
            GetUpgradeTargets(troop, ref list, 2);
            return list.GetRandomElement();
        }

        private static bool CanNpcJoinTournament(Hero hero, List<CharacterObject> list)
        {
            return hero != null && !hero.IsWounded && !hero.Noncombatant && !list.Contains(hero.CharacterObject) && hero != Hero.MainHero && hero.Age >= (float)Campaign.Current.Models.AgeModel.HeroComesOfAge;
        }

        private static void GetUpgradeTargets(CharacterObject troop, ref List<CharacterObject> list, int maxTier)
        {
            if (!list.Contains(troop) && troop.Tier <= maxTier)
            {
                list.Add(troop);
            }
            foreach (CharacterObject troop2 in troop.UpgradeTargets)
            {
                GetUpgradeTargets(troop2, ref list, maxTier);
            }
        }
    }
}
