using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.SaveSystem;

namespace TOW_Core.Abilities.SpellBook
{
    public enum SpellCastingLevel
    {
        [SaveableField(0)] None,
        [SaveableField(1)] Minor,
        [SaveableField(2)] Entry,
        [SaveableField(3)] Adept,
        [SaveableField(4)] Master
    }

    public class SpellCastingTypeDefiner : SaveableTypeDefiner
    {
        public SpellCastingTypeDefiner() : base(1_143_199)
        {

        }
        protected override void DefineEnumTypes()
        {
            base.DefineEnumTypes();
            AddEnumDefinition(typeof(SpellCastingLevel), 1);
        }

    }
}
