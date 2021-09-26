using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.ModuleManager;
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
		public List<TroopRosterElement> TroopsForVM = new List<TroopRosterElement>();
		public int LastNumberOfTroopsRaised = 0;

        public override void RegisterEvents()
        {
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

		public RaiseDeadCampaignBehavior()
        {
        }

		public List<TroopRosterElement> GenerateRaisedTroopsForVM()
        {
			if (!Hero.MainHero.CanRaiseDead())
				return new List<TroopRosterElement>();

			if (_raiseableCharacters.Count == 0)
				InitializeRaiseableCharacters();

			List<TroopRosterElement> elements = new List<TroopRosterElement>();

			List<CharacterInfo> killedEnemies = Campaign.Current
				.GetCampaignBehavior<BattleInfoCampaignBehavior>()?
				.PlayerBattleHistory?
				.DefaultIfEmpty(new BattleInfo())
				.Last()
				.EnemiesKilled;

			//TODO: Affect raise dead chance based on main hero items/attributes/traits/perks/etc?
			double raiseDeadChance = Hero.MainHero.GetRaiseDeadChance();

			int counter = 0;
			foreach (CharacterInfo enemy in killedEnemies)
			{
				List<CharacterObject> filteredVamps = _raiseableCharacters.Where(character => character.Level <= enemy.Level).ToList();
				if (TOWMath.GetRandomDouble(0, 1) <= raiseDeadChance && !filteredVamps.IsEmpty())
				{
					var characterObject = filteredVamps.GetRandomElement();
					if (characterObject != null)
					{
						elements.Add(new TroopRosterElement(characterObject));
						counter++;
					}
					else
					{
						TOWCommon.Log("Null encountered when generating raise dead characters list", LogLevel.Error);
					}
				}
			}

			LastNumberOfTroopsRaised = counter;
			return elements;
		}

		private List<CharacterObject> GetRaiseableCharacters()
		{
			var characters = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>();
			var characterlist = characters.Where(character => character.IsUndead() && character.IsBasicTroop && character.Culture.ToString().Equals(Hero.MainHero.Culture.ToString())).ToList();
			return characterlist;
		}

		private void InitializeRaiseableCharacters()
        {
			_raiseableCharacters = GetRaiseableCharacters();
        }
	}
}
