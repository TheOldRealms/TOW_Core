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
        [SaveableField(0)]Minor,
        [SaveableField(1)] Entry,
        [SaveableField(2)] Adept,
        [SaveableField(3)] Master
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
