using System;
using System.Runtime.Remoting.Channels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;
using TOW_Core.Utilities;

namespace TOW_Core.CampaignMode
{
    using static CampaignEvents;

 

    public  class Tow_CampaignManager
    {
        private static readonly Tow_CampaignManager instance;
        
        private Tow_CampaignManager(){}

        static Tow_CampaignManager()
        {
            instance = new Tow_CampaignManager();
        }
        public static Tow_CampaignManager Instance => instance;


        public void Initialize()
        {
           // Utilities.TOWCommon.Say("initialized");
          //  AttributeSystemManager.Instance.InitalizeAttributes();
            
        }
        
        public void Hello()
        {
            if (GameStateManager.Current.ActiveState.IsMission)
            {
                Utilities.TOWCommon.Say("Hello im in a mission");
                
            }
                
            else
            {
                Utilities.TOWCommon.Say("Hello im in the World");
            }
            
        }
        
        
        
    }
}