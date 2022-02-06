using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;

namespace TOW_Core.Abilities.SpellBook
{
    public class SpellBookVM : ViewModel
    {
        private Action _closeAction;

        public SpellBookVM(Action closeAction)
        {
            _closeAction = closeAction;
        }

        private void ExecuteClose()
        {
            _closeAction();
        }
    }
}
