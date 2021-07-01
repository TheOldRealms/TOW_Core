using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.Dismemberment
{
    public class Dismemberment
    {
        private static float slowMotionTimer;
        private static Int32 randChance = 2;
        private static bool isSlowMotionOn = false;
        private static bool isDebugModeOn = false;
        private static Random rand = new Random();
        private static List<Agent> ignoredAgents = new List<Agent>();
        private static List<PotentialVictim> potentialVictims = new List<PotentialVictim>();
        private static AttackCollisionData attackCollision = new AttackCollisionData();

        public static float SlowMotionTimer
        {
            get => slowMotionTimer;
            set => slowMotionTimer = value;
        }
        public static Int32 RandChance
        {
            get => randChance;
            set => randChance = value;
        }
        public static bool IsSlowMotionOn
        {
            get => isSlowMotionOn;
            set => isSlowMotionOn = value;
        }
        public static bool IsDebugModeOn
        {
            get => isDebugModeOn;
            set => isDebugModeOn = value;
        }
        public static List<Agent> IgnoredAgents
        {
            get => ignoredAgents;
            set => ignoredAgents = value;
        } 
        public static List<PotentialVictim> PotentialVictims
        {
            get => potentialVictims;
            set => potentialVictims = value;
        }
        public static AttackCollisionData AttackCollision
        {
            get => attackCollision;
            set => attackCollision = value;
        }

        public static void AddPDV(Agent agent, Agent attacker)
        {
            if (agent.IsHuman && agent != Agent.Main)
                if (Dismemberment.rand.Next(1, Dismemberment.randChance + 1) == 1)
                    Dismemberment.potentialVictims.Add(new PotentialVictim
                    {
                        agent = agent,
                        attacker = attacker,
                        timer = MBCommon.TimeType.Mission.GetTime() + 0.0001f
                    });
        }
        public static void DismemberHead(Agent victim)
        {
            if (victim.IsActive())
                KillAgent(victim);
            Dismemberment.ignoredAgents.Add(victim);
            victim.AgentVisuals.SetVoiceDefinitionIndex(-1, 0f);

            foreach (Mesh mesh in victim.AgentVisuals.GetEntity().Skeleton.GetAllMeshes())
            {
                bool isHeadMesh = mesh.Name.ToLower().Contains("head") || mesh.Name.ToLower().Contains("hair") || mesh.Name.ToLower().Contains("beard") || mesh.Name.ToLower().Contains("eyebrow") || mesh.Name.ToLower().Contains("helmet") || mesh.Name.ToLower().Contains("_cap_");
                if (isHeadMesh)
                    mesh.SetVisibilityMask((VisibilityMaskFlags)4293918720U);
            }
            SpawnHead(victim);

            MatrixFrame boneEntitialFrameWithIndex = victim.AgentVisuals.GetSkeleton().GetBoneEntitialFrameWithIndex((byte)victim.BoneMappingArray[HumanBone.Head]);
            Vec3 vec = victim.AgentVisuals.GetGlobalFrame().TransformToParent(boneEntitialFrameWithIndex.origin);
            victim.CreateBloodBurstAtLimb(13, ref vec, 0.5f + MBRandom.RandomFloat * 0.5f);
        }
        public static Agent SpawnAgent(Agent parentAgent)
        {
            AgentBuildData agentBuildData = new AgentBuildData(parentAgent.Character);
            agentBuildData.NoHorses(true);
            agentBuildData.NoWeapons(true);
            agentBuildData.NoArmor(false);
            agentBuildData.Team(Mission.Current.PlayerEnemyTeam);
            agentBuildData.TroopOrigin(parentAgent.Origin);

            MatrixFrame frame = default(MatrixFrame);
            frame.origin = parentAgent.Position;
            frame.rotation = parentAgent.Frame.rotation;
            agentBuildData.InitialFrame(frame);

            return Mission.Current.SpawnAgent(agentBuildData, false, 0);
        }

        private static void SpawnHead(Agent victim)
        {
            GameEntity head = GetHeadCopy(victim);

            head.AddSphereAsBody(Vec3.Zero, 0.1f, BodyFlags.Moveable);
            AddPhysicsFromCollision(head);

            MatrixFrame victimFrame = new MatrixFrame(victim.LookFrame.rotation, victim.GetEyeGlobalPosition());
            victimFrame.Advance(-0.2f);
            victimFrame.Elevate(0.02f);
            head.SetGlobalFrame(victimFrame);
            head.SetPhysicsState(true, false);
            head.EnableDynamicBody();
        }
        private static GameEntity GetHeadCopy(Agent victim)
        {
            var head = GameEntity.CreateEmptyDynamic(Mission.Current.Scene, true);
            MatrixFrame headLocalFrame = new MatrixFrame(Mat3.CreateMat3WithForward(in Vec3.Zero), new Vec3(0, 0, -1.6f));

            foreach (Mesh mesh in victim.AgentVisuals.GetSkeleton().GetAllMeshes())
            {
                if (mesh.Name.Contains("head") && !mesh.Name.Contains("lod"))
                {
                    Mesh childMesh = mesh.GetBaseMesh();
                    var child = GameEntity.CreateEmpty(Mission.Current.Scene, true);
                    childMesh.SetLocalFrame(headLocalFrame);
                    child.AddMesh(childMesh);
                    head.AddChild(child);
                }
            }

            String[] meshNames = { "hair", "beard", "eyebrow", "_cap_" };
            foreach (String name in meshNames)
            {
                Mesh mesh = victim.AgentVisuals.GetSkeleton().GetAllMeshes().FirstOrDefault(m => m.Name.Contains(name));
                if (mesh != default(Mesh))
                {
                    Mesh childMesh = mesh.GetBaseMesh();
                    var child = GameEntity.CreateEmpty(Mission.Current.Scene, true);
                    childMesh.SetLocalFrame(headLocalFrame);
                    child.AddMesh(childMesh);
                    head.AddChild(child);
                }
            }
            return head;
        }
        private static void AddPhysicsFromCollision(GameEntity head)
        {
            Vec3 blowDir = AttackCollision.WeaponBlowDir;
            Vec3 velocityVec = new Vec3(blowDir.X * 3, blowDir.Y * 3);
            Vec3 angularVec = new Vec3(velocityVec.X * 3, velocityVec.Y * 3);
            head.AddPhysics(1f, head.CenterOfMass, head.GetBodyShape(), velocityVec, angularVec, PhysicsMaterial.GetFromName("flesh"), false, -1);
        }
        private static void KillAgent(Agent agent)
        {
            if (!GameNetwork.IsClientOrReplay)
            {
                Blow blow = new Blow(agent.Index);
                blow.DamageType = 0;
                blow.StrikeType = 0;
                blow.BoneIndex = agent.Monster.HeadLookDirectionBoneIndex;
                blow.Position = agent.Position;
                blow.Position.z = blow.Position.z + agent.GetEyeGlobalHeight();
                blow.BaseMagnitude = 2000f;
                blow.InflictedDamage = 2000;
                Vec3 vec = new Vec3(1f, 0f, 0f, -1f);

                if (Mission.Current.InputManager.IsGameKeyDown(2))
                    vec = new Vec3(-1f, 0f, 0f, -1f);
                else
                {
                    if (Mission.Current.InputManager.IsGameKeyDown(3))
                        vec = new Vec3(1f, 0f, 0f, -1f);
                    else
                    {
                        if (Mission.Current.InputManager.IsGameKeyDown(1))
                            vec = new Vec3(0f, -1f, 0f, -1f);
                        else
                        {
                            if (Mission.Current.InputManager.IsGameKeyDown(0))
                                vec = new Vec3(0f, 1f, 0f, -1f);
                        }
                    }
                }
                blow.SwingDirection = agent.Frame.rotation.TransformToParent(vec);
                blow.SwingDirection.Normalize();
                blow.Direction = blow.SwingDirection;
                blow.DamageCalculated = true;
                agent.RegisterBlow(blow);
            }
        }

        public static void DisplayDebugMessage(string msg)
        {
            if (Dismemberment.isDebugModeOn)
                Dismemberment.DisplayMessage("[DEBUG] " + msg, 16711680U);
        }
        private static void DisplayMessage(string msg, uint color)
        {
            InformationManager.DisplayMessage(new InformationMessage(msg, Color.FromUint(color)));
        }
    }
}
