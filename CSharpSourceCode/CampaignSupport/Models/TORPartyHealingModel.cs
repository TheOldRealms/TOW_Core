using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Core;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.Models
{
    public class TORPartyHealingModel : DefaultPartyHealingModel
    {
        public override float GetSurvivalChance(PartyBase party, CharacterObject character, DamageTypes damageType, PartyBase enemyParty = null)
        {
            return character.IsUndead() ? 0f : base.GetSurvivalChance(party, character, damageType, enemyParty);
        }
    }
}
