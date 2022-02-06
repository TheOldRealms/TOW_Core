using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities
{
    public class SummonedAgentOrigin : IAgentOriginBase
    {
        private int _rank = -1;
        private UniqueTroopDescriptor _uniqueTroopDescriptor;
        public bool IsUnderPlayersCommand { get; private set; }

        public uint FactionColor { get; private set; }

        public uint FactionColor2 { get; private set; }

        public IBattleCombatant BattleCombatant { get; private set; }

        public int UniqueSeed => _uniqueTroopDescriptor.UniqueSeed;

        public int Seed => Troop.GetDefaultFaceSeed(_rank);

        public Banner Banner { get; private set; }

        public BasicCharacterObject Troop { get; private set; }

        public PartyBase OwnerParty { get; private set; }

        public SummonedAgentOrigin(Agent summoner, BasicCharacterObject summonedTroop)
        {
            Troop = summonedTroop;
            IsUnderPlayersCommand = summoner.Team.Leader == Agent.Main;
            FactionColor = summoner.Origin.FactionColor;
            FactionColor2 = summoner.Origin.FactionColor2;
            _rank = MBRandom.RandomInt(10000);
            _uniqueTroopDescriptor = new UniqueTroopDescriptor(Game.Current.NextUniqueTroopSeed);
            Banner = summoner.Team.Banner;
            OwnerParty = summoner.Team.Leader?.Origin.BattleCombatant as PartyBase;
            var manager = Mission.Current.GetMissionBehavior<AbilityManagerMissionLogic>();
            BattleCombatant = manager.GetSummoningCombatant(summoner.Team);
        }

        public void OnAgentRemoved(float agentHealth) { }

        public void OnScoreHit(BasicCharacterObject victim, BasicCharacterObject formationCaptain, int damage, bool isFatal, bool isTeamKill, WeaponComponentData attackerWeapon) { }

        public void SetBanner(Banner banner) => Banner = banner;

        public void SetKilled() { }

        public void SetRouted() { }

        public void SetWounded() { }
    }

    public class SummonedCombatant : IBattleCombatant
    {
        public TextObject Name { get; private set; }

        public BattleSideEnum Side { get; private set; }

        public BasicCultureObject BasicCulture { get; private set; }

        public BasicCharacterObject General { get; private set; }

        public Tuple<uint, uint> PrimaryColorPair { get; private set; }

        public Tuple<uint, uint> AlternativeColorPair { get; private set; }

        public Banner Banner { get; private set; }

        public SummonedCombatant(Team team, BasicCultureObject culture)
        {
            Name = new TextObject("Summoned");
            Side = team.Side;
            BasicCulture = culture;
            General = team.GeneralAgent == null ? null : team.GeneralAgent.Character;
            PrimaryColorPair = new Tuple<uint, uint>(team.Color, team.Color2);
            AlternativeColorPair = new Tuple<uint, uint>(team.Color, team.Color2);
            Banner = team.Banner;
        }

        public int GetTacticsSkillAmount() => 30;
    }
}
