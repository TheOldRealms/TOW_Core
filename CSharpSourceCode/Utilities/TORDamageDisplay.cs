using System.Globalization;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TOW_Core.Battle.Damage;

namespace TOW_Core.Utilities
{
    public static  class TORDamageDisplay
    {
        public static void DisplaySpellDamageResult(string SpellName, DamageType additionalDamageType, 
            int resultDamage, float damageAmplifier)
        {
            var displayColor = Color.White;
            string displayDamageType = "";

            switch (additionalDamageType)
            {
                case DamageType.Fire:
                    displayColor = Colors.Red;
                    displayDamageType = "fire";
                    break;
                case DamageType.Holy:
                    displayColor = Colors.Yellow;
                    displayDamageType = "holy";
                    break;
                case DamageType.Lightning:
                    displayColor = Colors.Blue;
                    displayDamageType = "lightning";
                    break;
                case DamageType.Magical:
                    displayColor = Colors.Cyan;
                    displayDamageType = "magical";
                    break;
                case DamageType.Physical:
                    displayColor = Color.White;
                    displayDamageType = "Physical";
                    break;
            }
            InformationManager.DisplayMessage(new InformationMessage(resultDamage + "cast damage consisting of  "+" ("+displayDamageType +") was applied "+ "which was modified by " + (1+damageAmplifier).ToString("##%", CultureInfo.InvariantCulture) , displayColor));
        }
        
        public static void DisplayDamageResult(int resultDamage, float[] categories)
        {
            var displaycolor = Color.White;
            var dominantAdditionalEffect = DamageType.Physical;
            float dominantCategory=0;
            string additionalDamageTypeText= "";
            
            for (int i = 2; i < categories.Length; i++) //starting from first real additional damage type
            {
                if (dominantCategory < categories[i])
                {
                    dominantCategory = categories[i];
                    dominantAdditionalEffect = (DamageType) i;
                }

                if (categories[i] > 0)
                {
                    DamageType t = (DamageType)i;
                    string s = ", " +(int) categories[i] + " was dealt in " + t;
                    if (additionalDamageTypeText == "")
                        additionalDamageTypeText = s;
                    else
                        additionalDamageTypeText.Add(s,false);
                }
            }

            switch (dominantAdditionalEffect)
            {
                case DamageType.Fire:
                    displaycolor = Colors.Red;
                    break;
                case DamageType.Holy:
                    displaycolor = Colors.Yellow;
                    break;
                case DamageType.Lightning:
                    displaycolor = Colors.Blue;
                    break;
                case DamageType.Magical:
                    displaycolor = Colors.Cyan;
                    break;
            }

            var resultText = (int) resultDamage+ " damage was dealt of which was "+ (int) categories[1]+ " "+ nameof(DamageType.Physical)+additionalDamageTypeText;
            InformationManager.DisplayMessage(new InformationMessage(resultText, displaycolor));
            
        }
    }
}