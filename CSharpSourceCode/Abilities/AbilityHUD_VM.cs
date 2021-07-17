using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.TwoDimension;
using TOW_Core.ObjectDataExtensions;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities
{
    public class AbilityHUD_VM : ViewModel
    {
        private Ability _ability = null;
        private string _name = "";
        private string _spriteName = "";
        private string _coolDownLeft = "";
        private string _WindsOfMagicLeft = "No WoM in CustomBattle.";
        private bool _hasAnyAbility;
        private bool _onCoolDown;
        private float _windsOfMagicValue;

        public AbilityHUD_VM() : base() { }

        public void UpdateProperties()
        {

            if (Agent.Main == null) 
            {
                HasAnyAbility = false;
                return;
            }
            _ability = Agent.Main.GetCurrentAbility();
            
            HasAnyAbility = _ability != null;
            
            if (HasAnyAbility)
            {
                SpriteName = _ability.Template.SpriteName;
                Name = _ability.Template.Name;
                CoolDownLeft = _ability.GetCoolDownLeft().ToString();
                IsOnCoolDown = _ability.IsOnCooldown();
                if(Game.Current.GameType is Campaign && _ability is Spell)
                {
                    SetWindsOfMagicValue((float)(Agent.Main?.GetHero()?.GetExtendedInfo()?.CurrentWindsOfMagic));

                    if (_windsOfMagicValue < _ability.Template.WindsOfMagicCost)
                    {
                        IsOnCoolDown = true;
                        CoolDownLeft = "";
                    }
                }
            }
            
            
        }

        private void SetWindsOfMagicValue(float value)
        {
            _windsOfMagicValue = value;
            _WindsOfMagicLeft = ((int) _windsOfMagicValue).ToString();
            WindsOfMagicLeft = _WindsOfMagicLeft;
        }
        
        

        [DataSourceProperty]
        public bool HasAnyAbility
        {
            get
            {
                return _hasAnyAbility;
            }
            set
            {
                if (value != _hasAnyAbility)
                {
                    _hasAnyAbility = value;
                    base.OnPropertyChangedWithValue(value, "HasAnyAbility");
                }
            }
        }
        
        [DataSourceProperty]
        public string WindsOfMagicLeft
        {
            get
            {
                return _WindsOfMagicLeft;
            }
            set
            {
                _WindsOfMagicLeft = value;
                base.OnPropertyChangedWithValue(value, "WindsOfMagicLeft");
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
