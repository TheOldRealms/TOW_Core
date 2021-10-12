using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace TOW_Core.CampaignSupport.Assimilation
{
    public class AssimilationSaveableTypeDefiner : SaveableTypeDefiner
    {
        public AssimilationSaveableTypeDefiner() : base(666000) { }

        protected override void DefineClassTypes()
        {
            base.DefineClassTypes();
            AddClassDefinition(typeof(AssimilationComponent), 1);
            AddClassDefinition(typeof(SettlementCultureChangedLogEntry), 2);
            AddClassDefinition(typeof(SettlementCultureChangedMapNotification), 3);
        }

        protected override void DefineContainerDefinitions()
        {
            base.DefineContainerDefinitions();
            ConstructContainerDefinition(typeof(List<AssimilationComponent>));
        }
    }
}
