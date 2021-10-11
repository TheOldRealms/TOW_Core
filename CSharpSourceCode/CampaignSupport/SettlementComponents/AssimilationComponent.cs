using HarmonyLib;
using SandBox.View.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;
using TOW_Core.Utilities.Extensions;
using static TaleWorlds.CampaignSystem.Hero;

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
            _assimilationProgress = 0;
            _settlement = settlement;
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
            var wanderers = _settlement.HeroesWithoutParty.Where(h => h.Occupation == Occupation.Wanderer).ToArray();
            for (int i = 0; i < wanderers.Length; i++)
            {
                this.DecideWandererFate(wanderers[i]);
            }
            CalculateConversion();
        }

        public void Tick()
        {
            if (_settlement.IsCastle)
            {
                foreach (var village in _settlement.BoundVillages)
                {
                    var villageOutrider = village.Settlement.Notables.FirstOrDefault(n => n.IsOutrider(_newCulture));
                    this.DecideNotableFate(villageOutrider);
                }
            }
            else if (_settlement.IsTown)
            {
                if (GetOutriderCoefficient(_settlement) <= 0.5f)
                {
                    foreach (var village in _settlement.BoundVillages)
                    {
                        var villageOutrider = village.Settlement.Notables.FirstOrDefault(n => n.IsOutrider(_newCulture));
                        this.DecideNotableFate(villageOutrider);
                    }
                }
                var outrider = _settlement.Notables.FirstOrDefault(n => n.IsOutrider(_newCulture));
                this.DecideNotableFate(outrider);
            }
            CalculateConversion();
        }

        private void CalculateConversion()
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
            if (IsAssimilationComplete)
            {
                AssimilationIsComplete?.Invoke(this, new AssimilationIsCompleteEventArgs(_settlement, _newCulture));
            }
        }

        private float GetOutriderCoefficient(Settlement settlement)
        {
            return (float)settlement.Notables.Where(n => n.IsOutrider(_newCulture)).Count() / (float)settlement.Notables.Count;
        }

        private void DecideWandererFate(Hero hero)
        {
            if (hero != null && hero.Culture != _newCulture)
            {
                if (MBRandom.RandomFloat >= 0.25f)
                {
                    List<ValueTuple<Settlement, float>> list = new List<ValueTuple<Settlement, float>>(Settlement.All.Count);
                    foreach (Settlement settlement in Settlement.All.Where(s => s.IsSuitableForHero(hero)))
                    {
                        list.Add(new ValueTuple<Settlement, float>(settlement, GetMoveScoreForCompanion(hero)));
                    }
                    var nextSettlement = (from x in list
                                          orderby x.Item2 descending
                                          select x).Take(5).GetRandomElementInefficiently<ValueTuple<Settlement, float>>().Item1;
                    if (nextSettlement != null)
                    {
                        LeaveSettlementAction.ApplyForCharacterOnly(hero);
                        EnterSettlementAction.ApplyForCharacterOnly(hero, nextSettlement);
                    }
                    else
                    {
                        KillCharacterAction.ApplyByMurder(hero, null, false);
                    }
                }
                else
                {
                    KillCharacterAction.ApplyByMurder(hero, null, false);
                }
            }
        }

        private float GetMoveScoreForCompanion(Hero companion)
        {
            float num = 0f;
            if (_settlement.IsTown)
            {
                num = 1E-06f;
                if (!FactionManager.IsAtWarAgainstFaction(_settlement.Party.MapFaction, companion.MapFaction))
                {
                    num += (_settlement == companion.HomeSettlement) ? 25f : 0f;
                    num += FactionManager.IsNeutralWithFaction(_settlement.Party.MapFaction, companion.MapFaction) ? 50f : 100f;
                    Settlement settlement2 = companion.LastSeenPlace ?? companion.HomeSettlement;
                    Vec2 v = new Vec2((settlement2.Position2D.x + MobileParty.MainParty.Position2D.x) / 2f, (settlement2.Position2D.y + MobileParty.MainParty.Position2D.y) / 2f);
                    float num2 = _settlement.Position2D.Distance(v);
                    if (num2 <= 50f)
                    {
                        num += 150f;
                    }
                    else if (num2 <= 100f)
                    {
                        num += 100f;
                    }
                    else if (num2 <= 150f)
                    {
                        num += 50f;
                    }
                    else
                    {
                        num += 10f;
                    }
                    if (num <= 60f)
                    {
                        num = 1f;
                    }
                }
            }
            return num;
        }

        private void DecideNotableFate(Hero hero)
        {
            if (hero != null && hero.HeroState != CharacterStates.Dead && _newCulture != hero.Culture)
            {
                if (hero.IsVampireNotable())
                {
                    KillCharacterAction.ApplyByMurder(hero, null, false);
                }
                else if (hero.IsEmpireNotable())
                {
                    if (_newCulture.Name.Contains("Vampire"))
                    {
                        hero.TurnIntoVampire();
                        if (!hero.CharacterObject.IsFemale)
                        {
                            if (hero.CurrentSettlement == Hero.MainHero.CurrentSettlement)
                            {
                                this.ReplaceImageIdentifier(hero.CharacterObject);
                            }
                        }
                        return;
                    }
                }
                if (hero.HeroState == CharacterStates.Dead)
                {
                    CreateNewNotable(hero);
                }
            }
        }

        private void CreateNewNotable(Hero victim)
        {
            Hero newNotable = HeroCreator.CreateRelativeNotableHero(victim);
            EnterSettlementAction.ApplyForCharacterOnly(newNotable, newNotable.CurrentSettlement);
            if (newNotable.CurrentSettlement.IsEmpireSettlement())
            {
                newNotable.TurnIntoHuman();
            }
            else if (newNotable.CurrentSettlement.IsVampireSettlement())
            {
                newNotable.TurnIntoVampire();
            }
            foreach (Hero otherHero in Hero.AllAliveHeroes)
            {
                int relation = victim.GetRelation(otherHero);
                if (relation != 0)
                {
                    newNotable.SetPersonalRelation(otherHero, relation);
                }
            }
            if (victim.Issue != null)
            {
                Campaign.Current.IssueManager.ChangeIssueOwner(victim.Issue, newNotable);
            }

            using (List<CaravanPartyComponent>.Enumerator enumerator = victim.OwnedCaravans.ToList<CaravanPartyComponent>().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    CaravanPartyComponent caravanPartyComponent = enumerator.Current;
                    CaravanPartyComponent.TransferCaravanOwnership(caravanPartyComponent.MobileParty, newNotable);
                }
            }
        }

        private void ReplaceImageIdentifier(CharacterObject character)
        {
            MapScreen mapScreen = MapScreen.Instance;
            var overlayVM = (SettlementMenuOverlayVM)Traverse.Create(mapScreen).Field("_menuViewContext").Field("_menuOverlayBase").Field("_overlayDataSource").GetValue();
            if (overlayVM != null)
            {
                var target = overlayVM.CharacterList.FirstOrDefault(ch => ch.Character == character);
                if (target != null)
                {
                    var charCode = CharacterCode.CreateFrom(character);
                    var newImageIdentifier = new ImageIdentifierVM(charCode);
                    target.Visual = newImageIdentifier;
                }
            }
        }


        public bool IsAssimilationComplete { get => _assimilationProgress == 1; }

        public float AssimilationProgress { get => _assimilationProgress; }

        public CultureObject NewCulture { get => _newCulture; }

        public new Settlement Settlement { get => _settlement; }

        public delegate void AssimilationIsCompleteEvent(object obj, AssimilationIsCompleteEventArgs e);

        public event AssimilationIsCompleteEvent AssimilationIsComplete;


        private int _settlementsToAssimilate;

        private float _assimilationProgress;


        [SaveableField(81)] private Settlement _settlement;

        [SaveableField(82)] private CultureObject _newCulture;
    }

    public class AssimilationIsCompleteEventArgs
    {
        public AssimilationIsCompleteEventArgs(Settlement settlement, CultureObject culture)
        {
            this._settlement = settlement;
            this._culture = culture;
        }

        public Settlement Settlement { get => _settlement; }

        public CultureObject Culture { get => _culture; }

        private Settlement _settlement;

        private CultureObject _culture;
    }
}
