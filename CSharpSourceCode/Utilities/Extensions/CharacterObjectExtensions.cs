using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace TOW_Core.Utilities.Extensions
{
    public static class CharacterObjectExtensions
    {
        public static bool IsTOWTemplate(this CharacterObject characterObject)
        {
            return characterObject.StringId.Contains("tow_");
        }
    }
}
