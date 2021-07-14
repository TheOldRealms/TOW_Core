using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;
using TOW_Core.ObjectDataExtensions;

namespace TOW_Core.Abilities
{
    [DefaultView]
    class AbilityHUDMissionView : MissionView
    {
        private AbilityHUD_VM _dataSource;
        private GauntletLayer _layer;
        private ExtendedInfoMissionLogic _infoManager;
        private bool _isInitialized;

        public override void EarlyStart()
        {
            _infoManager = Mission.Current.GetMissionBehaviour<ExtendedInfoMissionLogic>();
        }
       
        
        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            if (!_isInitialized)
            {
                this._dataSource = new AbilityHUD_VM();
                this._layer = new GauntletLayer(100);
                this._layer.LoadMovie("AbilityHUD", this._dataSource);
                base.MissionScreen.AddLayer(this._layer);
                _isInitialized = true;
            }
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (this._isInitialized)
            {
                this._dataSource.UpdateProperties();
            }
        }
    }
}
