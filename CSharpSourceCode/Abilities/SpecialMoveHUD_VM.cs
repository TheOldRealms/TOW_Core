using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;

namespace TOW_Core.Abilities
{
    public class SpecialMoveHUD_VM : ViewModel
    {
        private SpecialMove _specialMove = null;
        private string _name = "";
        private string _spriteName = "";
        private string _coolDownLeft = "";
        private bool _hasSpecialMove;
        private bool _onCoolDown;

        public SpecialMoveHUD_VM() : base() { }

        public void UpdateProperties()
        {
            if (Agent.Main == null)
            {
                HasSpecialMove = false;
                return;
            }
            _specialMove = Agent.Main.GetComponent<AbilityComponent>().SpecialMove;
            HasSpecialMove = _specialMove != null;
            if (HasSpecialMove)
            {
                SpriteName = _specialMove.Template.SpriteName;
                Name = _specialMove.Template.Name;
                CoolDownLeft = _specialMove.GetCoolDownLeft().ToString();
                IsOnCoolDown = _specialMove.IsOnCooldown();
                if (Game.Current.GameType is Campaign)
                {
                    if (!IsOnCoolDown)
                    {
                        CoolDownLeft = "";
                    }
                    IsOnCoolDown = true;
                }
            }
        }

        [DataSourceProperty]
        public bool HasSpecialMove
        {
            get
            {
                return _hasSpecialMove;
            }
            set
            {
                if (value != _hasSpecialMove)
                {
                    _hasSpecialMove = value;
                    base.OnPropertyChangedWithValue(value, "HasSpecialMove");
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
