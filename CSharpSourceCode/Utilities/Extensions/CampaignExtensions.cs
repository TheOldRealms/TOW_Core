using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace TOW_Core.Utilities.Extensions
{
    public static class CampaignExtensions
    {
        public static List<CharacterObject> GetTOWTemplates(this Campaign campaign)
        {
            List<CharacterObject> list = new List<CharacterObject>();
            foreach(var item in campaign.TemplateCharacters)
            {
                if (item.StringId.Contains("tow_")) list.Add(item);
            }
            return list;
        }
    }
}
