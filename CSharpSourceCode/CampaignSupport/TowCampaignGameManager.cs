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
            VideoPlaybackState videoPlaybackState = Game.Current.GameStateManager.CreateState<VideoPlaybackState>();
            string towFullPath = ModuleHelper.GetModuleFullPath("TOW_Core");
            string sandboxFullPath = ModuleHelper.GetModuleFullPath("SandBox");
            string towIntroPath = towFullPath + "Videos/tow_intro.ivf";
            string sandboxIntroPath = sandboxFullPath + "Videos/campaign_intro.ivf";
            if (File.Exists(towIntroPath))
            {
                string audioPath = towFullPath + "Videos/tow_intro.ogg";
                string subtitleFileBasePath = towFullPath + "Videos/tow_intro";
                videoPlaybackState.SetStartingParameters(towIntroPath, audioPath, subtitleFileBasePath, 30f, true);
                videoPlaybackState.SetOnVideoFinisedDelegate(new Action(this.LaunchStoryModeCharacterCreation));
            }
            else
            {
                string audioPath = towFullPath + "Videos/campaign_intro.ogg";
                string subtitleFileBasePath = towFullPath + "Videos/campaign_intro";
                videoPlaybackState.SetStartingParameters(sandboxIntroPath, audioPath, subtitleFileBasePath, 60f, true);
                videoPlaybackState.SetOnVideoFinisedDelegate(new Action(this.LaunchStoryModeCharacterCreation));
            }
            
            Game.Current.GameStateManager.CleanAndPushState(videoPlaybackState, 0);
            base.IsLoaded = true;
        }

        private void LaunchStoryModeCharacterCreation()
        {
            CharacterCreationState gameState = Game.Current.GameStateManager.CreateState<CharacterCreationState>(new object[]
            {
                new StoryModeCharacterCreationContent()
            });
            Game.Current.GameStateManager.CleanAndPushState(gameState, 0);
        }
    }
}
