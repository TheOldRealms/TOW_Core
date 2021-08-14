using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Crosshairs
{
    public class CenteredAOECrosshair : AOECrosshair
    {
        public CenteredAOECrosshair(AbilityTemplate template, Agent agent) : base(template)
        {
            this.agent = agent;
            crosshair = GameEntity.Instantiate(Mission.Current.Scene, "custom_marker", false);
            crosshair.EntityFlags |= EntityFlags.NotAffectedBySeason;
            MatrixFrame frame = crosshair.GetFrame();
            frame.Scale(new Vec3(template.TargetCapturingRadius, template.TargetCapturingRadius, 1, -1));
            crosshair.SetFrame(ref frame);
            AddLight();
            IsVisible = false;
        }
        public override void Tick()
        {
            if (!isBound)
            {
                if (agent.AgentVisuals != null)
                {
                    isBound = true;
                    agent.AgentVisuals.AddChildEntity(crosshair);
                }
            }
            HighlightNearbyAgents();
        }

        private bool isBound;
        private Agent agent;
    }
}
