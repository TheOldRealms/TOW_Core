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
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Items
{
    public class TorItemTraitVM : ViewModel
    {
        private HintViewModel _hintText;
        private string _icon;

        public TorItemTraitVM(ItemTrait trait)
        {
            _hintText = new HintViewModel(new TaleWorlds.Localization.TextObject(trait.ItemTraitDescription));
            _icon = "<img src=\"" + trait.IconName + "\"/>";
        }

		[DataSourceProperty]
		public HintViewModel Hint
		{
			get
			{
				return this._hintText;
			}
			set
			{
				if (value != this._hintText)
				{
					this._hintText = value;
					base.OnPropertyChangedWithValue(value, "Hint");
				}
			}
		}

		[DataSourceProperty]
		public string Icon
		{
			get
			{
				return this._icon;
			}
			set
			{
				if (value != this._icon)
				{
					this._icon = value;
					base.OnPropertyChangedWithValue(value, "Icon");
				}
			}
		}
	}
}