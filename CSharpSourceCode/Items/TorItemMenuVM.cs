using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;
using TOW_Core.CampaignSupport;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Items
{
    public class TorItemMenuVM : ItemMenuVM
    {
		private ItemObject _lastSetItem;
		private bool _isMagicItem = false;
		private MBBindingList<TorItemTraitVM> _itemTraitList;

		// Description Text
		private string _itemDescription = "";
		private bool _hasDescription = false;

		// Read Button
		private HintViewModel _readHint;
		private bool _isSkillBook = false;

		public TorItemMenuVM(Action<ItemVM, int> resetComparedItems, InventoryLogic inventoryLogic, Func<WeaponComponentData, ItemObject.ItemUsageSetFlags> getItemUsageSetFlags, Func<EquipmentIndex, SPItemVM> getEquipmentAtIndex) : base(resetComparedItems, inventoryLogic, getItemUsageSetFlags, getEquipmentAtIndex)
        {
			_itemTraitList = new MBBindingList<TorItemTraitVM>();
			_readHint = new HintViewModel(new TaleWorlds.Localization.TextObject("Read scroll"));
		}

        public void SetItemExtra(SPItemVM item, ItemVM comparedItem = null, BasicCharacterObject character = null, int alternativeUsageIndex = 0)
        {
			ItemTraitList.Clear();
			IsMagicItem = false;
			_lastSetItem = item.ItemRosterElement.EquipmentElement.Item;
			ItemDescription = _lastSetItem.GetTorSpecificData().Description;
			HasDescription = !ItemDescription.IsEmpty();
			UpdateReadButton(_lastSetItem);

			if (_lastSetItem != null && _lastSetItem.GetTorSpecificData() != null)
            {
				var info = _lastSetItem.GetTorSpecificData();
				if(info != null && (info.DamageProportions.Any(x=>x.DamageType != Battle.Damage.DamageType.Physical) || info.ItemTraits.Count > 0))
				{
					IsMagicItem = true;
					if(info.ItemTraits.Count > 0)
                    {
						foreach(var itemTrait in info.ItemTraits)
                        {
							ItemTraitList.Add(new TorItemTraitVM(itemTrait));
                        }
                    }
                }
                if (_lastSetItem.HasWeaponComponent)
                {
					var damageprops = base.TargetItemProperties.Where(x => x.DefinitionLabel.Contains("Damage"));
					foreach(var prop in damageprops)
                    {
						int damagenum = 0;
						bool success = int.TryParse(prop.ValueLabel.Split(' ')[0], out damagenum);
                        if (success)
                        {
							prop.ValueLabel = "";
							if(info != null && info.DamageProportions.Count > 1)
                            {
								prop.ValueLabel += damagenum.ToString() + " (";
								for (int i = 0; i < info.DamageProportions.Count; i++)
								{
									var tuple = info.DamageProportions[i];
									prop.ValueLabel += ((int)(tuple.Percent * damagenum)).ToString() + " " + tuple.DamageType.ToString() + (i == info.DamageProportions.Count - 1 ? "" : "+");
								}
								prop.ValueLabel += ")";
							}
							else if (info != null && info.DamageProportions.Count == 1)
                            {
								prop.ValueLabel = damagenum.ToString() + " " + info.DamageProportions[0].DamageType.ToString();
							}
							if(prop.ValueLabel == "")
                            {
								prop.ValueLabel = damagenum.ToString() + " Physical";
                            }
                        }
                    }
                }
            }
        }

		private void UpdateReadButton(ItemObject selectedItem)
        {
			IsSkillBook = TORSkillBookCampaignBehavior.Instance.IsSkillBook(selectedItem) 
				&& InventoryManager.Instance.CurrentMode == InventoryMode.Default;
        }

		private void ExecuteReadItem()
		{
			if (!IsSkillBook
				|| !TORSkillBookCampaignBehavior.Instance.IsBookUseful(_lastSetItem))
            {
				TOWCommon.Say(String.Format("It seems that there is nothing more to gain from studying {0}", _lastSetItem.Name));
				return;
            }
			if (TORSkillBookCampaignBehavior.Instance.CurrentBook.Equals(_lastSetItem.StringId ?? "")) {
				TOWCommon.Say(String.Format("You are already reading {0}", _lastSetItem.Name));
				return;
            }

			TORSkillBookCampaignBehavior.Instance.CurrentBook =
				_lastSetItem.StringId ?? "";
			UpdateReadButton(_lastSetItem);
			TOWCommon.Say(String.Format("Selected {0} for reading!", _lastSetItem?.Name));
			return;
		}

		[DataSourceProperty]
		public bool IsMagicItem
		{
			get
			{
				return this._isMagicItem;
			}
			set
			{
				if (value != this._isMagicItem)
				{
					this._isMagicItem = value;
					base.OnPropertyChangedWithValue(value, "IsMagicItem");
				}
			}
		}

		[DataSourceProperty]
		public string ItemDescription
		{
			get
			{
				return this._itemDescription;
			}
			set
			{
				if (value != this._itemDescription)
				{
					this._itemDescription = value;
					base.OnPropertyChangedWithValue(value, "ItemDescription");
				}
			}
		}

		[DataSourceProperty]
		public bool HasDescription
		{
			get
			{
				return this._hasDescription;
			}
			set
			{
				if (value != this._hasDescription)
				{
					this._hasDescription = value;
					base.OnPropertyChangedWithValue(value, "HasDescription");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<TorItemTraitVM> ItemTraitList
		{
			get
			{
				return this._itemTraitList;
			}
			set
			{
				if (value != this._itemTraitList)
				{
					this._itemTraitList = value;
					base.OnPropertyChangedWithValue(value, "ItemTraitList");
				}
			}
		}

		[DataSourceProperty]
		public HintViewModel ReadHint
		{
			get
			{
				return this._readHint;
			}
			set
			{
				if (value != this._readHint)
				{
					this._readHint = value;
					base.OnPropertyChangedWithValue(value, "ReadHint");
				}
			}
		}

		[DataSourceProperty]
		public bool IsSkillBook
		{
			get
			{
				return this._isSkillBook;
			}
			set
			{
				if (value != this._isSkillBook)
				{
					this._isSkillBook = value;
					base.OnPropertyChangedWithValue(value, "IsSkillBook");
				}
			}
		}
	}
}
