using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace TOW_Core.CampaignMode
{
    public class WorldMapAttribute
    {
        [SaveableField(1)]
        public Hero Leader;
        [SaveableField(2)]
        public string id;

        public string culture;

        public bool MagicUsers;
        public float WindsOfMagic;
        
        

        public WorldMapAttribute(string id)
        {
            this.id = id;
        }

        public WorldMapAttribute()
        {

        }
    }
}