using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.MapBar
{
    public class TorMapInfoVM : MapInfoVM
    {
		private string _windsOfMagic = "0";
		private bool _isSpellCaster = false;
		private BasicTooltipViewModel _windsHint;
		private float _windRechargeRate = 0f;
		private int _maxWinds = 0;

		public TorMapInfoVM() : base()
        {
			this._windsHint = new BasicTooltipViewModel(GetHintText);
        }

        private List<TooltipProperty> GetHintText()
        {
			List<TooltipProperty> list = new List<TooltipProperty>();
			list.Add(new TooltipProperty("Winds of Magic", WindsOfMagic, 0, false, TooltipProperty.TooltipPropertyFlags.Title));
			list.Add(new TooltipProperty("", string.Empty, 0, false, TooltipProperty.TooltipPropertyFlags.RundownSeperator));
			list.Add(new TooltipProperty("Maximum:", _maxWinds.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));
			list.Add(new TooltipProperty("Recharge Rate:", _windRechargeRate.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));
			return list;
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
			RefreshExtraProperties();
        }

        public void RefreshExtraProperties()
        {
			IsSpellCaster = Hero.MainHero.IsSpellCaster();
            if (IsSpellCaster)
            {
				var info = Hero.MainHero.GetExtendedInfo();
				WindsOfMagic = ((int)info.CurrentWindsOfMagic).ToString();
				_maxWinds = (int)info.MaxWindsOfMagic;
				_windRechargeRate = info.WindsOfMagicRechargeRate;
            }
        }

		[DataSourceProperty]
		public bool IsSpellCaster
		{
			get
			{
				return this._isSpellCaster;
			}
			set
			{
				if (value != this._isSpellCaster)
				{
					this._isSpellCaster = value;
					base.OnPropertyChangedWithValue(value, "IsSpellCaster");
				}
			}
		}

		[DataSourceProperty]
		public string WindsOfMagic
		{
			get
			{
				return this._windsOfMagic;
			}
			set
			{
				if (value != this._windsOfMagic)
				{
					this._windsOfMagic = value;
					base.OnPropertyChangedWithValue(value, "WindsOfMagic");
				}
			}
		}

		[DataSourceProperty]
		public BasicTooltipViewModel WindsHint
		{
			get
			{
				return this._windsHint;
			}
			set
			{
				if (value != this._windsHint)
				{
					this._windsHint = value;
					base.OnPropertyChangedWithValue(value, "WindsHint");
				}
			}
		}
	}
}
