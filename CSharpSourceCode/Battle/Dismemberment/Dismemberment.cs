using System;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.Dismemberment
{
    public class Dismemberment
    {
        #region debug fields
        public static float velocityX = 1;
        public static float velocityY = 1;
        public static float velocityZ = 1;
        public static float angularX = 1;
        public static float angularY = 1;
        public static float angularZ = 1;
        public static float angle = 0;
        #endregion

        public static void DismemberHead(Agent victim, AttackCollisionData attackCollision)
        {
            victim.AgentVisuals.SetVoiceDefinitionIndex(-1, 0f);
            MakeHeadInvisible(victim);
            GameEntity head = SpawnHead(victim);
            GameEntity hat = SpawnHat(victim);
            AddHeadPhysics(head, attackCollision);
            AddHatPhysics(hat, attackCollision);
            CreateBloodBurst(victim);
        }
        private static void MakeHeadInvisible(Agent victim)
        {
            foreach (Mesh mesh in victim.AgentVisuals.GetEntity().Skeleton.GetAllMeshes())
            {
                bool isHeadMesh = mesh.Name.ToLower().Contains("head") || mesh.Name.ToLower().Contains("hair") || mesh.Name.ToLower().Contains("beard") || mesh.Name.ToLower().Contains("eyebrow") || mesh.Name.ToLower().Contains("helmet") || mesh.Name.ToLower().Contains("_cap_") || mesh.Name.ToLower().Contains("_hat_");
                if (isHeadMesh)
                    mesh.SetVisibilityMask((VisibilityMaskFlags)4293918720U);
            }
        }
        private static GameEntity SpawnHead(Agent victim)
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
        private static GameEntity SpawnHat(Agent victim)
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
        private static GameEntity GetHeadCopy(Agent victim)
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
        private static GameEntity GetHatCopy(Agent victim)
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
        private static void AddHeadPhysics(GameEntity head, AttackCollisionData collisionData)
        {
            Vec3 blowDir = collisionData.WeaponBlowDir;
            Vec3 velocityVec = new Vec3(blowDir.X, blowDir.Y, blowDir.Z);
            head.AddPhysics(1f, head.CenterOfMass, head.GetBodyShape(), velocityVec, Vec3.Zero, PhysicsMaterial.GetFromName("flesh"), false, -1);
            head.ApplyImpulseToDynamicBody(new Vec3(head.GlobalPosition.X, head.GlobalPosition.Y, head.GlobalPosition.Z + 0.1f), new Vec3(blowDir.X * 3, blowDir.Y * 3, blowDir.Z));
        }
        private static void AddHatPhysics(GameEntity hat, AttackCollisionData collisionData)
        {
            Vec3 blowDir = collisionData.WeaponBlowDir;
            Vec3 velocityVec = new Vec3(blowDir.X * 6, blowDir.Y * 6, blowDir.Z);
            Vec3 angularVec = new Vec3(-6, -6, angularZ);
            hat.AddPhysics(0.1f, hat.CenterOfMass, PhysicsShape.GetFromResource("bo_hatbody"), velocityVec, angularVec, PhysicsMaterial.GetFromName("flesh"), false, -1);
            hat.ApplyImpulseToDynamicBody(new Vec3(hat.GlobalPosition.X, hat.GlobalPosition.Y, hat.GlobalPosition.Z + 1f), new Vec3(blowDir.X * 0.2f, blowDir.Y * 0.2f, blowDir.Z * 0));
        }
        private static void CoverWithFlesh(Agent victim, GameEntity head)
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
        private static void CreateBloodBurst(Agent victim)
        {
            MatrixFrame boneEntitialFrameWithIndex = victim.AgentVisuals.GetSkeleton().GetBoneEntitialFrameWithIndex((byte)victim.BoneMappingArray[HumanBone.Head]);
            //Vec3 vec = victim.AgentVisuals.GetGlobalFrame().TransformToParent(boneEntitialFrameWithIndex.origin);
            Vec3 vec = victim.Position;
            vec.z = 1;
            victim.CreateBloodBurstAtLimb(13, ref vec, 0.5f + MBRandom.RandomFloat * 0.5f);
        }

        private static void MakeHeadBloodyTest(Mesh head)
        {
            head.SetMaterial(Material.GetFromResource("prt_blood_5"));
        }
    }
}
