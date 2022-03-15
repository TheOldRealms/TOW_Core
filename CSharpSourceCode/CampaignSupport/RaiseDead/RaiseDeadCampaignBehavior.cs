using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using TOW_Core.CampaignSupport.BattleHistory;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;
using LogLevel = NLog.LogLevel;

namespace TOW_Core.CampaignSupport.RaiseDead
{
    public class RaiseDeadCampaignBehavior : CampaignBehaviorBase
    {
        private List<CharacterObject> _raiseableCharacters = new List<CharacterObject>();
        public int LastNumberOfTroopsRaised = 0;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, InitializeRaiseableCharacters);
            CampaignEvents.OnPlayerBattleEndEvent.AddNonSerializedListener(this, RaiseDead);
        }

        private void RaiseDead(MapEvent mapEvent)
        {
            if (mapEvent.PlayerSide == mapEvent.WinningSide && Hero.MainHero.CanRaiseDead())
            {
                var troops = GenerateRaisedTroopsForVM();
                for (int i = 0; i < troops.Count; i++)
                {
                    PlayerEncounter.Current.RosterToReceiveLootMembers.AddToCounts(troops[0], 1);
                }
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        public List<CharacterObject> GenerateRaisedTroopsForVM()
        {
            List<CharacterObject> elements = new List<CharacterObject>();

            List<CharacterInfo> killedEnemies = Campaign.Current
                .GetCampaignBehavior<BattleInfoCampaignBehavior>()?
                .PlayerBattleHistory?
                .DefaultIfEmpty(new BattleInfo())
                .Last()
                .EnemiesKilled;

            //TODO: Affect raise dead chance based on main hero items/attributes/traits/perks/etc?
            double raiseDeadChance = Hero.MainHero.GetRaiseDeadChance();

            int counter = 0;

            if (killedEnemies != null)
            {
                foreach (CharacterInfo enemy in killedEnemies)
                {
                    List<CharacterObject> filteredVamps = _raiseableCharacters.Where(character => character.Level <= enemy.Level).ToList();
                    if (TOWMath.GetRandomDouble(0, 1) <= raiseDeadChance && !filteredVamps.IsEmpty())
                    {
                        var characterObject = filteredVamps.GetRandomElement();
                        if (characterObject != null)
                        {
                            elements.Add(characterObject);
                            counter++;
                        }
                        else
                        {
                            TOWCommon.Log("Null encountered when generating raise dead characters list", LogLevel.Error);
                        }
                    }
                }
            }

            LastNumberOfTroopsRaised = counter;
            return elements;
        }

        private void InitializeRaiseableCharacters(CampaignGameStarter gameStarter)
        {
            var characters = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>();
            _raiseableCharacters = characters.Where(character => character.IsUndead() && character.IsBasicTroop && character.Culture.ToString().Equals(Hero.MainHero.Culture.ToString())).ToList();
        }
    }
}
