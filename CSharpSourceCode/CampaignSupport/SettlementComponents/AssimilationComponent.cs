using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.SettlementComponents
{
    public class AssimilationComponent : SettlementComponent
    {
        public AssimilationComponent()
        {
        }

        protected override void OnInventoryUpdated(ItemRosterElement item, int count)
        {
        }

        public void SetParameters(Settlement settlement)
        {
            _settlement = settlement;
            _settlement.Culture = settlement.MapFaction.Culture;

            _outsiders = new List<Hero>();
            foreach (var notable in _settlement.Notables)
            {
                if (!notable.IsSuitableForSettlement(_settlement))
                {
                    _outsiders.Add(notable);
                }
            }
            foreach (var village in _settlement.BoundVillages)
            {
                village.Settlement.Culture = _settlement.Culture;
                foreach (var notable in village.Settlement.Notables)
                {
                    if (!notable.IsSuitableForSettlement(village.Settlement))
                    {
                        _outsiders.Add(notable);
                    }
                }
            }
            _outsiders.Shuffle();

            if (_initialOutriderAmount == 0)
            {
                _initialOutriderAmount = _outsiders.Count;
            }
        }

        public void Tick()
        {
            for (byte num = 0; num < 2; num++)
            {
                var outsider = _outsiders[num];
                if (outsider != null)
                {
                    outsider.DecideNotableFate();
                }
                _outsiders.Remove(outsider);
            }
        }


        public bool IsAssimilationComplete { get => _outsiders.Count == 0; }

        public float AssimilationProgress { get => 100 - (_initialOutriderAmount * _outsiders.Count / 100); }

        public int InitialOutriderAmount { get => _initialOutriderAmount; }

        public List<Hero> Outriders { get => _outsiders; }

        public new Settlement Settlement { get => _settlement; }


        private List<Hero> _outsiders;

        [SaveableField(81)] private Settlement _settlement;

        [SaveableField(82)] private int _initialOutriderAmount;
    }
}
