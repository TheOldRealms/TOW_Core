using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.TriggeredEffect.Scripts
{
    public abstract class BlackPowderWeaponScript : ScriptComponentBehavior
    {
        protected Agent _shooterAgent;
        protected TriggeredEffect _explosion;


        public override TickRequirement GetTickRequirement()
        {
            return TickRequirement.Tick;
        }

        public void SetShooterAgent(Agent shooter)
        {
            _shooterAgent = shooter;
        }

        public void SetTriggeredEffect(TriggeredEffect effect)
        {
            _explosion = effect;
        }
    }
}
