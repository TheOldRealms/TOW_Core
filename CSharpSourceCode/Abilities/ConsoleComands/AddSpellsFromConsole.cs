using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities.ConsoleComands
{
    public class AddSpellsFromConsole
    {
        private static List<string> spells;

        static AddSpellsFromConsole()
        {
            spells = AbilityFactory.GetAllSpellNamesAsList();
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("list_abilities", "tow")]
        public static string ListAbilities(List<string> argumentNames) =>
            spells.Aggregate(
                "Available abilities are: \n",
                (current, ability) =>
                    $"{current}{ability} \n");

        [CommandLineFunctionality.CommandLineArgumentFunction("add_ability", "tow")]
        public static string AddAbilities(List<string> argumentNames)
        {
            if (Hero.MainHero == null)
            {
                return "Baby don't break me \n" +
                       "Don't break me \n" +
                       "No - no! \n" +
                       "\n" +
                       "Load TOW save, seriously.";
            }

            string log;

            var matchedAbilities = new List<string>();

            foreach (var argumentName in argumentNames)
            foreach (var knownSpell in spells)
                if (string.Equals(knownSpell, argumentName, StringComparison.CurrentCultureIgnoreCase))
                    matchedAbilities.Add(knownSpell);

            log = matchedAbilities.Aggregate(
                "Matched abilities are : \n",
                (current, ability) =>
                    $"{current}{ability} \n");

            log += "Already known abilities in request: \n";

            var newAbilities = new List<string>();

            foreach (var ability in matchedAbilities)
                if (Hero.MainHero.HasAbility(ability))
                    log += ability + "\n";
                else
                {
                    Hero.MainHero.AddAbility(ability);
                    newAbilities.Add(ability);
                }

            log += "Added abilities : \n";

            log = newAbilities.Aggregate(log,
                (current, newAbility) =>
                    current + (newAbility + " \n"));

            return log;
        }
    }
}