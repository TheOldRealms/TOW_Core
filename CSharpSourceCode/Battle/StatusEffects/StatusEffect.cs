using System.Collections.Generic;
using System.Xml.Serialization;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;
using static TOW_Core.Utilities.TOWParticleSystem;

namespace TOW_Core.Battle.StatusEffects
{
    public class StatusEffect
    {
        private StatusEffectTemplate _template;
        public Agent ApplierAgent = null;
        public int CurrentDuration = 0;
        public StatusEffectTemplate Template => _template;

        public StatusEffect(StatusEffectTemplate template)
        {
            _template = template;
        }
    }
}