using System.Collections.Generic;
using TOW_Core.Battle.Damage;

namespace TOW_Core.ObjectDataExtensions
{
    public static class SpellBlowInfoManager
    {
        private static Dictionary<int, SpellInfo> SpellIDs;



        public static void EnterSpellBlow(int agentIndex, string spellName, DamageType damageType)
        {
            if (agentIndex == -1)
                return;


            if (SpellIDs == null)
            {
                SpellIDs = new Dictionary<int, SpellInfo>();
            }
            
            if(SpellIDs.ContainsKey(agentIndex))
            {
                if (SpellIDs[agentIndex].SpellID == spellName)
                {
                    SpellIDs[agentIndex].Amount++;
                }

                return;

            }
            var spellItem = new SpellInfo();
            spellItem.Amount = 1;
            spellItem.SpellID = spellName;
            spellItem.DamageType = damageType;
            SpellIDs.Add(agentIndex, spellItem);
        }

        
        public static  SpellInfo GetSpellInfo(int agentIndex, bool removeAfterAccess=true)
        {
            var id = SpellIDs[agentIndex];
            if(removeAfterAccess)
                RemoveId(agentIndex);
            return id;
        }

        private static void RemoveId(int agentIndex)
        {
            if (SpellIDs.ContainsKey(agentIndex))
            {
                if (SpellIDs[agentIndex].Amount - 1 == 0)
                {
                    SpellIDs.Remove(agentIndex);
                }
                else
                {
                    SpellIDs[agentIndex].Amount--;
                }
            }
        }
        
        
    }



    public  class SpellInfo
    {
        public string SpellID;
        public DamageType DamageType;
        public int Amount;
    }
}