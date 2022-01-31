using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TOW_Core.Battle.Damage;

namespace TOW_Core.ObjectDataExtensions
{
    public static class SpellBlowInfoManager
    {
        private static Dictionary<Tuple<int,int>, Queue<SpellInfo>> SpellIDs = new Dictionary<Tuple<int, int>, Queue<SpellInfo>>();

        public static void EnqueueSpellInfo(int victimAgentIndex, int attackAgentIndex, string spellName, DamageType damageType)
        {
            if (victimAgentIndex == -1 || attackAgentIndex ==-1)
                return;

           

            var coord = new Tuple<int, int>(victimAgentIndex, attackAgentIndex);
            
            if(SpellIDs.ContainsKey(coord))
            {
                SpellInfo info = new SpellInfo();
                info.SpellID = spellName;
                info.DamageType = damageType;
                info.DamagerIndex = attackAgentIndex;
                SpellIDs[coord].Enqueue(info);
                return;
            }
            
            var spellItem = new SpellInfo();
            spellItem.SpellID = spellName;
            spellItem.DamageType = damageType;
            Queue<SpellInfo> queue = new Queue<SpellInfo>();
            queue.Enqueue(spellItem);
            SpellIDs.Add(coord, queue);
        }
        
        public static  SpellInfo GetSpellInfo(int victimAgentIndex, int attackAgentIndex)
        {
            var coord = new Tuple<int, int>(victimAgentIndex, attackAgentIndex);
            if (!SpellIDs.ContainsKey(coord)) return new SpellInfo();
            
            var item = SpellIDs[coord].Dequeue();

            if (!SpellIDs[coord].IsEmpty())
            {
                return item;
            }
            
            return item;
        }
    }
    
    public  struct SpellInfo
    {
        public int DamagerIndex;
        public string SpellID;
        public DamageType DamageType;
    }
}