﻿using System;
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
        private bool _isInitialized;
        private MobilePartyExtendedInfo playerPartyAttribute;
        private ExtendedInfoMissionLogic staticAttributemanager;

        public override void EarlyStart()
        {
            staticAttributemanager = Mission.Current.GetMissionBehaviour<ExtendedInfoMissionLogic>();
            staticAttributemanager.NotifyPlayerPartyAttributeAssignedObservers += InializeMissionView;
        }


        private void InializeMissionView()
        {
            playerPartyAttribute = staticAttributemanager.GetPlayerAttribute();
            _isInitialized = true;
        }
        
        
        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
           
            if (!this._isInitialized)
            {
                
                //this._isInitialized = true;
                this._dataSource = new AbilityHUD_VM();
                this._layer = new GauntletLayer(100);
                this._layer.LoadMovie("AbilityHUD", this._dataSource);
                base.MissionScreen.AddLayer(this._layer);
            }
            
            
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (this._isInitialized)
            {
                _dataSource.SetWindsOfMagicValue(playerPartyAttribute.WindsOfMagic);
                this._dataSource.UpdateProperties();
            }
        }
    }
}
