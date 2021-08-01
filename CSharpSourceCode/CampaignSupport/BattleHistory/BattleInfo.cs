using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace TOW_Core.CampaignSupport.BattleHistory
{
    public class BattleInfo
    {
        [SaveableField(0)]
        public List<CharacterInfo> EnemiesKilled = new List<CharacterInfo>();
        [SaveableField(1)]
        public List<CharacterInfo> AlliesKilled = new List<CharacterInfo>();
    }

    public class BattleInfoDefiner : SaveableTypeDefiner
    {
        public BattleInfoDefiner() : base(0x24BC1604) { }
        protected override void DefineClassTypes()
        {
            base.DefineClassTypes();
            AddClassDefinition(typeof(BattleInfo), 1);
            AddClassDefinition(typeof(CharacterInfo), 2);
        }

        protected override void DefineContainerDefinitions()
        {
            base.DefineContainerDefinitions();
            ConstructContainerDefinition(typeof(List<BattleInfo>));
            ConstructContainerDefinition(typeof(List<CharacterInfo>));
        }
    }
}
