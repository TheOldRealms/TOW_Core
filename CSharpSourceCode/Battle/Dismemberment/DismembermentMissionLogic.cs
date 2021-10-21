using System;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TOW_Core.Utilities;
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

        private readonly String[] headMeshes = { "head", "hair", "beard", "eyebrow" };

        public override void OnMissionTick(float dt)
        {
            if (Mission.CurrentTime >= slowMotionEndTime)
            {
                Mission.Current.Scene.SlowMotionMode = false;
            }
            if (Agent.Main != null)
                TOWCommon.Say($"{Agent.Main.WalkSpeedCached} {Agent.Main.Velocity} {Agent.Main.MovementVelocity}");
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

            bool test = victim != null &&
                        victim.IsHuman &&
                        victim != Agent.Main &&
                        attacker != null;

            if (test)
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
            GameEntity head = CopyHead(victim);
            MatrixFrame headFrame = new MatrixFrame(victim.LookFrame.rotation, victim.GetEyeGlobalPosition());
            head.SetGlobalFrame(headFrame);

            float weight = 0;
            var headEquipment = victim.SpawnEquipment[EquipmentIndex.Head];
            if (!headEquipment.IsEmpty)
            {
                MeshTag tag;
                weight = headEquipment.Weight;
                var headArmor = CopyHeadArmor(victim, headEquipment, out tag);
                if (tag == MeshTag.SHA)
                {
                    headArmor.SetGlobalFrame(headFrame);
                    AddPhysics(headArmor, attackCollision, weight);
                }
                else
                {
                    head.AddChild(headArmor);
                }
            }
            AddPhysics(head, attackCollision, 1.5f + weight);
            if (!victim.IsUndead())
            {
                CoverCutWithFlesh(victim, head);
                CreateBloodBurst(victim);
            }
        }

        private GameEntity CopyHead(Agent victim)
        {
            GameEntity head = GameEntity.CreateEmptyDynamic(Mission.Current.Scene, true);
            MatrixFrame headLocalFrame = new MatrixFrame(Mat3.CreateMat3WithForward(in Vec3.Zero), new Vec3(0, 0, -1.6f));
            var meshes = victim.AgentVisuals.GetSkeleton().GetAllMeshes();
            foreach (Mesh mesh in meshes)
            {
                String meshName = mesh.Name.ToLower();
                foreach (String name in headMeshes)
                {
                    bool flag = false;
                    if (meshName.Contains(name))
                    {
                        mesh.SetVisibilityMask((VisibilityMaskFlags)16U);
                        if (!flag)
                        {
                            Mesh childMesh = mesh.GetBaseMesh().CreateCopy();
                            childMesh.SetLocalFrame(headLocalFrame);
                            head.AddMesh(childMesh, true);
                            flag = true;
                            break;
                        }
                    }
                }
            }
            return head;
        }

        private GameEntity CopyHeadArmor(Agent victim, EquipmentElement equipment, out MeshTag tag)
        {
            tag = MeshTag.NSHA;
            var headArmor = GameEntity.CreateEmptyDynamic(Mission.Current.Scene, true);
            MatrixFrame headMeshFrame = new MatrixFrame(Mat3.CreateMat3WithForward(in Vec3.Zero), new Vec3(0, 0, -1.6f));
            var multiMesh = equipment.GetMultiMesh(victim.IsFemale, false, true);
            var meshes = victim.AgentVisuals.GetSkeleton().GetAllMeshes();
            for (int i = 0; i < multiMesh.MeshCount; i++)
            {
                var equipMesh = multiMesh.GetMeshAtIndex(i);
                var agentMesh = meshes.FirstOrDefault(m => m.Name == equipMesh.Name);
                agentMesh.SetVisibilityMask(VisibilityMaskFlags.ShadowStatic);
            }
            if (multiMesh.GetFirstMeshWithTag("SHA") != null)
            {
                tag = MeshTag.SHA;
            }
            multiMesh = multiMesh.CreateCopy();
            headArmor.AddMultiMesh(multiMesh, true);
            multiMesh.Frame = headMeshFrame;
            return headArmor;
        }

        private void AddPhysics(GameEntity entity, AttackCollisionData collisionData, float weight = 1)
        {
            entity.AddSphereAsBody(Vec3.Zero, 0.15f, BodyFlags.BodyOwnerEntity);
            entity.EnableDynamicBody();
            Vec3 blowDir = collisionData.WeaponBlowDir;
            entity.AddPhysics(weight, entity.CenterOfMass, entity.GetBodyShape(), blowDir * 2, blowDir * 10, PhysicsMaterial.GetFromName("flesh"), false, -1);
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

        public enum MeshTag
        {
            None,
            SHA, //Separate head armor
            NSHA //Non separate head armor (hat, cap, little helmet, etc)
        }
    }
}