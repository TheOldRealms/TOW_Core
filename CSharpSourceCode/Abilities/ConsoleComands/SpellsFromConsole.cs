using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TOW_Core.Abilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Spells.ConsoleComands
{
    public class SpellsFromConsole
    {
        private static List<string> towSpellNames = AbilityFactory.GetAllSpellNamesAsList();

        [CommandLineFunctionality.CommandLineArgumentFunction("list_spells", "tow")]
        public static string ListSpells(List<string> argumentNames) =>
            AggregateOutput("Available spells are:", towSpellNames);

        [CommandLineFunctionality.CommandLineArgumentFunction("list_player_spells", "tow")]
        public static string ListPlayerSpells(List<string> argumentNames) =>
            !CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType)
                ? CampaignCheats.ErrorType
                : AggregateOutput("Player got this spells:", Hero.MainHero.GetExtendedInfo().AllAbilities);

        [CommandLineFunctionality.CommandLineArgumentFunction("add_spells_to_player", "tow")]
        public static string AddSpells(List<string> arguments)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;

            var matchedArguments = new List<string>();
            var newSpells = new List<string>();
            var knownSpells = new List<string>();

            foreach (var argument in arguments)
            foreach (var towSpell in towSpellNames)
                if (string.Equals(towSpell, argument, StringComparison.CurrentCultureIgnoreCase))
                {
                    matchedArguments.Add(towSpell);

                    if (Hero.MainHero.HasAbility(towSpell))
                        knownSpells.Add(towSpell);
                    else
                    {
                        Hero.MainHero.AddAbility(towSpell);
                        newSpells.Add(towSpell);
                    }
                }

            if (newSpells.Count > 0)
                MakePlayerSpellCaster(null);

            return FormatAddedSpellsOutput(matchedArguments, knownSpells, newSpells);
        }

        private static string FormatAddedSpellsOutput(List<string> matchedArguments, List<string> knownSpells,
        List<string> newSpells) =>
            AggregateOutput("Matched spells:", matchedArguments) +
            AggregateOutput("Already known spells in request:", knownSpells) +
            AggregateOutput("Added spells :", newSpells
            );

        [CommandLineFunctionality.CommandLineArgumentFunction("make_player_necromancer", "tow")]
        public static string MakePlayerNecromancer(List<string> arguments)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;

            if (!Hero.MainHero.IsNecromancer())
                Hero.MainHero.AddAttribute("Necromancer");

            return MakePlayerSpellCaster(null) + "Player is necromancer now.\n ";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("make_player_spell_caster", "tow")]
        public static string MakePlayerSpellCaster(List<string> arguments)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;

            if (!Hero.MainHero.IsSpellCaster())
                Hero.MainHero.AddAttribute("SpellCaster");

            if (!Hero.MainHero.IsAbilityUser())
                Hero.MainHero.AddAttribute("AbilityUser");

            Hero.MainHero.GetExtendedInfo().MaxWindsOfMagic =
                Math.Max(Hero.MainHero.GetExtendedInfo().MaxWindsOfMagic, 30f);

            return "Player is spell caster now. \n";
        }

        private static string AggregateOutput(string topicHeader, List<string> matchedSpells) =>
            matchedSpells.Aggregate(
                $"\n{topicHeader}\n",
                (current, spell) =>
                    $"{current}{spell}\n"
            );
    }
}