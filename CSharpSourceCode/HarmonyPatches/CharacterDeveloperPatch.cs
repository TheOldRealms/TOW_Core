using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SandBox.GauntletUI;
using TaleWorlds.Core;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class CharacterDeveloperPatch
    {
        [HarmonyPatch(typeof(GauntletCharacterDeveloperScreen), "OnActivate", MethodType.Normal)]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var foundMethod = false;
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.opcode == OpCodes.Newobj)
                {
                    var inspect = code.operand;
                }
                else if(code.opcode == OpCodes.Stfld)
                {
                    foundMethod = true;
                }
            }
            return codes.AsEnumerable();
        }
    }
}
