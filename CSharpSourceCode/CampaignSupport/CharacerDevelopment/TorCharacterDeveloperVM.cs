using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TOW_Core.Abilities.SpellBook;

namespace TOW_Core.CampaignSupport.CharacerDevelopment
{
    public class TorCharacterDeveloperVM : CharacterDeveloperVM
    {
        public TorCharacterDeveloperVM(Action closeCharacterDeveloper) : base(closeCharacterDeveloper)
        {
        }

        private void ExecuteOpenSpellBook()
        {
            Game.Current.GameStateManager.PopState(0);
            var spellbookstate = Game.Current.GameStateManager.CreateState<SpellBookState>();
            Game.Current.GameStateManager.PushState(spellbookstate);
        }
    }
}
