using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.Dismemberment
{
    public class DismembermentMissionLogic : MissionLogic
    {
        #region debug fields
        private float velocityX = 1;
        private float velocityY = 1;
        private float velocityZ = 1;
        private float angularX = 1;
        private float angularY = 1;
        private float angularZ = 1;
        private float angle = 0;
        private float velocityOfs = 0.1f;
        private float angularOfs = 0.1f;
        private Int32 paramNumber = 1;
        #endregion

        private float slowMotionTimer;
        private bool isDebugModeOn = false;
        private float maxChance = 100;
        private float maxTroopChance = 10;

        public DismembermentMissionLogic() { }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (MBCommon.TimeType.Mission.GetTime() >= slowMotionTimer)
                Mission.Current.Scene.SlowMotionMode = false;
            Debug();
        }
        public override void OnRegisterBlow(Agent attacker, Agent victim, GameEntity realHitEntity, Blow blow, ref AttackCollisionData collisionData, in MissionWeapon attackerWeapon)
        {
            base.OnRegisterBlow(attacker, victim, realHitEntity, blow, ref collisionData, attackerWeapon);

            bool canBeDismembered = victim.IsHuman &&
                                    attacker == Mission.Current.MainAgent &&
                                    victim.Health <= 0 &&
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
            float damageModifier = blow.InflictedDamage / victim.HealthLimit;
            if (damageModifier > 1) damageModifier = 1;

            if (attacker.IsMainAgent)
                damageModifier = damageModifier * maxChance / 100;
            else
                damageModifier = damageModifier * maxTroopChance / 100;

            return damageModifier > MBRandom.RandomFloatRanged(0, 1);
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
            head.AddSphereAsBody(Vec3.Zero, 0.1f, BodyFlags.Moveable);
            MatrixFrame victimFrame = new MatrixFrame(victim.LookFrame.rotation, victim.GetEyeGlobalPosition());
            victimFrame.Advance(-0.2f);
            victimFrame.Elevate(0.02f);
            head.SetGlobalFrame(victimFrame);
            head.SetPhysicsState(true, false);
            head.AddBodyFlags(BodyFlags.AgentOnly);
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
            head.AddPhysics(1f, head.CenterOfMass, head.GetBodyShape(), velocityVec, Vec3.Zero, PhysicsMaterial.GetFromName("flesh"), false, -1);
            head.ApplyImpulseToDynamicBody(new Vec3(head.GlobalPosition.X, head.GlobalPosition.Y, head.GlobalPosition.Z + 0.1f), new Vec3(blowDir.X * 3, blowDir.Y * 3, blowDir.Z));
        }
        private void AddHatPhysics(GameEntity hat, AttackCollisionData collisionData)
        {
            Vec3 blowDir = collisionData.WeaponBlowDir;
            Vec3 velocityVec = new Vec3(blowDir.X * 6, blowDir.Y * 6, blowDir.Z);
            Vec3 angularVec = new Vec3(-6, -6, angularZ);
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
            //Vec3 vec = victim.AgentVisuals.GetGlobalFrame().TransformToParent(boneEntitialFrameWithIndex.origin);
            Vec3 vec = victim.Position;
            vec.z = 1;
            victim.CreateBloodBurstAtLimb(13, ref vec, 0.5f + MBRandom.RandomFloat * 0.5f);
        }

        private void Debug()
        {
            if (Input.IsKeyPressed(InputKey.O))
                TOWDebug.SpawnAgent(Mission.Current.MainAgent, new MatrixFrame(Mission.Current.MainAgent.LookRotation, Mission.Current.MainAgent.Position));
            else if (Input.IsKeyPressed(InputKey.Numpad0))
            {
                Blow blow = new Blow();
                Mission.Current.Teams.PlayerEnemy.ActiveAgents[0].Die(blow);
            }
            else if (Input.IsKeyPressed(InputKey.Numpad1))
            {
                paramNumber = 1;
                TOWCommon.Say("Change VelocityVec.X");
            }
            else if (Input.IsKeyPressed(InputKey.Numpad2))
            {
                paramNumber = 2;
                TOWCommon.Say("Change VelocityVec.Y");
            }
            else if (Input.IsKeyPressed(InputKey.Numpad3))
            {
                paramNumber = 3;
                TOWCommon.Say("Change VelocityVec.Z");
            }
            else if (Input.IsKeyPressed(InputKey.Numpad4))
            {
                paramNumber = 4;
                TOWCommon.Say("Change AngularVec.X");
            }
            else if (Input.IsKeyPressed(InputKey.Numpad5))
            {
                paramNumber = 5;
                TOWCommon.Say("Change AngularVec.Y");
            }
            else if (Input.IsKeyPressed(InputKey.Numpad6))
            {
                paramNumber = 6;
                TOWCommon.Say("Change AngularVec.Z");
            }
            else if (Input.IsKeyPressed(InputKey.NumpadPlus))
            {
                if (paramNumber == 1)
                {
                    velocityX += velocityOfs;
                    TOWCommon.Say($"{velocityX}");
                }
                else if (paramNumber == 2)
                {
                    velocityY += velocityOfs;
                    TOWCommon.Say($"{velocityY}");
                }
                else if (paramNumber == 3)
                {
                    velocityZ += velocityOfs;
                    TOWCommon.Say($"{velocityZ}");
                }
                else if (paramNumber == 4)
                {
                    angularX += angularOfs;
                    TOWCommon.Say($"{angularX}");
                }
                else if (paramNumber == 5)
                {
                    angularY += angularOfs;
                    TOWCommon.Say($"{angularY}");
                }
                else if (paramNumber == 6)
                {
                    angularZ += angularOfs;
                    TOWCommon.Say($"{angularZ}");
                }
            }
            else if (Input.IsKeyPressed(InputKey.NumpadMinus))
            {
                if (paramNumber == 1)
                {
                    velocityX -= velocityOfs;
                    TOWCommon.Say($"{velocityX}");
                }
                else if (paramNumber == 2)
                {
                    velocityY -= velocityOfs;
                    TOWCommon.Say($"{velocityY}");
                }
                else if (paramNumber == 3)
                {
                    velocityZ -= velocityOfs;
                    TOWCommon.Say($"{velocityZ}");
                }
                else if (paramNumber == 4)
                {
                    angularX -= angularOfs;
                    TOWCommon.Say($"{angularX}");
                }
                else if (paramNumber == 5)
                {
                    angularY -= angularOfs;
                    TOWCommon.Say($"{angularY}");
                }
                else if (paramNumber == 6)
                {
                    angularZ -= angularOfs;
                    TOWCommon.Say($"{angularZ}");
                }
            }
            else if (Input.IsKeyPressed(InputKey.Numpad7))
            {
                angle -= 5;
                TOWCommon.Say($"{angle}");
            }
            else if (Input.IsKeyPressed(InputKey.Numpad8))
            {
                angle += 5;
                TOWCommon.Say($"{angle}");
            }
        }
    }
}
