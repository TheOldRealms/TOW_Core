using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Crosshairs
{
    public class CenteredAOECrosshair : AOECrosshair
    {
        public CenteredAOECrosshair(AbilityTemplate template, Agent caster) : base(template)
        {
            this._caster = caster;
            _crosshair = GameEntity.Instantiate(Mission.Current.Scene, "custom_marker", false);
            _crosshair.EntityFlags |= EntityFlags.NotAffectedBySeason;
            MatrixFrame frame = _crosshair.GetFrame();
            frame.Scale(new Vec3(template.TargetCapturingRadius, template.TargetCapturingRadius, 1, -1));
            _crosshair.SetFrame(ref frame);
            AddLight();
            IsVisible = false;
        }
        public override void Tick()
        {
            if (!isBound)
            {
                if (_caster.AgentVisuals != null)
                {
                    isBound = true;
                    _caster.AgentVisuals.AddChildEntity(_crosshair);
                }
            }
            HighlightNearbyAgents();
        }

        private bool isBound;
        private Agent _caster;
    }
}
