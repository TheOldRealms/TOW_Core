using SandBox.ViewModelCollection.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using TOW_Core.Utilities;

namespace TOW_Core.CampaignSupport.RaiseDead
{
    public class TowPartyVm : PartyVM
    {
		private MBBindingList<PartyCharacterVM> _raiseDeadTroops;
		private InputKeyItemVM _takeAllRaiseDeadInputKey;
		private string _fiveStackShortcutkeyText;
		private string _entireStackShortcutkeyText;
		private List<string> _lockedCharacterIDs;
		private bool _mainHeroIsUndead;
        public TowPartyVm(Game game, PartyScreenLogic partyScreenLogic, string fiveStackShortcutkeyText, string entireStackShortcutkeyText) : base(game, partyScreenLogic, fiveStackShortcutkeyText, entireStackShortcutkeyText)
        {
			RaiseDeadTroops = new MBBindingList<PartyCharacterVM>();
			_fiveStackShortcutkeyText = fiveStackShortcutkeyText;
			_entireStackShortcutkeyText = entireStackShortcutkeyText;
			if(PartyScreenLogic != null)
            {
				InitializeRaiseDeadList(RaiseDeadTroops, PartyScreenLogic.MemberRosters[0], PartyScreenLogic.TroopType.Prisoner, 0);
				PopulatePartyListLabel(this.RaiseDeadTroops, this.RaiseDeadTroops.Count);
			}
			RefreshValues();
        }

		[DataSourceProperty]
		public MBBindingList<PartyCharacterVM> RaiseDeadTroops
		{
			get
			{
				return this._raiseDeadTroops;
			}
			set
			{
				if (value != this._raiseDeadTroops)
				{
					this._raiseDeadTroops = value;
					base.OnPropertyChangedWithValue(value, "RaiseDeadTroops");
				}
			}
		}

		[DataSourceProperty]
		public InputKeyItemVM TakeAllRaiseDeadInputKey
		{
			get
			{
				return this._takeAllRaiseDeadInputKey;
			}
			set
			{
				if (value != this._takeAllRaiseDeadInputKey)
				{
					this._takeAllRaiseDeadInputKey = value;
					base.OnPropertyChangedWithValue(value, "TakeAllRaiseDeadInputKey");
				}
			}
		}

		[DataSourceProperty]
		public string RaiseDeadAmount
		{
			get
			{
				return _raiseDeadTroops.Count.ToString();
			}
		}

		[DataSourceProperty]
		public bool IsRaiseDeadRelevantOnCurrentMode
		{
			get
			{
				return _mainHeroIsUndead;
			}
			set
			{
				if (value != this._mainHeroIsUndead)
				{
					_mainHeroIsUndead = value;
					base.OnPropertyChangedWithValue(value, "IsRaiseDeadRelevantOnCurrentMode");
				}
			}
		}

		public void SetTakeAllRaiseDeadInputKey(HotKey hotKey)
		{
			this.TakeAllRaiseDeadInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
			this.TransferAllOtherPrisonersHint = new BasicTooltipViewModel(delegate ()
			{
				GameTexts.SetVariable("TEXT", new TextObject("{=Srr4rOSq}Transfer All Raised Dead", null));
				GameTexts.SetVariable("HOTKEY", this.GetTransferAllOtherRaiseDeadKey());
				return GameTexts.FindText("str_hotkey_with_hint", null).ToString();
			});
		}

		private string GetTransferAllOtherRaiseDeadKey()
		{
			if (this.TakeAllRaiseDeadInputKey == null)
			{
				return string.Empty;
			}
			return this.TakeAllRaiseDeadInputKey.KeyID;
		}

