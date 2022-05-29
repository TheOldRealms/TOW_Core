using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TOW_Core.Battle.Crosshairs;

namespace TOW_Core.Abilities.Crosshairs
{
    public class SelfCrosshair : AbilityCrosshair
    {
        private bool _isVisible = false;
        public SelfCrosshair(AbilityTemplate template) : base(template) { }

        public override void Dispose() => base.Dispose();

        public override bool IsVisible { get => _isVisible; protected set => _isVisible = value; }
    }
}
