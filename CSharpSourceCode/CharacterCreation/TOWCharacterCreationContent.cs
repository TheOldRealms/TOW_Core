using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using System.Xml.Serialization;
using TaleWorlds.Library;
using System.IO;
using TaleWorlds.CampaignSystem.GameState;
using TOW_Core.ObjectDataExtensions;
using TOW_Core.Utilities.Extensions;
using TaleWorlds.ObjectSystem;
using TOW_Core.Abilities.SpellBook;

namespace TOW_Core.CharacterCreation
{
    class TOWCharacterCreationContent : CharacterCreationContentBase
    {
        private List<CharacterCreationOption> _options;
        private int _maxStageNumber = 3;
        private List<CharacterAttribute> _attributes = new List<CharacterAttribute>();

        public TOWCharacterCreationContent()
        {
            try
            {
                var path = Path.Combine(BasePath.Name, "Modules/TOW_Core/ModuleData/tow_cc_options.xml");
                XmlSerializer ser = new XmlSerializer(typeof(List<CharacterCreationOption>));
                _options = ser.Deserialize(File.OpenRead(path)) as List<CharacterCreationOption>;
            }
            catch (Exception)
            {
                TOW_Core.Utilities.TOWCommon.Log("Failed to open tow_cc_options.xml for character creation.", NLog.LogLevel.Error);
                throw;
            }
            _attributes.Add(DefaultCharacterAttributes.Control);
            _attributes.Add(DefaultCharacterAttributes.Cunning);
            _attributes.Add(DefaultCharacterAttributes.Endurance);
            _attributes.Add(DefaultCharacterAttributes.Intelligence);
            _attributes.Add(DefaultCharacterAttributes.Social);
            _attributes.Add(DefaultCharacterAttributes.Vigor);
        }

        public override IEnumerable<Type> CharacterCreationStages
        {
            get
            {
                yield return typeof(CharacterCreationCultureStage);
                yield return typeof(CharacterCreationFaceGeneratorStage);
                yield return typeof(CharacterCreationGenericStage);
                yield return typeof(CharacterCreationOptionsStage);
                yield return typeof(CharacterCreationBannerEditorStage);
                yield return typeof(CharacterCreationClanNamingStage);
                yield return typeof(CharacterCreationReviewStage);
                yield break;
            }
        }

        public override TextObject ReviewPageDescription
        {
            get
            {
                return new TextObject("{=!}You prepare to enter The Old World! Here is your character. Click finish if you are ready, or go back to make changes.", null);
            }
        }

        protected override void OnInitialized(TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation characterCreation)
        {
            AddMenus(characterCreation);
        }

        private void AddMenus(TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation characterCreation)
        {
            //stages
            CharacterCreationMenu stage1Menu = new CharacterCreationMenu(new TextObject("{=!}Origin", null), new TextObject("{=!}Choose your family's background...", null), new CharacterCreationOnInit(OnMenuInit), CharacterCreationMenu.MenuTypes.MultipleChoice);
            CharacterCreationMenu stage2Menu = new CharacterCreationMenu(new TextObject("{=!}Growth", null), new TextObject("{=!}Teenage years...", null), new CharacterCreationOnInit(OnMenuInit), CharacterCreationMenu.MenuTypes.MultipleChoice);
            CharacterCreationMenu stage3Menu = new CharacterCreationMenu(new TextObject("{=!}Profession", null), new TextObject("{=!}Your starting profession...", null), new CharacterCreationOnInit(OnMenuInit), CharacterCreationMenu.MenuTypes.MultipleChoice);

            for(int i = 1; i <= _maxStageNumber; i++)
            {
                List<string> cultures = new List<string>();
                _options.ForEach(x =>
                {
                    if (x.StageNumber == i && !cultures.Contains(x.Culture))
                    {
                        cultures.Add(x.Culture);
                    }
                });
                foreach(var culture in cultures)
                {
                    CharacterCreationCategory category = new CharacterCreationCategory();
                    switch (i)
                    {
                        case 1:
                            category = stage1Menu.AddMenuCategory(delegate ()
                            {
                                return GetSelectedCulture().StringId == culture;
                            });
                            break;
                        case 2:
                            category = stage2Menu.AddMenuCategory(delegate ()
                            {
                                return GetSelectedCulture().StringId == culture;
                            });
                            break;
                        case 3:
                            category = stage3Menu.AddMenuCategory(delegate ()
                            {
                                return GetSelectedCulture().StringId == culture;
                            });
                            break;
                        default:
                            break;
                    }
                    
                    var relevantOptions = _options.FindAll(x => x.StageNumber == i && x.Culture.Equals(culture));
                    foreach(var option in relevantOptions)
                    {
                        var effectedSkills = new List<SkillObject>();
                        foreach(var skillId in option.SkillsToIncrease)
                        {
                            effectedSkills.Add(Skills.All.FirstOrDefault(x => x.StringId == skillId));
                        }
                        CharacterAttribute attribute = _attributes.Where(x => x.StringId == option.AttributeToIncrease.ToLower()).FirstOrDefault();
                        category.AddCategoryOption(new TextObject("{=!}"+ option.OptionText), effectedSkills, attribute, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, delegate(TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation charInfo) 
                        {
                            OnOptionSelected(charInfo, option.Id);
                        },
                        delegate (TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation charInfo)
                        {
                            OnOptionFinalize(option.Id);
                        }, 
                        new TextObject("{=!}" + option.OptionFlavourText));
                    }
                }
            }

            characterCreation.AddNewMenu(stage1Menu);
            characterCreation.AddNewMenu(stage2Menu);
            characterCreation.AddNewMenu(stage3Menu);
        }

