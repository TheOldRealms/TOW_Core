using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade.View.Missions;

namespace TOW_Core.Abilities
{
    [DefaultView]
    class AbilityHUDMissionView : MissionView
    {
        private AbilityHUD_VM _abilityHUD_VM;
        private AbilityHUD_VM _abilityHUD_VM2;
        private SpecialMoveHUD_VM _specialMoveHUD_VM;
        private GauntletLayer _abilityLayer;
        private GauntletLayer _abilityLayer2;
        private GauntletLayer _specialMoveLayer;
        private bool _isInitialized;
        
        public override void OnMissionScreenTick(float dt)
        {
            if (!_isInitialized)
            {
                _abilityHUD_VM = new AbilityHUD_VM();
                _abilityLayer = new GauntletLayer(100);
                _abilityLayer.LoadMovie("AbilityHUD", _abilityHUD_VM);
                MissionScreen.AddLayer(_abilityLayer);

                //_specialMoveHUD_VM = new SpecialMoveHUD_VM();
                //_specialMoveLayer = new GauntletLayer(99);
                //_specialMoveLayer.LoadMovie("SpecialMoveHUD", _specialMoveHUD_VM);
                //MissionScreen.AddLayer(_specialMoveLayer);

                _isInitialized = true;
            }
        }

        public override void OnMissionTick(float dt)
        {
            if (_isInitialized)
            {
                _abilityHUD_VM.UpdateProperties();
                //_specialMoveHUD_VM.UpdateProperties();
            }
        }
    }
}
