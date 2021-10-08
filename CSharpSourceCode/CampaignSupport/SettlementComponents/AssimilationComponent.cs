using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace TOW_Core.CampaignSupport
{
    public class AssimilationComponent : SettlementComponent
    {
        public AssimilationComponent()
        {
        }

        protected override void OnInventoryUpdated(ItemRosterElement item, int count)
        {
        }

        public void Tick()
        {
            if (_assimilationProgress < 100)
            {
                if (_assimilationProgressRate == 0)
                {
                    if (Settlement.IsTown)
                    {
                        _assimilationProgressRate = 33 / Settlement.Town.Villages.Count + 1;
                    }
                    else
                    {
                        _assimilationProgressRate = 33 / Settlement.Town.Villages.Count;
                    }
                }
                _assimilationProgress += _assimilationProgressRate;
                if (_assimilationProgress > 50)
                {
                    Settlement.Culture = Settlement.MapFaction.Culture;
                }
                else if (_assimilationProgress > 66)
                {
                    var village = Settlement.Town.Villages.FirstOrDefault(v => v.Settlement.Culture != Settlement.Culture);
                    if (village != null)
                    {
                        village.Settlement.Culture = Settlement.Culture;
                    }
                }
            }
        }

        public void Reset()
        {
            _assimilationProgress = 0;
            _assimilationProgressRate = 0;
        }


        public bool IsAssimilationComplete { get => _assimilationProgress >= 100; }

        private float _assimilationProgress;

        private float _assimilationProgressRate;
    }
}
