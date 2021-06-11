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

namespace TOW_Core.Abilities
{
    [DefaultView]
    class AbilityHUDMissionView : MissionView
    {
        private AbilityHUD_VM _dataSource;
        private GauntletLayer _layer;
        private bool _isInitialized;

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            if (!this._isInitialized)
            {
                this._isInitialized = true;
                this._dataSource = new AbilityHUD_VM();
                this._layer = new GauntletLayer(100);
                this._layer.LoadMovie("AbilityHUD", this._dataSource);
                base.MissionScreen.AddLayer(this._layer);
            }
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if(this._isInitialized) this._dataSource.UpdateProperties();
        }
    }
}
