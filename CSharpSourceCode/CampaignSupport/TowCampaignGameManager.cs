using SandBox;
using StoryMode.CharacterCreationContent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.Core;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.CampaignSupport
{
    class TowCampaignGameManager : SandBoxGameManager
    {
        public override void OnLoadFinished()
        {
            LaunchStoryModeCharacterCreation();
            base.IsLoaded = true;
        }

        private void LaunchStoryModeCharacterCreation()
        {
            CharacterCreationState gameState = Game.Current.GameStateManager.CreateState<CharacterCreationState>(new object[]
            {
                new TOW_Core.CharacterCreation.TOWCharacterCreationContent()
            });
            Game.Current.GameStateManager.CleanAndPushState(gameState, 0);
        }
    }
}
