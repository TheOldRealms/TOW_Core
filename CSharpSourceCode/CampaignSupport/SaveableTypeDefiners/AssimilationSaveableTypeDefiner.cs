using System.Collections.Generic;
using TaleWorlds.SaveSystem;
using TOW_Core.CampaignSupport.SettlementComponents;

namespace TOW_Core.CampaignSupport.SaveableTypeDefiners
{
    public class AssimilationSaveableTypeDefiner : SaveableTypeDefiner
    {
        public AssimilationSaveableTypeDefiner() : base(666000) { }

        protected override void DefineClassTypes()
        {
            base.DefineClassTypes();
            AddClassDefinition(typeof(AssimilationComponent), 1);
        }

        protected override void DefineContainerDefinitions()
        {
            base.DefineContainerDefinitions();
            ConstructContainerDefinition(typeof(List<AssimilationComponent>));
        }
    }
}
