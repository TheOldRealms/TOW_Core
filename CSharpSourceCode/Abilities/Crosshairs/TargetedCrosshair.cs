using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Crosshairs
{
    public class TargetedCrosshair : ProjectileCrosshair
    {
        public TargetedCrosshair(AbilityTemplate template) : base(template)
        {
        }

        public override void Tick()
        {
            base.Tick();
            FindAim();
        }

        public override void Hide()
        {
            base.Hide();
            RemoveAim();
        }

        private void FindAim()
        {
            Vec3 position;
            Vec3 normal;
            _missionScreen.GetProjectedMousePositionOnGround(out position, out normal);
            float distance;
            var agent = Mission.Current.RayCastForClosestAgent(Agent.Main.GetEyeGlobalPosition(), position, out distance, Agent.Main.Index);
            if (agent != null && agent.IsHuman)
            {
                if (agent != aim)
                    RemoveAim();
                SetAim(agent);
            }
            else
            {
                RemoveAim();
            }
        }

        private void SetAim(Agent aim)
        {
            if (aim != null)
            {
                this.aim = aim;
                if (aim.IsEnemyOf(Agent.Main))
                    this.aim.AgentVisuals.GetEntity().Root.SetContourColor(enemyColor);
                else
                    this.aim.AgentVisuals.GetEntity().Root.SetContourColor(friendColor);
            }
        }
        
        private void RemoveAim()
        {
            if (aim != null)
            {
                aim.AgentVisuals.GetEntity().Root.SetContourColor(colorLess);
                aim = null;
            }
        }

        public Agent Aim
        {
            get => aim;
        }

        private Agent aim;
    }
}
