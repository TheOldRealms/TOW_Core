using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TOW_Core.Items;

namespace TOW_Core.Utilities.Extensions
{
    public static class ItemObjectExtensions
    {
        public static MagicWeaponEffect GetMagicalProperties(this ItemObject item)
        {
            return MagicWeaponEffectManager.GetEffectForItem(item.StringId);
        }

        public static bool IsMagicWeapon(this ItemObject item)
        {
            return MagicWeaponEffectManager.GetEffectForItem(item.StringId) != null;
        }
    }
}
