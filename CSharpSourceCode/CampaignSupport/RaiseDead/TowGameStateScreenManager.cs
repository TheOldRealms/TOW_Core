using SandBox.GauntletUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine.Screens;
using TaleWorlds.MountAndBlade.View.Screen;

namespace TOW_Core.CampaignSupport.RaiseDead
{
    class TowGameStateScreenManager : GameStateScreenManager, IGameStateManagerListener
    {
        public TowGameStateScreenManager() : base()
        {
            
        }

        void IGameStateManagerListener.OnCreateState(GameState gameState)
        {
            ScreenBase screenBase = CreateScreen(gameState);
            gameState.Listener = (screenBase as IGameStateListener);
        }

        public new ScreenBase CreateScreen(GameState state)
        {
            if(state.GetType().Equals(typeof(PartyState)))
            {
                return new TowGauntletPartyScreen((PartyState)state);
            }
            return base.CreateScreen(state);
        }
    }
}
