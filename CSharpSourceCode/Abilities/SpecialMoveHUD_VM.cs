using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TOW_Core.Abilities
{
    public class SpecialMoveHUD_VM : ViewModel
    {
        private bool _isVisible;
        private int _chargeLevel;

        public SpecialMoveHUD_VM() : base() { }

        public void UpdateProperties()
        {
            IsVisible = true;
            ChargeLevel = Convert.ToInt32(SpecialMove.ChargeLevel);
        }

        public SpecialMove SpecialMove { get; set; }

        [DataSourceProperty]
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                if (value != _isVisible)
                {
                    _isVisible = value;
                    base.OnPropertyChangedWithValue(value, "IsVisible");
                }
            }
        }

        [DataSourceProperty]
        public int ChargeLevel
        {
            get
            {
                return _chargeLevel;
            }
            set
            {
                if (value != _chargeLevel)
                {
                    _chargeLevel = value;
                    base.OnPropertyChangedWithValue(value, "ChargeLevel");
                }
            }
        }
    }
}
