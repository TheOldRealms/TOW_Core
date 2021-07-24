using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Items
{
    public class MagicWeaponAgentComponent : AgentComponent
    {
        private List<ParticleSystem> _list = new List<ParticleSystem>();
        private bool _particlesCreated;

        public MagicWeaponAgentComponent(Agent agent) : base(agent) { }

        public void OnWieldedItemChanged()
        {
            var index = Agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            var weapon = Agent.WieldedWeapon;
            if (weapon.Item != null && weapon.Item.IsMagicWeapon())
            {
                if (!_particlesCreated)
                {
                    var entity = Agent.GetWeaponEntityFromEquipmentSlot(index);
                    var skeleton = Agent.AgentVisuals.GetSkeleton();
                    var magiceffect = weapon.Item.GetMagicalProperties();
                    var offset = (magiceffect.ParticlesEndOffset - magiceffect.ParticlesStartOffset) / magiceffect.NumberOfParticleSystems;

                    for (int i = 0; i < magiceffect.NumberOfParticleSystems; i++)
                    {
                        var frame = MatrixFrame.Identity;
                        frame = frame.Elevate(offset * i);
                        _list.Add(ParticleSystem.CreateParticleSystemAttachedToEntity(magiceffect.ParticlePrefab, entity, ref frame));
                    }
                    foreach(var item in _list)
                    {
                        skeleton.AddComponentToBone(Game.Current.HumanMonster.MainHandItemBoneIndex, item);
                    }
                    _particlesCreated = true;
                }
                else
                {
                    foreach(var item in _list)
                    {
                        item.SetEnable(true);
                    }
                }
            }
            else
            {
                foreach (var item in _list)
                {
                    item.SetEnable(false);
                }
            }
        }
    }
}
