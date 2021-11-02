using System;
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
            var agent = Mission.Current.RayCastForClosestAgent(Agent.Main.GetEyeGlobalPosition(), position, out distance, Agent.Main.Index, 0.1f);
            if (agent != null)
            {
                if (agent.IsHuman)
                {
                    if (agent != aim)
                    {
                        RemoveAim();
                    }
                    lastAimIndex = agent.Index;
                    SetAim(agent);
                }
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


        public Int32 LastAimIndex
        {
            get => lastAimIndex;
        }

        public Agent Aim
        {
            get => aim;
        }

        private Int32 lastAimIndex;

        private Agent aim;
    }
}
