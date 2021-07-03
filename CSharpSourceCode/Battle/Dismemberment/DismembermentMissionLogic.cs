using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.Dismemberment
{
    public class DismembermentMissionLogic : MissionLogic
    {
        public DismembermentMissionLogic() { }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (Input.IsKeyPressed(InputKey.O))
                Dismemberment.SpawnAgent(Mission.Current.MainAgent);
        }

        public override void OnRegisterBlow(Agent attacker, Agent victim, GameEntity realHitEntity, Blow blow, ref AttackCollisionData collisionData, in MissionWeapon attackerWeapon)
        {
            base.OnRegisterBlow(attacker, victim, realHitEntity, blow, ref collisionData, attackerWeapon);

            bool isCompliant = victim.IsHuman &&
                               attacker == Mission.Current.MainAgent &&
                               victim.Health <= 0 &&
                               collisionData.VictimHitBodyPart == BoneBodyPartType.Head &&
                               collisionData.StrikeType == 0 &&
                               collisionData.DamageType == 0 &&
                               (attacker.AttackDirection == Agent.UsageDirection.AttackLeft || attacker.AttackDirection == Agent.UsageDirection.AttackRight);
            if (isCompliant)
                Dismemberment.DismemberHead(victim, collisionData);
        }
    }
}
