using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;

namespace TOW_Core.Abilities
{
    public class ArtilleryDeployer : Ability
    {
        private int _artilleryAmount;
        private AbilityComponent _abilityComponent;
        public delegate void OnArtilleryDeployed(); 
        public event OnArtilleryDeployed ArtilleryDeployed;

        public ArtilleryDeployer(AbilityTemplate template) : base(template)
        {
        }

        public void SetAmount(int amount)
        {
            _artilleryAmount = amount;
        }

        public void SetAbilityComponent(AbilityComponent component)
        {
            _abilityComponent = component;
        }

        public override bool CanCast(Agent casterAgent)
        {
            return base.CanCast(casterAgent) && _artilleryAmount > 0 && _abilityComponent.MaxArtilleryAmount > 0;
        }

        protected override void DoCast(Agent casterAgent)
        {
            base.DoCast(casterAgent);
            _artilleryAmount--;
            ArtilleryDeployed?.Invoke();
        }
    }
}
