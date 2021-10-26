using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;

namespace TOW_Core.Abilities
{
    [DefaultView]
    class AbilityHUDMissionView : MissionView
    {
        private bool _hasAbility;
        private bool _hasSpecialMove;
        private bool _isInitialized;
        private AbilityHUD_VM _abilityHUD_VM;
        private SpecialMoveHUD_VM _specialMoveHUD_VM;
        private GauntletLayer _abilityLayer;
        private GauntletLayer _specialMoveLayer;

        public override void OnBehaviourInitialize()
        {
            base.OnBehaviourInitialize();
            Mission.Current.OnMainAgentChanged += (o, s) => CheckMainAgent();

            _abilityHUD_VM = new AbilityHUD_VM();
            _abilityLayer = new GauntletLayer(100);
            _abilityLayer.LoadMovie("AbilityHUD", _abilityHUD_VM);
            MissionScreen.AddLayer(_abilityLayer);

            _specialMoveHUD_VM = new SpecialMoveHUD_VM();
            _specialMoveLayer = new GauntletLayer(99);
            _specialMoveLayer.LoadMovie("SpecialMoveHUD", _specialMoveHUD_VM);
            MissionScreen.AddLayer(_specialMoveLayer);

            _isInitialized = true;
        }

        private void CheckMainAgent()
        {
            if (Agent.Main != null)
            {
                var component = Agent.Main.GetComponent<AbilityComponent>();
                if (component != null)
                {
                    _hasAbility = component.CurrentAbility != null;
                    if (_hasAbility)
                    {
                        _abilityHUD_VM.IsVisible = true;
                    }
                    var specialMove = component.SpecialMove;
                    if (specialMove != null)
                    {
                        _specialMoveHUD_VM.SpecialMove = specialMove;
                        _specialMoveHUD_VM.SpriteName = specialMove.Template.SpriteName;
                        _specialMoveHUD_VM.Name = specialMove.Template.Name;
                        _specialMoveHUD_VM.IsVisible = true;
                        _hasSpecialMove = true;
                    }
                }
            }
        }

        public override void OnMissionTick(float dt)
        {
            if (_isInitialized)
            {
                if (Agent.Main != null && Agent.Main.State == TaleWorlds.Core.AgentState.Active)
                {
                    if (_hasAbility)
                    {
                        _abilityHUD_VM.UpdateProperties();
                    }
                    if (_hasSpecialMove)
                    {
                        _specialMoveHUD_VM.UpdateProperties();
                    }
                    return;
                }
                _abilityHUD_VM.IsVisible = false;
                _specialMoveHUD_VM.IsVisible = false;
            }
        }
    }
}
