using System.Collections.Generic;
using TaleWorlds.Core;
using TOW_Core.Battle.Damage;

namespace TOW_Core.ObjectDataExtensions
{
    public static class SpellBlowInfoManager
    {
        private static Dictionary<int, Queue<SpellInfo>> SpellIDs;
        
        public static void EnqueueSpellInfo(int agentIndex, string spellName, DamageType damageType)
        {
            if (agentIndex == -1)
                return;
            
            if (SpellIDs == null)
            {
                SpellIDs = new Dictionary<int, Queue<SpellInfo>>();
            }
            
            if(SpellIDs.ContainsKey(agentIndex))
            {
                SpellInfo info = new SpellInfo();
                info.SpellID = spellName;
                info.DamageType = damageType;
                SpellIDs[agentIndex].Enqueue(info);
                return;
            }
            
            var spellItem = new SpellInfo();
            spellItem.SpellID = spellName;
            spellItem.DamageType = damageType;
            Queue<SpellInfo> queue = new Queue<SpellInfo>();
            queue.Enqueue(spellItem);
            SpellIDs.Add(agentIndex, queue);
        }
        
        public static  SpellInfo GetSpellInfo(int agentIndex)
        {
            if (!SpellIDs.ContainsKey(agentIndex)) return new SpellInfo();
            
            var item = SpellIDs[agentIndex].Dequeue();

            if (!SpellIDs[agentIndex].IsEmpty())
            {
                return item;
            }

            SpellIDs.Remove(agentIndex);

            return item;
        }
    }
    
    public  struct SpellInfo
    {
        public string SpellID;
        public DamageType DamageType;
    }
}