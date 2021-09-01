using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.Dismemberment
{
    public class DismembermentMissionLogic : MissionLogic
    {
        private bool canTroopDismember = false;

        private Probability dismembermentFrequency = Probability.Always;

        private Probability slowMotionFrequency = Probability.Probably;

        private float slowMotionTimer;

        private float maxChance = 33;

        private float maxTroopChance = 10;

        private readonly String[] allMeshes = { "head", "hair", "beard", "eyebrow", "helmet", "helm_", "_bascinet", "Pothelm", "sallet", "_cap_", "_hood", "_mask", "straps", "feather", "_hat" };

        private readonly String[] headMeshes = { "head", "hair", "beard", "eyebrow" };

        private readonly String[] headDressMeshes = { "_hat" };

        private readonly String[] headArmorMeshes = { "helmet", "helm_", "_bascinet", "Pothelm", "sallet", "_cap_", "_hood", "_mask", "straps", "feather" };

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (MBCommon.TimeType.Mission.GetTime() >= slowMotionTimer)
            {
                Mission.Current.Scene.SlowMotionMode = false;
            }
        }

        public override void OnRegisterBlow(Agent attacker, Agent victim, GameEntity realHitEntity, Blow blow, ref AttackCollisionData collisionData, in MissionWeapon attackerWeapon)
        {
            base.OnRegisterBlow(attacker, victim, realHitEntity, blow, ref collisionData, attackerWeapon);
            bool canBeDismembered = victim != null &&
                                    victim.IsHuman &&
                                    victim != Agent.Main &&
                                    victim.Health <= 0 &&
                                    victim.State == AgentState.Killed &&
                                    attacker != null &&
                                    ((attacker != Agent.Main && canTroopDismember) ||
                                    attacker == Agent.Main) &&
                                    (collisionData.VictimHitBodyPart == BoneBodyPartType.Neck ||
                                    collisionData.VictimHitBodyPart == BoneBodyPartType.Head) &&
                                    blow.DamageType == DamageTypes.Cut &&
                                    (blow.WeaponRecord.WeaponClass == WeaponClass.OneHandedAxe ||
                                    blow.WeaponRecord.WeaponClass == WeaponClass.OneHandedSword ||
                                    blow.WeaponRecord.WeaponClass == WeaponClass.TwoHandedAxe ||
                                    blow.WeaponRecord.WeaponClass == WeaponClass.TwoHandedSword) &&
                                    (attacker.AttackDirection == Agent.UsageDirection.AttackLeft ||
                                    attacker.AttackDirection == Agent.UsageDirection.AttackRight);

            if (canBeDismembered && ShouldBeDismembered(attacker, victim, blow))
            {
                DismemberHead(victim, collisionData);
                if (slowMotionFrequency == Probability.Always)
                {
                    slowMotionTimer = MBCommon.TimeType.Mission.GetTime() + 0.5f;
                    Mission.Current.Scene.SlowMotionMode = true;
                }
                else if (slowMotionFrequency == Probability.Probably && MBRandom.RandomFloatRanged(0, 1) > 0.75f)
                {
                    slowMotionTimer = MBCommon.TimeType.Mission.GetTime() + 0.5f;
                    Mission.Current.Scene.SlowMotionMode = true;
                }
            }
        }

        private bool ShouldBeDismembered(Agent attacker, Agent victim, Blow blow)
        {
            if (dismembermentFrequency == Probability.Probably)
            {
                float damageModifier = blow.InflictedDamage / victim.HealthLimit;
                if (damageModifier > 1) damageModifier = 1;

                if (attacker.IsMainAgent)
                    damageModifier = damageModifier * maxChance / 100;
                else
                    damageModifier = damageModifier * maxTroopChance / 100;

                return damageModifier > MBRandom.RandomFloatRanged(0, 1);
            }
            else if (dismembermentFrequency == Probability.Always)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void DismemberHead(Agent victim, AttackCollisionData attackCollision)
        {
            victim.AgentVisuals.SetVoiceDefinitionIndex(-1, 0f);
            MakeHeadInvisible(victim);
            GameEntity head = SpawnHead(victim);
            if (!victim.IsUndead())
            {
                CoverCutWithFlesh(victim, head);
                CreateBloodBurst(victim);
            }
            GameEntity hat = SpawnHat(victim);
            AddHeadPhysics(head, attackCollision);
            if (hat != null)
                AddHeaddressPhysics(hat, attackCollision);
        }

        //This method was copied from the jedijosh920 dismemberment mod
        private void MakeHeadInvisible(Agent victim)
        {
            foreach (Mesh mesh in victim.AgentVisuals.GetEntity().Skeleton.GetAllMeshes())
            {
                String meshName = mesh.Name.ToLower();
                foreach (String triggerName in allMeshes)
                {
                    if (meshName.Contains(triggerName))
                    {
                        mesh.SetVisibilityMask((VisibilityMaskFlags)4293918720U);
                        break;
                    }
                }
            }
        }

        private GameEntity SpawnHead(Agent victim)
        {
            GameEntity head = GetHeadCopy(victim);
            head.AddSphereAsBody(Vec3.Zero, 0.15f, BodyFlags.Moveable);
            MatrixFrame victimFrame = new MatrixFrame(victim.LookFrame.rotation, victim.GetEyeGlobalPosition());
            head.SetGlobalFrame(victimFrame);
            head.SetPhysicsState(true, false);
            head.EnableDynamicBody();
            return head;
        }

        private GameEntity SpawnHat(Agent victim)
        {
            GameEntity hat = GetHeaddressCopy(victim);
            if (hat != null)
            {
                hat.AddSphereAsBody(Vec3.Zero, 0.2f, BodyFlags.Moveable);
                MatrixFrame victimFrame = new MatrixFrame(victim.LookFrame.rotation, victim.GetEyeGlobalPosition());
                victimFrame.Advance(-0.2f);
                hat.SetGlobalFrame(victimFrame);
                hat.SetPhysicsState(true, false);
                hat.EnableDynamicBody();
            }
            return hat;
        }

        private GameEntity GetHeadCopy(Agent victim)
        {
            var head = GameEntity.CreateEmptyDynamic(Mission.Current.Scene, true);
            MatrixFrame headLocalFrame = new MatrixFrame(Mat3.CreateMat3WithForward(in Vec3.Zero), new Vec3(0, 0, -1.6f));
            foreach (Mesh mesh in victim.AgentVisuals.GetSkeleton().GetAllMeshes())
            {
                String meshName = mesh.Name.ToLower();
                foreach (String name in headMeshes)
                {
                    if (meshName.Contains(name) && !meshName.Contains("lod"))
                    {
                        Mesh childMesh = mesh.GetBaseMesh().CreateCopy();
                        var child = GameEntity.CreateEmpty(Mission.Current.Scene, true);
                        childMesh.SetLocalFrame(headLocalFrame);
                        child.AddMesh(childMesh);
                        head.AddChild(child);
                        break;
                    }
                }
            }
            foreach (Mesh mesh in victim.AgentVisuals.GetSkeleton().GetAllMeshes())
            {
                if (mesh != default(Mesh))
                {
                    String meshName = mesh.Name.ToLower();
                    if (meshName.Contains("spangenhelm_a.lod0.gen") || meshName.Contains("battania_fur_helmet_a.lod0.gen"))
                    {
                        Mesh childMesh = mesh.GetBaseMesh().CreateCopy();
                        var child = GameEntity.CreateEmpty(Mission.Current.Scene, true);
                        childMesh.SetLocalFrame(headLocalFrame);
                        child.AddMesh(childMesh);
                        head.AddChild(child);
                    }
                    else
                    {
                        foreach (String name in headArmorMeshes)
                        {
                            if (meshName.Contains(name) && !meshName.Contains("lod"))
                            {
                                Mesh childMesh = mesh.GetBaseMesh().CreateCopy();
                                var child = GameEntity.CreateEmpty(Mission.Current.Scene, true);
                                childMesh.SetLocalFrame(headLocalFrame);
                                child.AddMesh(childMesh);
                                head.AddChild(child);
                                break;
                            }
                        }
                    }
                }
            }
            return head;
        }

        private GameEntity GetHeaddressCopy(Agent victim)
        {
            GameEntity hat = null;
            bool hasHeadDress = false;
            foreach (Mesh mesh in victim.AgentVisuals.GetSkeleton().GetAllMeshes())
            {
                if (!hasHeadDress)
                {
                    String meshName = mesh.Name.ToLower();
                    foreach (String name in headDressMeshes)
                    {
                        if (meshName.Contains(name) && !mesh.Name.Contains("lod"))
                        {
                            hat = GameEntity.CreateEmptyDynamic(Mission.Current.Scene, true);
                            Mesh childMesh = mesh.GetBaseMesh().CreateCopy();
                            MatrixFrame headLocalFrame = new MatrixFrame(Mat3.CreateMat3WithForward(in Vec3.Zero), new Vec3(0, 0, -1.6f));
                            var child = GameEntity.CreateEmpty(Mission.Current.Scene, true);
                            childMesh.SetLocalFrame(headLocalFrame);
                            child.AddMesh(childMesh);
                            hat.AddChild(child);
                            hasHeadDress = true;
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            return hat;
        }

        private void AddHeadPhysics(GameEntity head, AttackCollisionData collisionData)
        {
            Vec3 blowDir = collisionData.WeaponBlowDir;
            Vec3 velocityVec = new Vec3(blowDir.X, blowDir.Y, blowDir.Z);
            head.AddPhysics(1f, head.CenterOfMass, head.GetBodyShape(), velocityVec, Vec3.Zero, PhysicsMaterial.GetFromName("flesh"), false, -1);
            head.ApplyImpulseToDynamicBody(new Vec3(head.GlobalPosition.X, head.GlobalPosition.Y, head.GlobalPosition.Z + 0.1f), new Vec3(blowDir.X * 5, blowDir.Y * 5, blowDir.Z * 1.1f));
        }

        private void AddHeaddressPhysics(GameEntity hat, AttackCollisionData collisionData)
        {
            Vec3 blowDir = collisionData.WeaponBlowDir;
            Vec3 velocityVec = new Vec3(blowDir.X * 6, blowDir.Y * 6, blowDir.Z);
            Vec3 angularVec = new Vec3(-6, -6, 1);
            hat.AddPhysics(0.1f, hat.CenterOfMass, hat.GetBodyShape(), velocityVec, angularVec, PhysicsMaterial.GetFromName("flesh"), false, -1);
            hat.ApplyImpulseToDynamicBody(new Vec3(hat.GlobalPosition.X, hat.GlobalPosition.Y, hat.GlobalPosition.Z + 1f), new Vec3(blowDir.X * 0.2f, blowDir.Y * 0.2f, blowDir.Z * 0));
        }

        private void CoverCutWithFlesh(Agent victim, GameEntity head)
        {
            Mesh throatMesh = Mesh.GetFromResource("dismemberment_head_throat");
            MatrixFrame throatFrame = new MatrixFrame(Mat3.CreateMat3WithForward(in Vec3.Zero), new Vec3(0, 0, -1.6f));
            throatMesh.SetLocalFrame(throatFrame);
            var throatEntity = GameEntity.CreateEmpty(Mission.Current.Scene, true);
            throatEntity.AddMesh(throatMesh);
            head.AddChild(throatEntity);

            Mesh neckMesh = Mesh.GetFromResource("dismemberment_head_neck").CreateCopy();
            victim.AgentVisuals.GetSkeleton().AddMesh(neckMesh);
        }

        private void CreateBloodBurst(Agent victim)
        {
            MatrixFrame boneEntitialFrameWithIndex = victim.AgentVisuals.GetSkeleton().GetBoneEntitialFrameWithIndex((byte)victim.BoneMappingArray[HumanBone.Head]);
            Vec3 vec = victim.AgentVisuals.GetGlobalFrame().TransformToParent(boneEntitialFrameWithIndex.origin);
            victim.CreateBloodBurstAtLimb(13, ref vec, 0.5f + MBRandom.RandomFloat * 0.5f);
        }

        public enum Probability
        {
            Always,
            Probably,
            Never
        }
    }
}
// using TaleWorlds.Engine;
//using TaleWorlds.Library;
//using TaleWorlds.MountAndBlade;
//using TOW_Core.Utilities;
//using TOW_Core.Utilities.Extensions;
//
//namespace TOW_Core.Battle.Dismemberment
//{
//    public class DismembermentMissionLogic : MissionLogic
//    {
//        private bool isExecutionerModeOn = true;
//
//        private bool canTroopDismember = false;
//
//        private bool isSlowMotionModeOn = true;
//        private Probability dismembermentFrequency = Probability.Always;
//
//        private Probability slowMotionFrequency = Probability.Probably;
//
//        private float slowMotionTimer;
//
//@ -22,13 +21,13 @@ namespace TOW_Core.Battle.Dismemberment
//
//        private float maxTroopChance = 10;
//
//        private readonly String[] allMeshes = { "head", "hair", "beard", "eyebrow", "helmet", "helm_", "_bascinet", "Pothelm", "sallet", "_hood", "_mask", "straps", "feather", "_hat" };
//        private readonly String[] allMeshes = { "head", "hair", "beard", "eyebrow", "helmet", "helm_", "_bascinet", "Pothelm", "sallet", "_cap_", "_hood", "_mask", "straps", "feather", "_hat" };
//
//        private readonly String[] headMeshes = { "head", "hair", "beard", "eyebrow" };
//
//        private readonly String[] headDressMeshes = { "_hat" };
//
//        private readonly String[] headArmorMeshes = { "helmet", "helm_", "_bascinet", "Pothelm", "sallet", "_hood", "_mask", "straps", "feather" };
//        private readonly String[] headArmorMeshes = { "helmet", "helm_", "_bascinet", "Pothelm", "sallet", "_cap_", "_hood", "_mask", "straps", "feather" };
//
//        public override void OnMissionTick(float dt)
//        {
//@ -63,7 + 62,12 @@ namespace TOW_Core.Battle.Dismemberment
//            if (canBeDismembered && ShouldBeDismembered(attacker, victim, blow))
//            {
//                DismemberHead(victim, collisionData);
//                if (isSlowMotionModeOn)
//                if (slowMotionFrequency == Probability.Always)
//                {
//                    slowMotionTimer = MBCommon.TimeType.Mission.GetTime() + 0.5f;
//                    Mission.Current.Scene.SlowMotionMode = true;
//                }
//                else if (slowMotionFrequency == Probability.Probably && MBRandom.RandomFloatRanged(0, 1) > 0.75f)
//                {
//                    slowMotionTimer = MBCommon.TimeType.Mission.GetTime() + 0.5f;
//                    Mission.Current.Scene.SlowMotionMode = true;
//@ -73,7 +77,7 @@ namespace TOW_Core.Battle.Dismemberment
//
//        private bool ShouldBeDismembered(Agent attacker, Agent victim, Blow blow)
//    {
//        if (!isExecutionerModeOn)
//            if (dismembermentFrequency == Probability.Probably)
//            {
//                float damageModifier = blow.InflictedDamage / victim.HealthLimit;
//                if (damageModifier > 1) damageModifier = 1;
//@ -85,7 + 89,14 @@ namespace TOW_Core.Battle.Dismemberment
//
//                return damageModifier > MBRandom.RandomFloatRanged(0, 1);
//            }
//            else return true;
//            else if (dismembermentFrequency == Probability.Always)
//            {
//                return true;
//            }
//            else
//            {
//                return false;
//            }
//        }
//
//        private void DismemberHead(Agent victim, AttackCollisionData attackCollision)
//@ -269,5 + 280,12 @@ namespace TOW_Core.Battle.Dismemberment
//            Vec3 vec = victim.AgentVisuals.GetGlobalFrame().TransformToParent(boneEntitialFrameWithIndex.origin);
//victim.CreateBloodBurstAtLimb(13, ref vec, 0.5f + MBRandom.RandomFloat* 0.5f);
//        }
//
//public enum Probability
//{
//    Always,
//    Probably,
//    Never
//}
//    }
//}