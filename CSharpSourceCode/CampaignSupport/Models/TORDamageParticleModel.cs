using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.Models
{
    public class TORDamageParticleModel : DefaultDamageParticleModel
    {
        public override void GetMeleeAttackBloodParticles(Agent attacker, Agent victim, in Blow blow, in AttackCollisionData collisionData, out HitParticleResultData particleResultData)
        {
            if (victim.IsUndead())
            {
                particleResultData.ContinueHitParticleIndex = -1;
                particleResultData.StartHitParticleIndex = -1;
                particleResultData.EndHitParticleIndex = -1;
            }
            else
            {
                base.GetMeleeAttackBloodParticles(attacker, victim, blow, collisionData, out particleResultData);
            }
        }
        public override void GetMeleeAttackSweatParticles(Agent attacker, Agent victim, in Blow blow, in AttackCollisionData collisionData, out HitParticleResultData particleResultData)
        {
            if (victim.IsUndead())
            {
                particleResultData.ContinueHitParticleIndex = -1;
                particleResultData.StartHitParticleIndex = -1;
                particleResultData.EndHitParticleIndex = -1;
            }
            else
            {
                base.GetMeleeAttackSweatParticles(attacker, victim, blow, collisionData, out particleResultData);
            }
        }
        public override int GetMissileAttackParticle(Agent attacker, Agent victim, in Blow blow, in AttackCollisionData collisionData)
        {
            if (victim.IsUndead())
            {
                return -1;
            }
            return base.GetMissileAttackParticle(attacker, victim, blow, collisionData);
        }
    }
}
