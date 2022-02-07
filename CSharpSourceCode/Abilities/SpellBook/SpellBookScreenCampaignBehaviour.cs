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
using TaleWorlds.Library;

namespace TOW_Core.Abilities.SpellBook
{
    public class SpellBookScreenCampaignBehaviour : CampaignBehaviorBase
    {
        private SpellBookMapIconVM _vm;
        private GauntletLayer _layer;

        public override void RegisterEvents()
        {
            ScreenManager.OnPushScreen += ScreenManager_OnPushScreen;
        }

        private void ScreenManager_OnPushScreen(ScreenBase pushedScreen)
        {
            if(pushedScreen is MapScreen)
            {
                _vm = new SpellBookMapIconVM();
                _layer = new GauntletLayer(2);
                _layer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
                _layer.LoadMovie("SpellBookMapIcon", _vm);
                pushedScreen.AddLayer(_layer);
            }
        }

        public override void SyncData(IDataStore dataStore) { }
    }
}
