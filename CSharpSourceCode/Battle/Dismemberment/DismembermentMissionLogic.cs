using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.Dismemberment
{
    public class DismembermentMissionLogic : MissionLogic
    {
        private bool isDebugModeOn = false;
        private bool isExecutionerModeOn = true;
        private bool canTroopDismember = false;
        private float slowMotionTimer;
        private float maxChance = 100;
        private float maxTroopChance = 10;

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (MBCommon.TimeType.Mission.GetTime() >= slowMotionTimer)
                Mission.Current.Scene.SlowMotionMode = false;
            if (isDebugModeOn && Input.IsKeyPressed(InputKey.O))
                TOWDebug.SpawnAgent(Mission.Current.MainAgent);
        }

        public override void OnRegisterBlow(Agent attacker, Agent victim, GameEntity realHitEntity, Blow blow, ref AttackCollisionData collisionData, in MissionWeapon attackerWeapon)
        {
            base.OnRegisterBlow(attacker, victim, realHitEntity, blow, ref collisionData, attackerWeapon);

            bool canBeDismembered = victim != null &&
                                    attacker != null &&
                                    victim.IsHuman &&
                                    victim != Agent.Main &&
                                    victim.Health <= 0 &&
                                    ((attacker != Agent.Main && canTroopDismember) ||
                                    attacker == Agent.Main) &&
                                    (collisionData.VictimHitBodyPart == BoneBodyPartType.Neck ||
                                    collisionData.VictimHitBodyPart == BoneBodyPartType.Head) &&
                                    collisionData.StrikeType == 0 &&
                                    collisionData.DamageType == 0 &&
                                    blow.WeaponRecord.WeaponClass != WeaponClass.Dagger &&
                                    (attacker.AttackDirection == Agent.UsageDirection.AttackLeft || attacker.AttackDirection == Agent.UsageDirection.AttackRight);

            if (canBeDismembered && ShouldBeDismembered(attacker, victim, blow))
            {
                DismemberHead(victim, collisionData);
                slowMotionTimer = MBCommon.TimeType.Mission.GetTime() + 0.5f;
                Mission.Current.Scene.SlowMotionMode = true;
            }
            else if (isDebugModeOn) TOWCommon.Say("can't be dismembered");
        }

        private bool ShouldBeDismembered(Agent attacker, Agent victim, Blow blow)
        {
            if (!isExecutionerModeOn)
            {
                float damageModifier = blow.InflictedDamage / victim.HealthLimit;
                if (damageModifier > 1) damageModifier = 1;

                if (attacker.IsMainAgent)
                    damageModifier = damageModifier * maxChance / 100;
                else
                    damageModifier = damageModifier * maxTroopChance / 100;

                return damageModifier > MBRandom.RandomFloatRanged(0, 1);
            }
            else return true;
        }
        private void DismemberHead(Agent victim, AttackCollisionData attackCollision)
        {
            victim.AgentVisuals.SetVoiceDefinitionIndex(-1, 0f);
            MakeHeadInvisible(victim);
            GameEntity head = SpawnHead(victim);
            GameEntity hat = SpawnHat(victim);
            AddHeadPhysics(head, attackCollision);
            AddHatPhysics(hat, attackCollision);
            CreateBloodBurst(victim);
        }

        //This method was copied from the jedijosh920 dismemberment mod
        private void MakeHeadInvisible(Agent victim)
        {
            foreach (Mesh mesh in victim.AgentVisuals.GetEntity().Skeleton.GetAllMeshes())
            {
                bool isHeadMesh = mesh.Name.ToLower().Contains("head") || mesh.Name.ToLower().Contains("hair") || mesh.Name.ToLower().Contains("beard") || mesh.Name.ToLower().Contains("eyebrow") || mesh.Name.ToLower().Contains("helmet") || mesh.Name.ToLower().Contains("_cap_") || mesh.Name.ToLower().Contains("_hat_");
                if (isHeadMesh)
                    mesh.SetVisibilityMask((VisibilityMaskFlags)4293918720U);
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
            GameEntity hat = GetHatCopy(victim);
            hat.AddSphereAsBody(Vec3.Zero, 0.2f, BodyFlags.Moveable);
            MatrixFrame victimFrame = new MatrixFrame(victim.LookFrame.rotation, victim.GetEyeGlobalPosition());
            victimFrame.Advance(-0.2f);
            hat.SetGlobalFrame(victimFrame);
            hat.SetPhysicsState(true, false);
            hat.EnableDynamicBody();
            return hat;
        }
        private GameEntity GetHeadCopy(Agent victim)
        {
            var head = GameEntity.CreateEmptyDynamic(Mission.Current.Scene, true);
            MatrixFrame headLocalFrame = new MatrixFrame(Mat3.CreateMat3WithForward(in Vec3.Zero), new Vec3(0, 0, -1.6f));
            foreach (Mesh mesh in victim.AgentVisuals.GetSkeleton().GetAllMeshes())
            {
                if (mesh.Name.Contains("head") && !mesh.Name.Contains("lod"))
                {
                    Mesh childMesh = mesh.GetBaseMesh().CreateCopy();
                    var child = GameEntity.CreateEmpty(Mission.Current.Scene, true);
                    childMesh.SetLocalFrame(headLocalFrame);
                    child.AddMesh(childMesh);
                    head.AddChild(child);
                }
            }
            String[] meshNames = { "hair", "beard", "eyebrow", "_cap_", "helmet" };
            foreach (String name in meshNames)
            {
                Mesh mesh = victim.AgentVisuals.GetSkeleton().GetAllMeshes().FirstOrDefault(m => m.Name.Contains(name));
                if (mesh != default(Mesh))
                {
                    Mesh childMesh = mesh.GetBaseMesh().CreateCopy();
                    var child = GameEntity.CreateEmpty(Mission.Current.Scene, true);
                    childMesh.SetLocalFrame(headLocalFrame);
                    child.AddMesh(childMesh);
                    head.AddChild(child);
                }
            }
            CoverWithFlesh(victim, head);
            return head;
        }
        private GameEntity GetHatCopy(Agent victim)
        {
            var hat = GameEntity.CreateEmptyDynamic(Mission.Current.Scene, true);
            MatrixFrame headLocalFrame = new MatrixFrame(Mat3.CreateMat3WithForward(in Vec3.Zero), new Vec3(0, 0, -1.6f));
            foreach (Mesh mesh in victim.AgentVisuals.GetSkeleton().GetAllMeshes())
            {
                if (mesh.Name.Contains("_hat_") && !mesh.Name.Contains("lod"))
                {
                    Mesh childMesh = mesh.GetBaseMesh().CreateCopy();
                    var child = GameEntity.CreateEmpty(Mission.Current.Scene, true);
                    childMesh.SetLocalFrame(headLocalFrame);
                    child.AddMesh(childMesh);
                    hat.AddChild(child);
                }
            }
            return hat;
        }
        private void AddHeadPhysics(GameEntity head, AttackCollisionData collisionData)
        {
            Vec3 blowDir = collisionData.WeaponBlowDir;
            Vec3 velocityVec = new Vec3(blowDir.X, blowDir.Y, blowDir.Z);
            head.AddPhysics(1f, head.CenterOfMass + new Vec3(0, 0, 0.05f), head.GetBodyShape(), velocityVec, Vec3.Zero, PhysicsMaterial.GetFromName("flesh"), false, -1);
            head.ApplyImpulseToDynamicBody(new Vec3(head.GlobalPosition.X, head.GlobalPosition.Y, head.GlobalPosition.Z + 0.1f), new Vec3(blowDir.X * 3, blowDir.Y * 3, blowDir.Z * 1.1f));
        }
        private void AddHatPhysics(GameEntity hat, AttackCollisionData collisionData)
        {
            Vec3 blowDir = collisionData.WeaponBlowDir;
            Vec3 velocityVec = new Vec3(blowDir.X * 6, blowDir.Y * 6, blowDir.Z);
            Vec3 angularVec = new Vec3(-6, -6, 1);
            hat.AddPhysics(0.1f, hat.CenterOfMass, PhysicsShape.GetFromResource("bo_hatbody"), velocityVec, angularVec, PhysicsMaterial.GetFromName("flesh"), false, -1);
            hat.ApplyImpulseToDynamicBody(new Vec3(hat.GlobalPosition.X, hat.GlobalPosition.Y, hat.GlobalPosition.Z + 1f), new Vec3(blowDir.X * 0.2f, blowDir.Y * 0.2f, blowDir.Z * 0));
        }
        private void CoverWithFlesh(Agent victim, GameEntity head)
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
    }
}