		private List<CharacterObject> GetVampireCharacters()
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
					output.Add(charObj);
				}
			}
			return output;
		}

		private void InitializeRaiseDeadList(MBBindingList<PartyCharacterVM> partyList, TroopRoster currentTroopRoster, PartyScreenLogic.TroopType type, int side)
        {
			this.IsRaiseDeadRelevantOnCurrentMode = Hero.MainHero != null && Hero.MainHero.Culture.ToString().Equals("Vampire Counts");
			List<FlattenedTroopRosterElement> elements = new List<FlattenedTroopRosterElement>();
			//for each enemy that died, 90% chance to continue, 10% chance to create an undead unit of the same level or lower
			List<CharacterObject> vampireCharacters = GetVampireCharacters();
			for(int i=0; i<10; i++)
            {
				List<CharacterObject> filteredVamps = vampireCharacters.Where(character => character.Level < 12).ToList();
				if(TOWMath.GetRandomInt(0, 10) > 0)
                {
					elements.Add(new FlattenedTroopRosterElement(filteredVamps.GetRandomElement()));
                }
            }
			IPartyTroopLockTracker campaignBehavior = Campaign.Current.GetCampaignBehavior<IPartyTroopLockTracker>();
			_lockedCharacterIDs = campaignBehavior.GetLocks().ToList<string>();
			partyList.Clear();
			currentTroopRoster.Clear();
			currentTroopRoster.Add(elements);
			for (int i = 0; i < currentTroopRoster.Count; i++)
			{
				TroopRosterElement elementCopyAtIndex = currentTroopRoster.GetElementCopyAtIndex(i);
				PartyCharacterVM partyCharacterVM = new PartyCharacterVM(PartyScreenLogic, new Action<PartyCharacterVM, bool>(this.ProcessCharacterLock), new Action<PartyCharacterVM>(this.SetSelectedCharacter), new Action<PartyCharacterVM, int, int, PartyScreenLogic.PartyRosterSide>(this.OnTransferTroop), null, this, currentTroopRoster, i, type, (PartyScreenLogic.PartyRosterSide)side, this.PartyScreenLogic.IsTroopTransferable(type, elementCopyAtIndex.Character, side), this._fiveStackShortcutkeyText, this._entireStackShortcutkeyText);
				partyList.Add(partyCharacterVM);
				partyCharacterVM.ThrowOnPropertyChanged();
				partyCharacterVM.IsLocked = (partyCharacterVM.Side == PartyScreenLogic.PartyRosterSide.Right && this.IsTroopLocked(partyCharacterVM.Troop));
			}
			partyList.Add(OtherPartyPrisoners[0]);
		}

		private void ProcessCharacterLock(PartyCharacterVM troop, bool isLocked)
		{
			if (isLocked && !this._lockedCharacterIDs.Contains(troop.StringId))
			{
				this._lockedCharacterIDs.Add(troop.StringId);
				return;
			}
			if (!isLocked && this._lockedCharacterIDs.Contains(troop.StringId))
			{
				this._lockedCharacterIDs.Remove(troop.StringId);
			}
		}

		private void SetSelectedCharacter(PartyCharacterVM troop)
		{
			this.CurrentCharacter = troop;
			this.CurrentCharacter.UpdateRecruitable();
		}

		private void SaveCharacterLockStates()
		{
			Campaign.Current.GetCampaignBehavior<IPartyTroopLockTracker>().SetLocks(this._lockedCharacterIDs);
		}

		private bool IsTroopLocked(TroopRosterElement troop)
		{
			return this._lockedCharacterIDs.Contains(troop.Character.StringId);
		}

		private void OnTransferTroop(PartyCharacterVM troop, int newIndex, int transferAmount, PartyScreenLogic.PartyRosterSide fromSide)
		{
			if (troop.Side == PartyScreenLogic.PartyRosterSide.None || fromSide == PartyScreenLogic.PartyRosterSide.None)
			{
				return;
			}
			PartyScreenLogic.PartyRosterSide side = troop.Side;
			this.SetSelectedCharacter(troop);
			PartyScreenLogic.PartyCommand partyCommand = new PartyScreenLogic.PartyCommand();
			if (transferAmount > 0)
			{
				int numberOfHealthyTroopNumberForSide = this.GetNumberOfHealthyTroopNumberForSide(troop.Troop.Character, fromSide, troop.IsPrisoner);
				int numberOfWoundedTroopNumberForSide = this.GetNumberOfWoundedTroopNumberForSide(troop.Troop.Character, fromSide, troop.IsPrisoner);
				if ((this.PartyScreenLogic.TransferHealthiesGetWoundedsFirst && fromSide == PartyScreenLogic.PartyRosterSide.Right) || (!this.PartyScreenLogic.TransferHealthiesGetWoundedsFirst && fromSide == PartyScreenLogic.PartyRosterSide.Left))
				{
					int num = (transferAmount <= numberOfHealthyTroopNumberForSide) ? 0 : (transferAmount - numberOfHealthyTroopNumberForSide);
					num = (int)MathF.Clamp((float)num, 0f, (float)numberOfWoundedTroopNumberForSide);
					partyCommand.FillForTransferTroop(fromSide, troop.Type, troop.Character, transferAmount, num, newIndex);
				}
				else
				{
					partyCommand.FillForTransferTroop(fromSide, troop.Type, troop.Character, transferAmount, (numberOfWoundedTroopNumberForSide >= transferAmount) ? transferAmount : numberOfWoundedTroopNumberForSide, newIndex);
				}
				this.PartyScreenLogic.AddCommand(partyCommand);
			}
		}

		private int GetNumberOfHealthyTroopNumberForSide(CharacterObject character, PartyScreenLogic.PartyRosterSide fromSide, bool isPrisoner)
		{
			PartyCharacterVM partyCharacterVM = this.FindCharacterVM(character, fromSide, isPrisoner);
			return partyCharacterVM.Troop.Number - partyCharacterVM.Troop.WoundedNumber;
		}

		private int GetNumberOfWoundedTroopNumberForSide(CharacterObject character, PartyScreenLogic.PartyRosterSide fromSide, bool isPrisoner)
		{
			return this.FindCharacterVM(character, fromSide, isPrisoner).WoundedCount;
		}

		private PartyCharacterVM FindCharacterVM(CharacterObject character, PartyScreenLogic.PartyRosterSide side, bool isPrisoner)
		{
			MBBindingList<PartyCharacterVM> mbbindingList = null;
			if (side == PartyScreenLogic.PartyRosterSide.Left)
			{
				mbbindingList = (isPrisoner ? this.OtherPartyPrisoners : this.OtherPartyTroops);
			}
			else if (side == PartyScreenLogic.PartyRosterSide.Right)
			{
				mbbindingList = (isPrisoner ? this.MainPartyPrisoners : this.MainPartyTroops);
			}
			if (mbbindingList == null)
			{
				return null;
			}
			return mbbindingList.First((PartyCharacterVM x) => x.Troop.Character == character);
		}

		private static string PopulatePartyListLabel(MBBindingList<PartyCharacterVM> partyList, int limit = 0)
		{
			int content = partyList.Sum((PartyCharacterVM item) => Math.Max(0, item.Number - item.WoundedCount));
			int num = partyList.Sum(delegate (PartyCharacterVM item)
			{
				if (item.Number < item.WoundedCount)
				{
					return 0;
				}
				return item.WoundedCount;
			});
			MBTextManager.SetTextVariable("COUNT", content);
			MBTextManager.SetTextVariable("WEAK_COUNT", num);
			if (limit != 0)
			{
				MBTextManager.SetTextVariable("MAX_COUNT", limit);
				if (num > 0)
				{
					MBTextManager.SetTextVariable("PARTY_LIST_TAG", "", false);
					MBTextManager.SetTextVariable("WEAK_COUNT", num);
					string blah = GameTexts.FindText("str_party_list_label_with_weak", null).ToString();
					return blah;
				}
				MBTextManager.SetTextVariable("PARTY_LIST_TAG", "", false);
				return GameTexts.FindText("str_party_list_label", null).ToString();
			}
			else
			{
				if (num > 0)
				{
					string blah = GameTexts.FindText("str_party_list_label_with_weak_without_max", null).ToString();
					return blah;
				}
				return content.ToString();
			}
		}
	}
}
