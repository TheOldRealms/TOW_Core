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
    public class HurlWeaponsScript : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents)
        {
            maxAmount = 3; // it should be related to caster skills
            PutWeaponsInAir(triggeredByAgent);
            timer.AutoReset = false;
            timer.Elapsed += (s, e) => HurlWeaponsAtTarget(triggeredByAgent, triggeredAgents.First());
            timer.Start();
        }

        private void PutWeaponsInAir(Agent caster)
        {
            var searchingSize = 15;
            Vec3 vec = caster.Position - new Vec3(searchingSize, searchingSize, 1f, -1f);
            Vec3 vec2 = caster.Position + new Vec3(searchingSize, searchingSize, 1.8f, -1f);
            UIntPtr[] itemIds = new UIntPtr[128];
            GameEntity[] entities = new GameEntity[128];
            Mission.Current.Scene.SelectEntitiesInBoxWithScriptComponent<SpawnedItemEntity>(ref vec, ref vec2, entities, itemIds);

            var material = PhysicsMaterial.GetFromName("missile");
            var amount = 0;
            for (var num = 0; num < entities.Length; num++)
            {
                var entity = entities[num];
                if (amount < maxAmount && entity != null)
                {
                    var weapon = entity.GetFirstScriptOfType<SpawnedItemEntity>();
                    if (weapon != null)
                    {
                        bool flag = weapon.WeaponCopy.Item.Type == ItemTypeEnum.OneHandedWeapon ||
                                    weapon.WeaponCopy.Item.Type == ItemTypeEnum.Polearm ||
                                    (weapon.WeaponCopy.Item.Type == ItemTypeEnum.Thrown &&
                                    weapon.WeaponCopy.GetWeaponComponentDataForUsage(0).WeaponClass != WeaponClass.Boulder) ||
                                    weapon.WeaponCopy.Item.Type == ItemTypeEnum.TwoHandedWeapon;
                        if (flag)
                        {
                            Vec3 velocity = new Vec3(0, 0, 15 / weapon.GameEntity.Mass);
                            entity.AddSphereAsBody(Vec3.Zero, 0.15f, BodyFlags.BodyOwnerEntity);
                            entity.EnableDynamicBody();
                            entity.AddPhysics(weapon.GameEntity.Mass, entity.CenterOfMass, entity.GetBodyShape(), velocity, Vec3.Zero, material, false, -1);
                            weapons.Add(weapon);
                            amount++;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void HurlWeaponsAtTarget(Agent caster, Agent target)
        {
            if (caster.Health > 0)
            {
                foreach (var weapon in weapons)
                {
                    var item = MBObjectManager.Instance.GetObject<ItemObject>("empty_missile");
                    Traverse.Create(item).Property("WeaponDesign").SetValue(weapon.WeaponCopy.Item.WeaponDesign);
                    var missile = new MissionWeapon(item, null, Banner.CreateRandomBanner());
                    var position = weapon.GameEntity.GlobalPosition;
                    var hurlDirection = target.GetEyeGlobalPosition() - position;
                    hurlDirection.Normalize();
                    var orientation = weapon.GameEntity.GetFrame().rotation;
                    var speed = 75 / weapon.GameEntity.Mass;
                    Mission.Current.AddCustomMissile(Agent.Main, missile, position, hurlDirection, orientation, 0, speed, false, null);
                    weapon.GameEntity.FadeOut(0.1f, true);
                }
            }
            weapons.Clear();
        }


        private List<SpawnedItemEntity> weapons = new List<SpawnedItemEntity>();
        private Timer timer = new Timer(1000);
        private int maxAmount = 0;
    }
}
