using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.Dismemberment
{
    public class DismembermentMissionLogic : MissionLogic
    {
        #region debug fields
        private static float velocityOfs = 0.1f;
        private static float angularOfs = 0.1f;
        private static Int32 paramNumber = 1;
        #endregion

        private static float slowMotionTimer;
        private static bool isDebugModeOn = false;
        private static float maxChance = 100;
        private static float maxTroopChance = 10;

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

            bool isCompliant = victim.IsHuman &&
                               attacker == Mission.Current.MainAgent &&
                               victim.Health <= 0 &&
                               (collisionData.VictimHitBodyPart == BoneBodyPartType.Neck ||
                               collisionData.VictimHitBodyPart == BoneBodyPartType.Head) &&
                               collisionData.StrikeType == 0 &&
                               collisionData.DamageType == 0 &&
                               blow.WeaponRecord.WeaponClass != WeaponClass.Dagger &&
                               (attacker.AttackDirection == Agent.UsageDirection.AttackLeft || attacker.AttackDirection == Agent.UsageDirection.AttackRight);

            if (isCompliant)
            {
                if (CanDismember(attacker, victim, blow))
                {
                    Dismemberment.DismemberHead(victim, collisionData);
                    slowMotionTimer = MBCommon.TimeType.Mission.GetTime() + 0.5f;
                    Mission.Current.Scene.SlowMotionMode = true;
                }
                else if (isDebugModeOn) DebugDisplayMessage("can not dismember");
            }
            else if (isDebugModeOn) DebugDisplayMessage("is not compliant");
        }

        private static bool CanDismember(Agent attacker, Agent victim, Blow blow)
        {
            float damageModifier = blow.InflictedDamage / victim.HealthLimit;
            if (damageModifier > 1) damageModifier = 1;

            if (attacker.IsMainAgent)
                damageModifier = damageModifier * maxChance / 100;
            else
                damageModifier = damageModifier * maxTroopChance / 100;

            return damageModifier > MBRandom.RandomFloatRanged(0, 1);
        }
        private static Agent SpawnAgent(Agent agent, MatrixFrame frame = default(MatrixFrame))
        {
            AgentBuildData agentBuildData = new AgentBuildData(agent.Character);
            agentBuildData.NoHorses(true);
            agentBuildData.NoWeapons(true);
            agentBuildData.NoArmor(false);
            agentBuildData.Team(Mission.Current.PlayerEnemyTeam);
            agentBuildData.TroopOrigin(agent.Origin);

            if (frame == default(MatrixFrame))
            {
                frame.origin = agent.Position;
                frame.rotation = agent.Frame.rotation;
            }
            agentBuildData.InitialFrame(frame);

            return Mission.Current.SpawnAgent(agentBuildData, false, 0);
        }

        public static void DebugDisplayMessage(string msg)
        {
            InformationManager.DisplayMessage(new InformationMessage($"[DEBUG] {msg}", Color.FromUint(16711680U)));
        }
        private void Debug()
        {
            if (Input.IsKeyPressed(InputKey.O))
                SpawnAgent(Mission.Current.MainAgent, new MatrixFrame(Mission.Current.MainAgent.LookRotation, Mission.Current.MainAgent.Position));
            else if (Input.IsKeyPressed(InputKey.Numpad0))
            {
                Blow blow = new Blow();
                Mission.Current.Teams.PlayerEnemy.ActiveAgents[0].Die(blow);
            }
            else if (Input.IsKeyPressed(InputKey.Numpad1))
            {
                paramNumber = 1;
                DebugDisplayMessage("Change VelocityVec.X");
            }
            else if (Input.IsKeyPressed(InputKey.Numpad2))
            {
                paramNumber = 2;
                DebugDisplayMessage("Change VelocityVec.Y");
            }
            else if (Input.IsKeyPressed(InputKey.Numpad3))
            {
                paramNumber = 3;
                DebugDisplayMessage("Change VelocityVec.Z");
            }
            else if (Input.IsKeyPressed(InputKey.Numpad4))
            {
                paramNumber = 4;
                DebugDisplayMessage("Change AngularVec.X");
            }
            else if (Input.IsKeyPressed(InputKey.Numpad5))
            {
                paramNumber = 5;
                DebugDisplayMessage("Change AngularVec.Y");
            }
            else if (Input.IsKeyPressed(InputKey.Numpad6))
            {
                paramNumber = 6;
                DebugDisplayMessage("Change AngularVec.Z");
            }
            else if (Input.IsKeyPressed(InputKey.NumpadPlus))
            {
                if (paramNumber == 1)
                {
                    Dismemberment.velocityX += velocityOfs;
                    DebugDisplayMessage($"{Dismemberment.velocityX}");
                }
                else if (paramNumber == 2)
                {
                    Dismemberment.velocityY += velocityOfs;
                    DebugDisplayMessage($"{Dismemberment.velocityY}");
                }
                else if (paramNumber == 3)
                {
                    Dismemberment.velocityZ += velocityOfs;
                    DebugDisplayMessage($"{Dismemberment.velocityZ}");
                }
                else if (paramNumber == 4)
                {
                    Dismemberment.angularX += angularOfs;
                    DebugDisplayMessage($"{Dismemberment.angularX}");
                }
                else if (paramNumber == 5)
                {
                    Dismemberment.angularY += angularOfs;
                    DebugDisplayMessage($"{Dismemberment.angularY}");
                }
                else if (paramNumber == 6)
                {
                    Dismemberment.angularZ += angularOfs;
                    DebugDisplayMessage($"{Dismemberment.angularZ}");
                }
            }
            else if (Input.IsKeyPressed(InputKey.NumpadMinus))
            {
                if (paramNumber == 1)
                {
                    Dismemberment.velocityX -= velocityOfs;
                    DebugDisplayMessage($"{Dismemberment.velocityX}");
                }
                else if (paramNumber == 2)
                {
                    Dismemberment.velocityY -= velocityOfs;
                    DebugDisplayMessage($"{Dismemberment.velocityY}");
                }
                else if (paramNumber == 3)
                {
                    Dismemberment.velocityZ -= velocityOfs;
                    DebugDisplayMessage($"{Dismemberment.velocityZ}");
                }
                else if (paramNumber == 4)
                {
                    Dismemberment.angularX -= angularOfs;
                    DebugDisplayMessage($"{Dismemberment.angularX}");
                }
                else if (paramNumber == 5)
                {
                    Dismemberment.angularY -= angularOfs;
                    DebugDisplayMessage($"{Dismemberment.angularY}");
                }
                else if (paramNumber == 6)
                {
                    Dismemberment.angularZ -= angularOfs;
                    DebugDisplayMessage($"{Dismemberment.angularZ}");
                }
            }
            else if (Input.IsKeyPressed(InputKey.Numpad7))
            {
                Dismemberment.angle -= 5;
                DebugDisplayMessage($"{Dismemberment.angle}");
            }
            else if (Input.IsKeyPressed(InputKey.Numpad8))
            {
                Dismemberment.angle += 5;
                DebugDisplayMessage($"{Dismemberment.angle}");
            }
        }
    }
}
