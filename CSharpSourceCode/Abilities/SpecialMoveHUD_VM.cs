using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TOW_Core.Abilities
{
    public class SpecialMoveHUD_VM : ViewModel
    {
        private string _name = "";
        private string _spriteName = "";
        private string _coolDownLeft = "";
        private bool _isVisible;
        private bool _onCoolDown;

        public SpecialMoveHUD_VM() : base() { }

        public void UpdateProperties()
        {            
            CoolDownLeft = SpecialMove.GetCoolDownLeft().ToString();
            IsOnCoolDown = SpecialMove.IsOnCooldown();
            if (Game.Current.GameType is Campaign)
            {
                if (!IsOnCoolDown)
                {
                    CoolDownLeft = "";
                }
                IsOnCoolDown = true;
            }
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
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    base.OnPropertyChangedWithValue(value, "Name");
                }
            }
        }

        [DataSourceProperty]
        public string SpriteName
        {
            get
            {
                return _spriteName;
            }
            set
            {
                if (value != _spriteName)
                {
                    _spriteName = value;
                    base.OnPropertyChangedWithValue(value, "SpriteName");
                }
            }
        }

        [DataSourceProperty]
        public string CoolDownLeft
        {
            get
            {
                return _coolDownLeft;
            }
            set
            {
                if (value != _coolDownLeft)
                {
                    _coolDownLeft = value;
                    base.OnPropertyChangedWithValue(value, "CoolDownLeft");
                }
            }
        }

        [DataSourceProperty]
        public bool IsOnCoolDown
        {
            get
            {
                return _onCoolDown;
            }
            set
            {
                if (value != _onCoolDown)
                {
                    _onCoolDown = value;
                    base.OnPropertyChangedWithValue(value, "IsOnCoolDown");
                }
            }
        }
    }
}
