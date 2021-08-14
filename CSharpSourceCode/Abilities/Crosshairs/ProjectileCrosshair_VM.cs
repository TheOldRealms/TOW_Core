using TaleWorlds.Library;

namespace TOW_Core.Abilities.Crosshairs
{
    public class ProjectileCrosshair_VM : ViewModel
    {
        private string _name = "Projectile Crosshair";
        private string _spriteName = "test_spell_crosshair";
        private bool isVisible = false;

        public ProjectileCrosshair_VM() : base()
        {
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
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                isVisible = value;
                base.OnPropertyChangedWithValue(value, "IsVisible");
            }
        }
    }
}
