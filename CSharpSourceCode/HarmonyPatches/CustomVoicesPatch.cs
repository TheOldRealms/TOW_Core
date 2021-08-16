using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class CustomVoicesPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MissionOrderVM), "ApplySelectedOrder")]
        public static void Postfix()
        {
            //When the player gives an order, if they have custom voice lines, assign a new random voice definition
            //to prevent the same line being repeated over and over.
            Agent agent = Agent.Main;
            string agentVoiceClassName = agent?.Character?.GetCustomVoiceClassName();
            if (agentVoiceClassName != null)
            {
                agent.SetAgentVoiceByClassName(agentVoiceClassName);
            }
        }
    }
}