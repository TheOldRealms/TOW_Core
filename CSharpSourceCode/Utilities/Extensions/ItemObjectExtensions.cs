using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOW_Core.Items;

namespace TOW_Core.Utilities.Extensions
{
    public static class ItemObjectExtensions
    {
        public static List<ItemTrait> GetTraits(this ItemObject item)
        {
            var result = ExtendedItemObjectManager.GetAdditionalProperties(item.StringId);
            if (result == null) result = ExtendedItemObjectProperties.CreateDefault(item.StringId);
            return result.Traits.ToList();
        }

        public static List<ItemTrait> GetTraits(this ItemObject item, Agent agent)
        {
            var result = item.GetTraits();
            var comp = agent.GetComponent<ItemTraitAgentComponent>();
            if (comp != null)
            {
                if(result == null) result = new List<ItemTrait>();
                result.AddRange(comp.GetDynamicTraits(item));
            }
            return result;
        }

        public static ExtendedItemObjectProperties GetTorSpecificData(this ItemObject item)
        {
            var result = ExtendedItemObjectManager.GetAdditionalProperties(item.StringId);
            if (result == null) result = ExtendedItemObjectProperties.CreateDefault(item.StringId);
            return result;
        }

        public static ExtendedItemObjectProperties GetTorSpecificData(this ItemObject item, Agent agent)
        {
            var result = item.GetTorSpecificData();
            if (result == null) result = ExtendedItemObjectProperties.CreateDefault(item.StringId);
            var comp = agent.GetComponent<ItemTraitAgentComponent>();
            if(comp != null)
            {
                result.Traits.AddRange(comp.GetDynamicTraits(item));
            }
            return result;
        }

        public static bool HasTrait(this ItemObject item)
        {
            if (item.GetTraits() != null)
            {
                return item.GetTraits().Count > 0;
            }
            else return false;
        }

        public static bool HasTrait(this ItemObject item, Agent agent)
        {
            if (item.GetTraits(agent) != null)
            {
                return item.GetTraits(agent).Count > 0;
            }
            else return false;
        }

        public static bool IsTorItem(this ItemObject item)
        {
            return item.StringId.StartsWith("tor_");
        }
    }
}
