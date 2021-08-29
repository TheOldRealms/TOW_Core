using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Spells.ConsoleComands
{
    public class AddSpellsFromConsole
    {
        private static List<string> towSpellNames = AbilityFactory.GetAllSpellNamesAsList();

        private static readonly string invalidCallMessage =
            "Baby don't break me \n" +
            "Don't break me\n" +
            "No - no!\n" +
            "\n" +
            "Load TOW save, seriously.\n";


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
            {
                if (!Hero.MainHero.IsSpellCaster())
                    Hero.MainHero.AddAttribute("SpellCaster");

                if (!Hero.MainHero.IsAbilityUser())
                    Hero.MainHero.AddAttribute("AbilityUser");
            }

            return FormatOutput(matchedArguments, knownSpells, newSpells);
        }

        private static string FormatOutput(List<string> matchedArguments, List<string> knownSpells,
        List<string> newSpells) =>
            AggregateOutput("Matched spells:", matchedArguments) +
            AggregateOutput("Already known spells in request:", knownSpells) +
            AggregateOutput("Added spells :", newSpells
            );

        private static string AggregateOutput(string topicHeader, List<string> matchedSpells) =>
            matchedSpells.Aggregate(
                $"\n{topicHeader}\n",
                (current, spell) =>
                    $"{current}{spell}\n"
            );

        private static List<string> FindValidSpellsInArguments(List<string> argumentNames)
        {
            var matchedSpells = new List<string>();

            foreach (var argumentName in argumentNames)
            foreach (var knownSpell in towSpellNames)
                if (string.Equals(knownSpell, argumentName, StringComparison.CurrentCultureIgnoreCase))
                    matchedSpells.Add(knownSpell);
            return matchedSpells;
        }
    }
}