        private void OnOptionFinalize(string id)
        {
            var selectedOption = _options.Find(x => x.Id == id);
            if(selectedOption.OptionText == "Bright Order Initiate")
            {
                Hero.MainHero.AddAttribute("AbilityUser");
                Hero.MainHero.AddAttribute("SpellCaster");
                Hero.MainHero.AddAbility("Fireball");
                Hero.MainHero.AddKnownLore("MinorMagic");
                Hero.MainHero.AddKnownLore("LoreOfFire");
                Hero.MainHero.SetSpellCastingLevel(SpellCastingLevel.Entry);
            }
            if (selectedOption.OptionText == "Warrior Priest Acolyte")
            {
                Hero.MainHero.AddAttribute("AbilityUser");
                Hero.MainHero.AddAbility("HealingAOE");
            }
            else if (selectedOption.OptionText == "Novice Necromancer")
            {
                Hero.MainHero.AddAttribute("AbilityUser");
                Hero.MainHero.AddAttribute("SpellCaster");
                Hero.MainHero.AddAttribute("Necromancer");
                Hero.MainHero.AddAbility("SummonSkeleton");
                Hero.MainHero.AddKnownLore("MinorMagic");
                Hero.MainHero.AddKnownLore("Necromancy");
                Hero.MainHero.SetSpellCastingLevel(SpellCastingLevel.Entry);
            }
            else if (selectedOption.OptionText == "Vampiric Nobility")
            {
                Hero.MainHero.AddAttribute("Undead");
                Hero.MainHero.AddAttribute("VampireBodyOverride");
                Hero.MainHero.AddAttribute("Necromancer");
                Hero.MainHero.AddAttribute("AbilityUser");
                Hero.MainHero.AddAttribute("SpellCaster");
                Hero.MainHero.AddAbility("WindOfDeath");
                Hero.MainHero.AddKnownLore("MinorMagic");
                Hero.MainHero.AddKnownLore("Necromancy");
                Hero.MainHero.SetSpellCastingLevel(SpellCastingLevel.Entry);
            }
        }

        //It is important that such a method exists, because if its null, CharacterCreationMenu.ApplyFinalEffect does not apply SkillAndAttributeEffects.
        private void OnFinalize(TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation charInfo)
        {
            return;
        }

        private void OnOptionSelected(TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation charInfo, string optionId)
        {
            var selectedOption = _options.Find(x => x.Id == optionId);
            List<Equipment> list = new List<Equipment>();
            Equipment equipment = null;
            try
            {
                equipment = MBObjectManager.Instance.GetObject<MBEquipmentRoster>(selectedOption.EquipmentSetId).DefaultEquipment;
                if(equipment == null) MBObjectManager.Instance.CreateObject<MBEquipmentRoster>(selectedOption.EquipmentSetId);
            }
            catch (NullReferenceException)
            {
                TOW_Core.Utilities.TOWCommon.Log("Attempted to read characterobject " + selectedOption.EquipmentSetId + " in Character Creation, but no such entry exists in XML. Falling back to default.", NLog.LogLevel.Error);
                throw;
            }
            if (equipment != null)
            {
                if (equipment.IsValid && !equipment.IsEmpty())
                {
                    list.Add(equipment);
                    charInfo.ChangeCharactersEquipment(list);
                    PlayerStartEquipment = equipment;
                    CharacterObject.PlayerCharacter.Equipment.FillFrom(PlayerStartEquipment);
                }
            }
        }

        private void OnMenuInit(TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation charInfo)
        {
            charInfo.IsPlayerAlone = true;
            charInfo.HasSecondaryCharacter = false;
            charInfo.ClearFaceGenMounts();
        }

        public override void OnCharacterCreationFinalized()
        {
            CultureObject culture = CharacterObject.PlayerCharacter.Culture;
            Vec2 position2D = default(Vec2);

            switch (culture.StringId)
            {
                case "empire":
                    position2D = new Vec2(1450.97f, 991.37f);
                    break;
                case "khuzait":
                    position2D = new Vec2(1617.54f, 969.70f);
                    break;
                default:
                    position2D = new Vec2(1420.97f, 981.37f);
                    break;
            }
            MobileParty.MainParty.Position2D = position2D;
            MapState mapState;
            if ((mapState = (GameStateManager.Current.ActiveState as MapState)) != null)
            {
                mapState.Handler.ResetCamera();
                mapState.Handler.TeleportCameraToMainParty();
            }
            SetHeroAge(25);
        }

        protected void SetHeroAge(float age)
        {
            Hero.MainHero.SetBirthDay(CampaignTime.YearsFromNow(-age));
        }

        private void OnChangeClanNameDone(string newClanName)
        {
            TextObject textObject = new TextObject(newClanName ?? "", null);
            Clan.PlayerClan.InitializeClan(textObject, textObject, Clan.PlayerClan.Culture, Clan.PlayerClan.Banner, default(Vec2), false);
            this.OpenBannerSelectionScreen();
        }

        // Token: 0x06002513 RID: 9491 RVA: 0x00097382 File Offset: 0x00095582
        private void OpenBannerSelectionScreen()
        {
            Game.Current.GameStateManager.PushState(Game.Current.GameStateManager.CreateState<BannerEditorState>(), 0);
        }
    }
}
