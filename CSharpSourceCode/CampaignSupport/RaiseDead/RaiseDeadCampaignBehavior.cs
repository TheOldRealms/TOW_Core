using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.ModuleManager;
using TOW_Core.Battle.Extensions;
using TOW_Core.CampaignSupport.BattleHistory;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.RaiseDead
{
    public class RaiseDeadCampaignBehavior : CampaignBehaviorBase
    {
		private List<CharacterObject> _raiseableCharacters = new List<CharacterObject>();
		public List<FlattenedTroopRosterElement> TroopsForVM = new List<FlattenedTroopRosterElement>();

        public override void RegisterEvents()
        {
			CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, new Action(InitializeRaiseableCharacters));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

		public RaiseDeadCampaignBehavior()
        {
        }

		public List<FlattenedTroopRosterElement> GenerateRaisedTroopsForVM()
        {
			List<FlattenedTroopRosterElement> elements = new List<FlattenedTroopRosterElement>();

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
				if (TOWMath.GetRandomDouble(0, 1) <= raiseDeadChance)
				{
					elements.Add(new FlattenedTroopRosterElement(filteredVamps.GetRandomElement()));
					counter++;
				}
			}

			TOWCommon.Say("Raised " + counter + " troops from the dead.");
			return elements;
		}

		private List<CharacterObject> GetRaiseableCharacters()
		{
			List<CharacterObject> output = new List<CharacterObject>();

			var files = Directory.GetFiles(ModuleHelper.GetModuleFullPath("TOW_Core"), "tow_troopdefinitions_vc.xml", SearchOption.AllDirectories);
			foreach (var file in files)
			{
				XmlDocument characterXml = new XmlDocument();
				characterXml.Load(file);
				XmlNodeList characters = characterXml.GetElementsByTagName("NPCCharacter");

				foreach (XmlNode character in characters)
				{
					CharacterObject charObj = new CharacterObject();
					charObj.Deserialize(Game.Current.ObjectManager, character);
					//TODO: Only add if the unit is Undead. Can do once the ExtensionContainers are added for (Basic)CharacterObjects. i.e. if(charObj.IsUndead())
					output.Add(charObj);
				}
			}
			return output;
		}

		private void InitializeRaiseableCharacters()
        {
			_raiseableCharacters = GetRaiseableCharacters();
        }
	}
}
