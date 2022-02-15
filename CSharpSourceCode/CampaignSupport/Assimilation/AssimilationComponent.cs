using HarmonyLib;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace TOW_Core.CampaignSupport.Assimilation
{
    public class AssimilationComponent : SettlementComponent
    {
        protected override void OnInventoryUpdated(ItemRosterElement item, int count) { }

        public void InitializeComponent(bool isFirstTime)
        {
            base.Initialize();
            if (isFirstTime)
            {
                _settlement = Settlement;
            }
            else
            {
                _settlement.SettlementComponents.AddItem(this);
            }
        }

        public void StartNewAssimilation()
        {
            _assimilationProgress = 0;
        }

        public void Tick()
        {
            ProgressAssimilation();
        }

        private void ProgressAssimilation()
        {
            //change culture of first notable from random village
            var randomVillage = _settlement.BoundVillages.GetRandomElementWithPredicate(v => v.Settlement.Culture != _settlement.Owner.Culture);
            TryToAssimilateSettlement(randomVillage.Settlement);
            //change culture of first notable from town or change settlement culture if it's castle
            if (_settlement.IsCastle)
            {
                _settlement.Culture = _settlement.Owner.Culture;
            }
            else
            {
                TryToAssimilateSettlement(_settlement);
            }
            //count amount of assimilated settlements of region
            float assimilatedCount = 0;
            float villagesCount = _settlement.BoundVillages.Count;
            for (int i = 0; i < villagesCount; i++)
            {
                var village = _settlement.BoundVillages[i].Settlement;
                if (village.Culture == _settlement.Owner.Culture) assimilatedCount++;
            }
            if (_settlement.Culture == _settlement.Owner.Culture) assimilatedCount++;
            _assimilationProgress = assimilatedCount / (villagesCount + 1f);
            if (_assimilationProgress == 1)
            {
                AssimilationIsComplete?.Invoke(this, new AssimilationIsCompleteEventArgs(_settlement, _settlement.Owner.Culture));
            }

        }

        private void TryToAssimilateSettlement(Settlement settlement)
        {
            var outriders = settlement.Notables.Where(n => n.Culture != _settlement.Owner.Culture);
            var outriderCount = outriders.Count();
            if (outriderCount > 0)
            {
                var outrider = outriders.First();
                outrider.Culture = _settlement.Owner.Culture;
                if (outriderCount == 1)
                {
                    settlement.Culture = _settlement.Owner.Culture;
                }
            }
        }


        public bool IsAssimilationComplete { get => _assimilationProgress == 1; }

        public float AssimilationProgress { get => _assimilationProgress; private set => _assimilationProgress = value; }

        public delegate void AssimilationIsCompleteEvent(object obj, AssimilationIsCompleteEventArgs e);

        public event AssimilationIsCompleteEvent AssimilationIsComplete;

        private float _assimilationProgress;

        [SaveableField(81)] public Settlement _settlement;
    }
}
