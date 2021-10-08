using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;
using TOW_Core.Utilities;

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
            if (_settlement.IsCastle)
            {
                _settlement.Culture = _settlement.MapFaction.Culture;
            }
            if (_settlement.IsTown)
            {
                _assimilationProgressRate = 33 / Settlement.BoundVillages.Count + 1;
            }
            else
            {
                _assimilationProgressRate = 33 / Settlement.BoundVillages.Count;
            }
        }

        public void Tick()
        {
            _assimilationProgress += _assimilationProgressRate;
            TOWCommon.Say($"{Settlement.Name} + {_assimilationProgressRate} = {_assimilationProgress}");

            if (_assimilationProgress > 50)
            {
                Settlement.Culture = Settlement.MapFaction.Culture;
            }
            if (_assimilationProgress > 66)
            {
                var village = Settlement.BoundVillages.FirstOrDefault(v => v.Settlement.Culture != Settlement.Culture);
                if (village != null)
                {
                    village.Settlement.Culture = Settlement.Culture;
                    _assimilatedVillageAmount++;
                }
            }
        }

        public void UpdateCulture()
        {
            if ((_settlement.IsTown && _assimilationProgress > 50) || _settlement.IsCastle)
            {
                _settlement.Culture = _settlement.MapFaction.Culture;
            }
            if (_assimilationProgress >= 100)
            {
                foreach (var village in Settlement.BoundVillages)
                {
                    village.Settlement.Culture = _settlement.MapFaction.Culture;
                }
            }
            else
            {
                for (int b = 0; b < _assimilatedVillageAmount; b++)
                {
                    _settlement.BoundVillages[b].Settlement.Culture = Settlement.MapFaction.Culture;
                }
            }
        }

        public void Reset()
        {
            _assimilationProgress = 0;
            _assimilationProgressRate = 0;
            _assimilatedVillageAmount = 0;
        }


        public bool IsAssimilationComplete { get => _assimilationProgress >= 100; }

        public new Settlement Settlement { get => _settlement; set => _settlement = value; }


        [SaveableField(81)] private float _assimilationProgress;

        [SaveableField(82)] private float _assimilationProgressRate;

        [SaveableField(83)] private int _assimilatedVillageAmount;

        [SaveableField(84)] private Settlement _settlement;
    }
}
