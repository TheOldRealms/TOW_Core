using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Crosshairs
{
    public class SummoningCrosshair : AbilityCrosshair
    {
        public SummoningCrosshair()
        {
            //crosshair = GameEntity.CreateEmpty(this.mission.Scene, true);
            //crosshair.AddComponent(MetaMesh.GetCopy("order_flag_a", true, false));
            crosshair = GameEntity.Instantiate(Mission.Current.Scene, "custom_marker", false);
            crosshair.EntityFlags |= EntityFlags.NotAffectedBySeason;
            AddLight();
            UpdateFrame();
            //MatrixFrame frame = crosshair.GetFrame();
            //frame.Scale(new Vec3(10f, 10f, 1f, -1f));
            //crosshair.SetFrame(ref frame);
            IsVisible = false;
        }
        public override void Tick()
        {
            UpdateFrame();
        }
        private void UpdateFrame()
        {
            Vec3 position;
            Vec3 vec;
            if (Agent.Main != null && this.missionScreen.GetProjectedMousePositionOnGround(out position, out vec, true))
            {
                if (Agent.Main.Position.Distance(position) < ability.Template.MaxDistance)
                    this.Position = position;
                else
                {
                    position = Agent.Main.LookFrame.Advance(ability.Template.MaxDistance).origin;
                    position.z = Mission.Current.Scene.GetGroundHeightAtPosition(Position);
                    Position = position;
                }
                //WorldPosition worldPosition = new WorldPosition(Mission.Current.Scene, UIntPtr.Zero, position, false);
                //isOnValidGround = (!this.IsVisible || AOEAbilityCrosshair.IsPositionOnValidGround(worldPosition));
            }
            else
            {
                Position = new Vec3(0f, 0f, -100000f, -1f);
                //isOnValidGround = false;
            }
        }
        public void SetVisibilty(bool visibility)
        {
            this.crosshair.SetVisibilityExcludeParents(visibility);
        }
        public static bool IsPositionOnValidGround(WorldPosition worldPosition)
        {
            return Mission.Current.IsFormationUnitPositionAvailable(ref worldPosition, Mission.Current.PlayerTeam);
        }
    }
}
