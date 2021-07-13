using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.ObjectDataExtensions.CustomAgentComponents;
using TaleWorlds.CampaignSystem;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.ObjectDataExtensions.CustomMissionLogic
{
    class AttributeSystemMissionLogic : MissionLogic
    {
        public AttributeSystemMissionLogic()
        {
        }

        public override void OnAgentCreated(Agent agent)
        {
            base.OnAgentCreated(agent);


            List<string> attributeList = new List<string>();
            if(agent.IsHero) attributeList = Hero.FindFirst(x=>x.StringId == agent.Character.StringId)
            agent.GetAttributes();
            attributeList.ForEach(attribute => ApplyAgentComponentsForAttribute(attribute, agent));
        }

        private void ApplyAgentComponentsForAttribute(string attribute, Agent agent)
        {
            switch (attribute)
            {
                case "Expendable":
                    //Expendable units are handled in the mission's morale interaction logic
                    break;
                case "Undead":
                    agent.AddComponent(new UndeadMoraleAgentComponent(agent));
                    break;
            }
        }
    }
}
