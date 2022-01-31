using System.Collections.Generic;
using TaleWorlds.Core;
using TOW_Core.Battle.Damage;

namespace TOW_Core.ObjectDataExtensions
{
    public static class SpellBlowInfoManager
    {
        private static Dictionary<int, Queue<SpellInfo>> SpellIDs;
        private static Dictionary<int, Queue<SpellInfo>> DotIDs;
        
        
        public static void EnqueueDotInfo(int agentIndex, string dotName, DamageType damageType)
        {
            if (agentIndex == -1)
                return;
            
            if (DotIDs == null)
            {
                DotIDs = new Dictionary<int, Queue<SpellInfo>>();
            }
            
            if(DotIDs.ContainsKey(agentIndex))
            {
                SpellInfo info = new SpellInfo();
                info.SpellID = dotName;
                info.DamageType = damageType;
                DotIDs[agentIndex].Enqueue(info);
                return;
            }
            
            var spellItem = new SpellInfo();
            spellItem.SpellID = dotName;
            spellItem.DamageType = damageType;
            Queue<SpellInfo> queue = new Queue<SpellInfo>();
            queue.Enqueue(spellItem);
            DotIDs.Add(agentIndex, queue);
        }
        
        public static  SpellInfo GetDotInfo(int agentIndex)
        {
            if (!DotIDs.ContainsKey(agentIndex)) return new SpellInfo();
            var item = DotIDs[agentIndex].Dequeue();

            if (!DotIDs[agentIndex].IsEmpty())
            {
                return item;
            }

            DotIDs.Remove(agentIndex);
            return item;
        }
        
        
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