using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;
using TOW_Core.Utilities;
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
            //_oldCulture = Campaign.Current.Factions.FirstOrDefault(f => f.Culture == _settlement.Culture).Culture;
            _newCulture = _settlement.MapFaction.Culture;
            if (_settlement.IsCastle)
            {
                _settlement.Culture = _newCulture;
                _settlementsToAssimilate = _settlement.BoundVillages.Count;
            }
            else
            {
                _settlementsToAssimilate = _settlement.BoundVillages.Count + 1;
            }
            UpdateCulture();
        }

        public void Tick()
        {
            if (_settlement.IsCastle)
            {
                foreach (var village in _settlement.BoundVillages)
                {
                    village.Settlement.Notables.FirstOrDefault(n => n.IsOutrider(_newCulture)).DecideNotableFate();
                }
            }
            else if (_settlement.IsTown)
            {
                if (GetOutriderCoefficient(_settlement) <= 0.5f)
                {
                    foreach (var village in _settlement.BoundVillages)
                    {
                        village.Settlement.Notables.FirstOrDefault(n => n.IsOutrider(_newCulture)).DecideNotableFate();
                    }
                }
                _settlement.Notables.FirstOrDefault(n => n.IsOutrider(_newCulture)).DecideNotableFate();
            }
            UpdateCulture();
            TOWCommon.Say($"{_settlement.Name}'s area - {_assimilationProgress}");
        }

        private void UpdateCulture()
        {
            _assimilationProgress = 0;
            float _setCoef;
            if (_settlement.IsTown)
            {
                _setCoef = GetOutriderCoefficient(_settlement);
                _assimilationProgress += _setCoef;
                if (_setCoef <= 0.5f)
                {
                    _settlement.Culture = _newCulture;
                }
            }

            foreach (var village in _settlement.BoundVillages)
            {
                float _vilCoef = GetOutriderCoefficient(village.Settlement);
                _assimilationProgress += _vilCoef;
                if (_vilCoef <= 0.5f)
                {
                    village.Settlement.Culture = _newCulture;
                }
            }

            _assimilationProgress = 1 - (_assimilationProgress / _settlementsToAssimilate);
        }

        private float GetOutriderCoefficient(Settlement settlement)
        {
            return (float)settlement.Notables.Where(n => n.IsOutrider(_newCulture)).Count() / (float)settlement.Notables.Count;
        }


        public bool IsAssimilationComplete { get => _assimilationProgress == 1; }

        public float AssimilationProgress { get => _assimilationProgress; }

        public int SettlementsToAssimilate { get => _settlementsToAssimilate; }

        public CultureObject NewCulture { get => _newCulture; }

        public new Settlement Settlement { get => _settlement; }


        private int _settlementsToAssimilate;

        private float _assimilationProgress;

        [SaveableField(81)] private Settlement _settlement;

        [SaveableField(82)] private CultureObject _newCulture;

        //[SaveableField(83)] private CultureObject _oldCulture;
    }
}
