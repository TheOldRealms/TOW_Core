using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Utilities
{
    /// <summary>
    /// Provides particle-specific functions for use cases that are expected to be reused
    /// throughout the code base.
    /// </summary>
    public class TOWParticleSystem
    {
        /// <summary>
        /// Attach a particle to an agent. Returns the list of ParticleSystems added so the caller
        /// can modify and manage the added particles.
        /// </summary>
        /// <param name="agent">The agent receiving the particle system.</param>
        /// <param name="particleId">The ID of the particle system.</param>
        /// <param name="intensity">Affects the number of bones the particle attaches to</param>
        /// <param name="childEntities">A list of child entities. These game entities hold the particle systems, and the agent holds the entities.</param>
        /// <returns>A List of ParticleSystems attached to the agent</returns>
        public static List<ParticleSystem> ApplyParticleToAgent(Agent agent, string particleId, out List<GameEntity> childEntities, ParticleIntensity intensity = ParticleIntensity.High)
        {
            List<ParticleSystem> particleList = new List<ParticleSystem>();
            childEntities = new List<GameEntity>();
            if (intensity == ParticleIntensity.Undefined)
            {
                TOWCommon.Log("Attempted to give an agent a particle with undefined intensity.", LogLevel.Warn);
            }
            else
            {
                int[] boneIndexes = { 0, 1, 2, 3, 5, 6, 7, 9, 12, 13, 15, 17, 22, 24 };
                for (byte i = 0; i < boneIndexes.Length / (int)intensity; i++)
                {
                    GameEntity childEntity;
                    ParticleSystem particle = ApplyParticleToAgentBone(agent, particleId, (sbyte)boneIndexes[i], out childEntity);
                    particleList.Add(particle);
                    childEntities.Add(childEntity);
                }
            }
            
            return particleList;
        }

        /// <summary>
        /// Attach a particle to an agent's bone at the given index.
        /// </summary>
        /// <param name="agent">The agent receiving the particle system.</param>
        /// <param name="particleId">The ID of the particle system.</param>
        /// <param name="boneIndex">The index of the bone on the agent's skeleton that the particle should be attached to.</param>
        /// <param name="childEntity">The child entity that the particle is attached to.</param>
        /// <returns>The ParticleSystem that was attached to the agent's bone.</returns>
        public static ParticleSystem ApplyParticleToAgentBone(Agent agent, string particleId, sbyte boneIndex, out GameEntity childEntity)
        {
            Skeleton skeleton = agent.AgentVisuals.GetSkeleton();
            Scene scene = Mission.Current.Scene;
            childEntity = GameEntity.CreateEmpty(scene);
            MatrixFrame localFrame = new MatrixFrame(Mat3.Identity, new Vec3(0, 0, 0));
            ParticleSystem particle = ParticleSystem.CreateParticleSystemAttachedToEntity(particleId, childEntity, ref localFrame);
            if(particle != null)
            {
                agent.AgentVisuals.AddChildEntity(childEntity);
                skeleton.AddComponentToBone(boneIndex, particle);
            }
            else
            {
                TOWCommon.Log("Attempted to apply a null particle to agent bone. Particle ID: " + particleId + ". Agent name: " + agent.Name, LogLevel.Warn);
            }
            
            return particle;
        }

        public enum ParticleIntensity
        {
            Undefined,
            High,
            Medium,
            Low = 14
        }
    }
}
