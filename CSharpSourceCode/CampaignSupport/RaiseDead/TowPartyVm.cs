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
using TOW_Core.CampaignSupport.BattleHistory;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

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
		private BasicTooltipViewModel _transferAllRaiseDeadHint;
		private string _raiseDeadAmountLabel;
        public TowPartyVm(Game game, PartyScreenLogic partyScreenLogic, string fiveStackShortcutkeyText, string entireStackShortcutkeyText) : base(game, partyScreenLogic, fiveStackShortcutkeyText, entireStackShortcutkeyText)
        {
			RaiseDeadTroops = new MBBindingList<PartyCharacterVM>();
			_fiveStackShortcutkeyText = fiveStackShortcutkeyText;
			_entireStackShortcutkeyText = entireStackShortcutkeyText;
			if(PartyScreenLogic != null)
            {
				InitializeRaiseDeadList(RaiseDeadTroops, PartyScreenLogic.MemberRosters[0], PartyScreenLogic.TroopType.Member, 0);
			}
			RefreshValues();
			PartyScreenLogic.UpdateDelegate += new PartyScreenLogic.PresentationUpdate(this.Update);
        }

		public void Update(PartyScreenLogic.PartyCommand command)
        {
			RaiseDeadAmountLabel = RaiseDeadAmount;
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

		public string RaiseDeadAmount
		{
			get
			{
				return _raiseDeadTroops.Select(troop => troop.Number).ToList().Sum().ToString();
			}
		}

		[DataSourceProperty]
		public string RaiseDeadAmountLabel
        {
			get
            {
				return _raiseDeadAmountLabel;
            }
			set
            {
				if (value != _raiseDeadAmountLabel)
                {
					_raiseDeadAmountLabel = value;
					base.OnPropertyChangedWithValue(value, "RaiseDeadAmountLabel");
                }
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

		[DataSourceProperty]
		public BasicTooltipViewModel TransferAllRaiseDeadHint
        {
            get
            {
				return _transferAllRaiseDeadHint;
            }
			set
            {
				if (value != this._transferAllRaiseDeadHint)
                {
					_transferAllRaiseDeadHint = value;
					base.OnPropertyChangedWithValue(value, "TransferAllRaiseDeadHint");
                }
            }
        }

		public void SetTakeAllRaiseDeadInputKey(HotKey hotKey)
		{
			this.TakeAllRaiseDeadInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
			this.TransferAllRaiseDeadHint = new BasicTooltipViewModel(delegate ()
			{
				GameTexts.SetVariable("TEXT", new TextObject("{=Srr4rOSq}Transfer All Raised Dead", null));
				GameTexts.SetVariable("HOTKEY", this.GetTransferAllOtherRaiseDeadKey());
				return GameTexts.FindText("str_hotkey_with_hint", null).ToString();
			});
		}

        public override void RefreshValues()
        {
            base.RefreshValues();
			if(RaiseDeadTroops != null)
            {
				RaiseDeadTroops.ApplyActionOnAllItems(delegate (PartyCharacterVM troopVM)
				{
					troopVM.RefreshValues();
				});
            }
        }

        private string GetTransferAllOtherRaiseDeadKey()
		{
			if (this.TakeAllRaiseDeadInputKey == null)
			{
				return string.Empty;
			}
			return this.TakeAllRaiseDeadInputKey.KeyID;
		}

		private void InitializeRaiseDeadList(MBBindingList<PartyCharacterVM> partyList, TroopRoster currentTroopRoster, PartyScreenLogic.TroopType type, int side)
        {
			//TODO: Raise dead shouldn't be determined only by culture. In the future, depending on design decisions, change this condition
			//to whatever is decided as the key to whether a hero can raise dead
			IsRaiseDeadRelevantOnCurrentMode = Hero.MainHero != null && Hero.MainHero.CanRaiseDead();

			List<FlattenedTroopRosterElement> elements = Campaign.Current.GetCampaignBehavior<RaiseDeadCampaignBehavior>().TroopsForVM;

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

		public new void ExecuteTransferWithParameters(PartyCharacterVM party, int index, string targetTag)
		{
   //         try
   //         {
			//	base.ExecuteTransferWithParameters(party, index, targetTag);
			//}
			//catch(InvalidOperationException e)
   //         {
				PartyScreenLogic.PartyRosterSide side = party.Side;
				PartyScreenLogic.PartyRosterSide partyRosterSide = targetTag.StartsWith("MainParty") ? PartyScreenLogic.PartyRosterSide.Right : PartyScreenLogic.PartyRosterSide.Left;
				if (targetTag == "MainParty")
				{
					index = -1;
				}
				else if (targetTag.EndsWith("Prisoners") != party.IsPrisoner)
				{
					index = -1;
				}
				if (side != partyRosterSide && party.IsTroopTransferrable)
				{
					OnTransferTroop(party, index, party.Number, party.Side);
					ExecuteRemoveZeroCounts();
					return;
				}
			//}
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
			PartyCharacterVM characterVM = null;
			mbbindingList = this.RaiseDeadTroops;
			if (mbbindingList == null)
			{
				return null;
			}
			else
            {
				characterVM = mbbindingList.FirstOrDefault((PartyCharacterVM x) => x.Troop.Character == character);
            }

			if(characterVM == null)
            {
				if (side == PartyScreenLogic.PartyRosterSide.Left)
				{
					mbbindingList = (isPrisoner ? this.OtherPartyPrisoners : this.OtherPartyTroops);
				}
				else if (side == PartyScreenLogic.PartyRosterSide.Right)
				{
					mbbindingList = (isPrisoner ? this.MainPartyPrisoners : this.MainPartyTroops);
				}
				if(mbbindingList == null)
				{
					return null;
				}
			}

			return mbbindingList.FirstOrDefault((PartyCharacterVM x) => x.Troop.Character == character);
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
					return GameTexts.FindText("str_party_list_label_with_weak", null).ToString(); ;
				}
				MBTextManager.SetTextVariable("PARTY_LIST_TAG", "", false);
				return GameTexts.FindText("str_party_list_label", null).ToString();
			}
			else
			{
				if (num > 0)
				{
					return GameTexts.FindText("str_party_list_label_with_weak_without_max", null).ToString(); ;
				}
				return content.ToString();
			}
		}
	}
}
