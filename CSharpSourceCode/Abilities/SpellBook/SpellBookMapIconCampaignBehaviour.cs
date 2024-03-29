﻿using SandBox.GauntletUI.Map;
using SandBox.View.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;

namespace TOW_Core.Abilities.SpellBook
{
    public class SpellBookMapIconCampaignBehaviour : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            ScreenManager.OnPushScreen += ScreenManager_OnPushScreen;
        }

        private void ScreenManager_OnPushScreen(ScreenBase pushedScreen)
        {
            if (pushedScreen.GetType() != typeof(MapScreen)) return;
            else
            {
                var mapscreen = pushedScreen as MapScreen;
                var mapview = mapscreen.GetMapView<SpellBookMapIconMapView>();
                if(mapview == null) mapscreen.AddMapView<SpellBookMapIconMapView>();
            }
        }

        public override void SyncData(IDataStore dataStore) { }
    }

    public class SpellBookMapIconMapView : MapView
    {
        private SpellBookMapIconVM _vm;
        private GauntletLayer _layer;
        private IGauntletMovie _movie;

        protected override void CreateLayout()
        {
            base.CreateLayout();
            _vm = new SpellBookMapIconVM();
            GauntletMapBasicView mapView = MapScreen.GetMapView<GauntletMapBasicView>();
            Layer = mapView.GauntletLayer;
            _layer = Layer as GauntletLayer;
            _movie = _layer.LoadMovie("SpellBookMapIcon", _vm);
        }

        protected override void OnMapScreenUpdate(float dt)
        {
            base.OnMapScreenUpdate(dt);
            _vm.RefreshValues();
        }

        protected override void OnFinalize()
        {
            _vm.OnFinalize();
            _vm = null;
            _layer.ReleaseMovie(_movie);
            _movie = null;
            _layer = null;
            Layer = null;
            base.OnFinalize();
        }
    }
}
