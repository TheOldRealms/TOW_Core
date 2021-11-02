using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.Core.ItemObject;

namespace TOW_Core.Battle.TriggeredEffect.Scripts
{
    public class MagnettoScript : ITriggeredScript
    {
        private uint? enemyColor = new Color(0.255f, 0, 0, 1f).ToUnsignedInteger();
        private List<SpawnedItemEntity> weapons = new List<SpawnedItemEntity>();
        System.Timers.Timer timer = new System.Timers.Timer(1000);

        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents)
        {
            Up(triggeredByAgent);
            timer.AutoReset = false;
            timer.Elapsed += (s, e) => Forward(triggeredAgents.First());
            timer.Start();
        }

        private void Up(Agent caster)
        {
            var num = 15;
            Vec3 vec = caster.Position - new Vec3(num, num, 1f, -1f);
            Vec3 vec2 = caster.Position + new Vec3(num, num, 1.8f, -1f);
            UIntPtr[] itemIds = new UIntPtr[128];
            GameEntity[] entities = new GameEntity[128];
            Mission.Current.Scene.SelectEntitiesInBoxWithScriptComponent<SpawnedItemEntity>(ref vec, ref vec2, entities, itemIds);
            foreach (var entity in entities)
            {
                if (entity != null)
                {
                    var weapon = entity.GetFirstScriptOfType<SpawnedItemEntity>();
                    if (weapon != null)
                    {
                        bool flag = weapon.WeaponCopy.Item.Type == ItemTypeEnum.OneHandedWeapon ||
                                    weapon.WeaponCopy.Item.Type == ItemTypeEnum.Polearm ||
                                    weapon.WeaponCopy.Item.Type == ItemTypeEnum.Thrown ||
                                    weapon.WeaponCopy.Item.Type == ItemTypeEnum.TwoHandedWeapon;
                        if (flag)
                        {
                            entity.AddSphereAsBody(Vec3.Zero, 0.15f, BodyFlags.BodyOwnerEntity);
                            entity.EnableDynamicBody();
                            Vec3 dir = new Vec3(0, 0, 12);
                            var material = PhysicsMaterial.GetFromName("flesh");
                            entity.AddPhysics(weapon.GameEntity.Mass, entity.CenterOfMass, entity.GetBodyShape(), dir, Vec3.Zero, material, false, -1);
                            weapons.Add(weapon);
                        }
                    }
                }
            }
        }
        private void Forward(Agent target)
        {
            foreach (var weapon in weapons)
            {
                var item = MBObjectManager.Instance.GetObject<ItemObject>("musket_ball");
                Traverse.Create(item).Property("WeaponDesign").SetValue(weapon.WeaponCopy.Item.WeaponDesign);
                var missile = new MissionWeapon(item, null, Banner.CreateRandomBanner());
                var pos = weapon.GameEntity.GlobalPosition;
                var dir = target.GetEyeGlobalPosition() - weapon.GameEntity.GlobalPosition;
                dir.Normalize();
                var dir2 = Agent.Main.LookDirection;
                var orient = weapon.GameEntity.GetFrame().rotation;
                Mission.Current.AddCustomMissile(Agent.Main, missile, pos, dir, orient, 50, 50, false, null);
                weapon.GameEntity.FadeOut(0.1f, true);
            }
            weapons.Clear();
        }


        public void On(Vec3 position, Agent triggeredByAgent)
        {
            IAgentOriginBase agentOrigin = triggeredByAgent.Origin;
            IMissionTroopSupplier supplier = null;
            if (Game.Current.GameType is Campaign)
            {
                supplier = Traverse.Create(agentOrigin).Field("_supplier").GetValue<PartyGroupTroopSupplier>();
            }
            var data = GetAgentBuildData(triggeredByAgent, supplier);
            SpawnAgent(data, position);
        }
        private AgentBuildData GetAgentBuildData(Agent caster, IMissionTroopSupplier supplier)
        {
            BasicCharacterObject troopCharacter = MBObjectManager.Instance.GetObject<BasicCharacterObject>("tow_summoned_skeleton");
            IAgentOriginBase troopOrigin = null;
            if (Game.Current.GameType is Campaign)
            {
                troopOrigin = new PartyAgentOrigin(MobileParty.MainParty.Party, CharacterObject.FindFirst(x => x.StringId == troopCharacter.StringId), 1, new UniqueTroopDescriptor(1), false);
            }
            else
            {
                troopOrigin = new CustomBattleAgentOrigin((CustomBattleCombatant)caster.Origin.BattleCombatant, troopCharacter, supplier as CustomBattleTroopSupplier, !caster.Team.IsEnemyOf(Mission.Current.PlayerTeam));
            }
            var formation = caster.Team.Formations.FirstOrDefault(f => f.PrimaryClass == FormationClass.Infantry);
            if (formation == null)
            {
                formation = caster.Formation;
            }
            AgentBuildData buildData = new AgentBuildData(troopCharacter).
                Team(Mission.Current.PlayerEnemyTeam).
                Formation(formation).
                ClothingColor1(caster.Team.Color).
                ClothingColor2(caster.Team.Color2).
                Equipment(troopCharacter.GetFirstEquipment(false)).
                TroopOrigin(troopOrigin).
                IsReinforcement(true).
                InitialDirection(Vec2.Forward);
            return buildData;
        }
        private Agent SpawnAgent(AgentBuildData buildData, Vec3 position)
        {
            Agent troop = Mission.Current.SpawnAgent(buildData, false, 1);
            troop.TeleportToPosition(position);
            troop.FadeIn();
            troop.SetWatchState(Agent.WatchState.Alarmed);
            return troop;
        }

    }
}
