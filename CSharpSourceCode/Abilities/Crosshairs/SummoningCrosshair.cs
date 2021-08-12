using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;

namespace TOW_Core.Abilities.Crosshairs
{
    public class SummoningCrosshair : AbilityCrosshair
    {
        public SummoningCrosshair(AbilityTemplate template) : base(template)
        {
            crosshair = GameEntity.Instantiate(Mission.Current.Scene, "custom_marker_1", false);
            crosshair.EntityFlags |= EntityFlags.NotAffectedBySeason;
            AddLight();
            UpdateFrame();
            IsVisible = false;
        }
        public override void Tick()
        {
            UpdateFrame();
        }
        private void UpdateFrame()
        {
            if (Agent.Main != null && this.missionScreen.GetProjectedMousePositionOnGround(out position, out vec, true))
            {
                if (Agent.Main.Position.Distance(position) < template.MaxDistance)
                    this.Position = position;
                else
                {
                    position = Agent.Main.LookFrame.Advance(template.MaxDistance).origin;
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
        public static bool IsPositionOnValidGround(WorldPosition worldPosition)
        {
            return Mission.Current.IsFormationUnitPositionAvailable(ref worldPosition, Mission.Current.PlayerTeam);
        }

        private Vec3 position;
        private Vec3 vec;
    }
}
