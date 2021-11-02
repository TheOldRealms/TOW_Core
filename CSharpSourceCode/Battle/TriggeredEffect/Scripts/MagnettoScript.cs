using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.Core.ItemObject;
using Timer = System.Timers.Timer;

namespace TOW_Core.Battle.TriggeredEffect.Scripts
{
    public class MagnettoScript : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents)
        {
            PutWeaponsInAir(triggeredByAgent);
            timer.AutoReset = false;
            timer.Elapsed += (s, e) => ThrowWeaponsAtTarget(triggeredByAgent, triggeredAgents.First());
            timer.Start();
        }

        private void PutWeaponsInAir(Agent caster)
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

        private void ThrowWeaponsAtTarget(Agent caster, Agent target)
        {
            if (caster.Health > 0)
            {
                foreach (var weapon in weapons)
                {
                    var pos = weapon.GameEntity.GlobalPosition;
                    var dir = target.GetEyeGlobalPosition() - pos;
                    dir.Normalize();
                    var orient = weapon.GameEntity.GetFrame().rotation;
                    var item = MBObjectManager.Instance.GetObject<ItemObject>("musket_ball");
                    Traverse.Create(item).Property("WeaponDesign").SetValue(weapon.WeaponCopy.Item.WeaponDesign);
                    var missile = new MissionWeapon(item, null, Banner.CreateRandomBanner());
                    Mission.Current.AddCustomMissile(Agent.Main, missile, pos, dir, orient, 50, 50, false, null);
                    weapon.GameEntity.FadeOut(0.1f, true);
                }
            }
            weapons.Clear();
        }


        private List<SpawnedItemEntity> weapons = new List<SpawnedItemEntity>();
        private Timer timer = new Timer(1000);
    }
}
