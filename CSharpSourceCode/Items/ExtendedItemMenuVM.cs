using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;

namespace TOW_Core.Items
{
    public class ExtendedItemMenuVM : ItemMenuVM
    {

        public ExtendedItemMenuVM(Action<ItemVM, int> resetComparedItems, InventoryLogic inventoryLogic, Func<WeaponComponentData, ItemObject.ItemUsageSetFlags> getItemUsageSetFlags, Func<EquipmentIndex, SPItemVM> getEquipmentAtIndex) : base(resetComparedItems, inventoryLogic, getItemUsageSetFlags, getEquipmentAtIndex)
        {
        }

        internal static SPItemVM GetEquipmentAtIndex(EquipmentIndex arg)
        {
            throw new NotImplementedException();
        }

        internal static void ResetComparedItems(ItemVM arg1, int arg2)
        {
            throw new NotImplementedException();
        }
    }
}
