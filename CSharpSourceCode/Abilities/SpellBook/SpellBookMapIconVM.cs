using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TOW_Core.Abilities.SpellBook
{
    public class SpellBookMapIconVM : ViewModel
    {
        public SpellBookMapIconVM() { }

        public void ExecuteOpenSpellBook()
        {
            var state = Game.Current.GameStateManager.CreateState<SpellBookState>();
            Game.Current.GameStateManager.PushState(state);
        }
    }
}
