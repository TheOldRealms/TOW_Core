﻿using SandBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
using TaleWorlds.ObjectSystem;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Utilities
{
    public class ShaderGameManager : CustomGameManager
    {
        public override void OnLoadFinished()
        {
            base.IsLoaded = true;
            LoadScene();
        }

        public override void BeginGameStart(Game game)
        {
            base.BeginGameStart(game);
            
        }

        private void LoadScene()
        {
            CustomBattleData data = new CustomBattleData();
            data.GameType = CustomBattleGameType.Battle;
            data.SceneId = "battle_terrain_a";
            data.PlayerCharacter = SelectPlayer();
            data.PlayerParty = GetPlayerParty(data.PlayerCharacter);
            data.EnemyParty = GetEnemyParty();
            data.IsPlayerGeneral = true;
            data.PlayerSideGeneralCharacter = null;
            data.SeasonId = "summer";
            data.SceneLevel = "";
            data.TimeOfDay = 12;
            CustomBattleHelper.StartGame(data);
        }

        private CustomBattleCombatant GetEnemyParty()
        {
            var culture = MBObjectManager.Instance.GetObject<BasicCultureObject>("empire");
            var enemycharacter = MBObjectManager.Instance.GetObject<BasicCharacterObject>("tor_empire_recruit");

            var party = new CustomBattleCombatant(new TextObject("{=0xC75dN6}Enemy Party", null), culture, Banner.CreateRandomBanner(-1));
            party.AddCharacter(enemycharacter, 1);
            party.Side = BattleSideEnum.Attacker;
            return party;
        }

        private CustomBattleCombatant GetPlayerParty(BasicCharacterObject playerCharacter)
        {
            var characters = MBObjectManager.Instance.GetObjectTypeList<BasicCharacterObject>();
            var culture = MBObjectManager.Instance.GetObject<BasicCultureObject>("empire");
            var characterslist = characters.Where(x => x.IsTOWTemplate() && x != playerCharacter);
            var party = new CustomBattleCombatant(new TextObject("{=!}Player Party", null), culture, Banner.CreateRandomBanner(-1));
            party.AddCharacter(playerCharacter, 1);
            party.SetGeneral(playerCharacter);
            party.Side = BattleSideEnum.Defender;
            foreach(var unit in characterslist)
            {
                int num = 1;
                if (unit.IsSoldier) num = 5;
                party.AddCharacter(unit, num);
            }
            return party;
        }

        private BasicCharacterObject SelectPlayer()
        {
            return MBObjectManager.Instance.GetObject<BasicCharacterObject>("tor_emp_lord");
        }
    }
}
