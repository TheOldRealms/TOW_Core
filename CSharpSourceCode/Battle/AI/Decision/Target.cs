using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.AI.Decision
{
    // This class exists primarily for two reasons.
    // Decoupling our implementation. If TaleWorlds changes their Threat class or removes it, we can rework all references to the logic in this class without having to change all of our classes.
    // Additionally, it did not make sense to refer to friendly units / formations as "Threats".
    public class Target : Threat
    {
        public Vec3 SelectedWorldPosition = Vec3.Zero;

        public float UtilityValue
        {
            get => ThreatValue;
            set => ThreatValue = value;
        }

        public Vec3 GetPosition()
        {
            if (SelectedWorldPosition != Vec3.Zero)
                return SelectedWorldPosition;
            return Position;
        }

        public new Agent Agent
        {
            get
            {
                if (base.Agent == null && Formation != null)
                {
                    return Formation.GetMedianAgent(false, false, SelectedWorldPosition == Vec3.Zero ? Formation.CurrentPosition : SelectedWorldPosition.AsVec2);
                }

                return base.Agent;
            }
            set => base.Agent = value;
        }
    }
}