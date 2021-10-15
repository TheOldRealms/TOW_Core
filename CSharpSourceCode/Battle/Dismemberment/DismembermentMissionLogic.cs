using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.Dismemberment
{
    public class DismembermentMissionLogic : MissionLogic
    {
        private bool canTroopDismember = true;

        private Probability dismembermentFrequency = Probability.Always;

        private Probability slowMotionFrequency = Probability.Never;

        private float slowMotionEndTime;

        private float maxChance = 33;

        private float maxTroopChance = 10;

        private readonly String[] allMeshes = { "head", "hair", "beard", "eyebrow", "helmet", "helm_", "_bascinet", "Pothelm", "sallet", "_cap_", "_hood", "_mask", "straps", "feather", "_hat" };

        private readonly String[] headMeshes = { "head", "hair", "beard", "eyebrow" };

        public override void OnMissionTick(float dt)
        {
            if (Mission.CurrentTime >= slowMotionEndTime)
            {
                Mission.Current.Scene.SlowMotionMode = false;
            }
        }

        public override void OnRegisterBlow(Agent attacker, Agent victim, GameEntity realHitEntity, Blow blow, ref AttackCollisionData collisionData, in MissionWeapon attackerWeapon)
        {
            bool canBeDismembered = victim != null &&
                                    victim.IsHuman &&
                                    victim != Agent.Main &&
                                    victim.Health <= 0 &&
                                    victim.State == AgentState.Killed &&
                                    attacker != null &&
                                    (collisionData.VictimHitBodyPart == BoneBodyPartType.Neck ||
                                    collisionData.VictimHitBodyPart == BoneBodyPartType.Head) &&
                                    blow.DamageType == DamageTypes.Cut &&
                                    (blow.WeaponRecord.WeaponClass == WeaponClass.OneHandedAxe ||
                                    blow.WeaponRecord.WeaponClass == WeaponClass.OneHandedSword ||
                                    blow.WeaponRecord.WeaponClass == WeaponClass.TwoHandedAxe ||
                                    blow.WeaponRecord.WeaponClass == WeaponClass.TwoHandedSword) &&
                                    (attacker.AttackDirection == Agent.UsageDirection.AttackLeft ||
                                    attacker.AttackDirection == Agent.UsageDirection.AttackRight);

            if (canBeDismembered)
            {
                if (attacker == Agent.Main)
                {
                    if (ShouldBeDismembered(attacker, victim, blow))
                    {
                        DismemberHead(victim, collisionData);
                        if (slowMotionFrequency == Probability.Always)
                        {
                            EnableSlowMotion();
                        }
                        else if (slowMotionFrequency == Probability.Probably && MBRandom.RandomFloatRanged(0, 1) > 0.75f)
                        {
                            EnableSlowMotion();
                        }
                    }
                }
                else if (attacker.IsHuman && canTroopDismember)
                {
                    DismemberHead(victim, collisionData);
                }
            }
        }

        private void EnableSlowMotion()
        {
            slowMotionEndTime = Mission.CurrentTime + 0.5f;
            Mission.Current.Scene.SlowMotionMode = true;
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
            MakeHeadInvisible(victim);
            GameEntity head = SpawnHead(victim, true);
            if (!victim.IsUndead())
            {
                CoverCutWithFlesh(victim, head);
                CreateBloodBurst(victim);
            }
            AddHeadPhysics(head, attackCollision);
            //if (tag == MeshTag.SHA)
            //{
            //    var hat = GameEntity.CreateEmptyDynamic(Mission.Current.Scene, true);
            //    MatrixFrame victimFrame = new MatrixFrame(victim.LookFrame.rotation, victim.GetEyeGlobalPosition());
            //    hat.AddSphereAsBody(Vec3.Zero, 0.15f, BodyFlags.BodyOwnerEntity);
            //    hat.SetGlobalFrame(victimFrame);
            //    hat.EnableDynamicBody();
            //
            //    var headEquip = victim.Character.Equipment[EquipmentIndex.Head];
            //    if (!headEquip.IsEmpty)
            //    {
            //        MatrixFrame headLocalFrame = new MatrixFrame(Mat3.CreateMat3WithForward(in Vec3.Zero), new Vec3(0, 0, -1.6f));
            //        var multiMesh = headEquip.GetMultiMesh(victim.IsFemale, false, true).CreateCopy();
            //        multiMesh.Frame = headLocalFrame;
            //        hat.AddMultiMesh(multiMesh, true);
            //    }
            //    AddHeadPhysics(hat, attackCollision);
            //}
        }

        //This method was copied from the jedijosh920 dismemberment mod
        private void MakeHeadInvisible(Agent victim)
        {
            //MeshTag tag = MeshTag.None;
            //foreach (var mesh in victim.AgentVisuals.GetSkeleton().GetAllMeshes())
            //{
            //    foreach (String triggerName in headMeshes)
            //    {
            //        if (mesh.Name.Contains(triggerName))
            //        {
            //            mesh.SetVisibilityMask((VisibilityMaskFlags)16U);
            //        }
            //        else if (mesh.HasTag("SHA"))
            //        {
            //            mesh.SetVisibilityMask((VisibilityMaskFlags)16U);
            //            tag = MeshTag.SHA;
            //        }
            //        else if (mesh.HasTag("NSHA"))
            //        {
            //            mesh.SetVisibilityMask((VisibilityMaskFlags)16U);
            //            tag = MeshTag.NSHA;
            //        }
            //    }
            //}
            //return tag;
            foreach (var mesh in victim.AgentVisuals.GetSkeleton().GetAllMeshes())
            {
                String meshName = mesh.Name.ToLower();
                foreach (String triggerName in allMeshes)
                {
                    if (meshName.Contains(triggerName))
                    {
                        mesh.SetVisibilityMask((VisibilityMaskFlags)16U);
                    }
                }
            }
        }

        private GameEntity SpawnHead(Agent victim, bool isThereBoundHelmet)
        {
            GameEntity head = GetHeadCopy(victim, isThereBoundHelmet);
            head.AddSphereAsBody(Vec3.Zero, 0.15f, BodyFlags.BodyOwnerEntity);
            MatrixFrame victimFrame = new MatrixFrame(victim.LookFrame.rotation, victim.GetEyeGlobalPosition());
            head.SetGlobalFrame(victimFrame);
            head.EnableDynamicBody();
            return head;
        }

        private GameEntity GetHeadCopy(Agent victim, bool isThereBoundHelmet)
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
                        childMesh.SetLocalFrame(headLocalFrame);
                        head.AddMesh(childMesh, true);
                        break;
                    }
                }
            }

            if (isThereBoundHelmet)
            {
                var headEquip = victim.Character.Equipment[EquipmentIndex.Head];
                if (!headEquip.IsEmpty)
                {
                    var multiMesh = headEquip.GetMultiMesh(victim.IsFemale, false, true).CreateCopy();
                    multiMesh.Frame = headLocalFrame;
                    head.AddMultiMesh(multiMesh, true);
                }
            }
            return head;
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

        private void AddHeadPhysics(GameEntity head, AttackCollisionData collisionData)
        {
            Vec3 blowDir = collisionData.WeaponBlowDir;
            head.AddPhysics(1f, head.CenterOfMass, head.GetBodyShape(), blowDir * 2, blowDir * 10, PhysicsMaterial.GetFromName("flesh"), false, -1);
        }

        public enum Probability
        {
            Always,
            Probably,
            Never
        }

        public enum MeshTag
        {
            None,
            SHA,
            NSHA
        }
    }
}