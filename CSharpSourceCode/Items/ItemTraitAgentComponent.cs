using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Items
{
    public class ItemTraitAgentComponent : AgentComponent
    {
        private List<Tuple<MissionWeapon, ItemTrait, float>> _dynamicTraits = new List<Tuple<MissionWeapon, ItemTrait, float>>();
        private List<Tuple<WeaponParticlePreset, List<ParticleSystem>, bool>> _currentPresets = new List<Tuple<WeaponParticlePreset, List<ParticleSystem>, bool>>();

        public ItemTraitAgentComponent(Agent agent) : base(agent) { }

        public override void OnTickAsAI(float dt)
        {
            base.OnTickAsAI(dt);
            if (_dynamicTraits.Count > 0)
            {
                for(int i = 0; i< _dynamicTraits.Count; i++)
                {
                    var itemTrait = _dynamicTraits[i];
                    _dynamicTraits[i] = new Tuple<MissionWeapon, ItemTrait, float>(itemTrait.Item1, itemTrait.Item2, itemTrait.Item3 - dt); 
                    if (itemTrait.Item3 < 0)
                    {
                        _dynamicTraits.RemoveAt(i);
                        UpdatePresets();
                    }
                }
            }
        }

        public void OnWieldedItemChanged()
        {
            UpdatePresets();
        }

        public void ApplyParticlePreset(WeaponParticlePreset preset, MissionWeapon weapon)
        {
            if (_currentPresets.Any(x => x.Item1 == preset))
            {
                for(int i = 0; i < _currentPresets.Count; i++)
                {
                    var tuple = _currentPresets[i];
                    if(tuple.Item1 == preset && tuple.Item2 != null && tuple.Item2.Count > 0 && tuple.Item3 == false)
                    {
                        _currentPresets[i] = new Tuple<WeaponParticlePreset, List<ParticleSystem>, bool>(tuple.Item1, tuple.Item2, true);
                    }
                }
                return;
            }
            GameEntity entity;
            var particle = TOWParticleSystem.ApplyParticleToAgentBone(Agent, preset.ParticlePrefab, Game.Current.HumanMonster.MainHandItemBoneIndex, out entity);
            List<ParticleSystem> particles = new List<ParticleSystem>();
            particles.Add(particle); ;
            _currentPresets.Add(new Tuple<WeaponParticlePreset, List<ParticleSystem>, bool>(preset, particles, true));
        }

        public void AddTraitToWieldedWeapon(ItemTrait trait, float duration)
        {
            if (trait != null && duration > 0)
            {
                var weapon = Agent.WieldedWeapon;
                if(!_dynamicTraits.Any(x => x.Item1.Item == weapon.Item))
                {
                    _dynamicTraits.Add(new Tuple<MissionWeapon, ItemTrait, float>(weapon, trait, duration));
                    UpdatePresets();
                }
            }
        }

        private void UpdatePresets()
        {
            var index = Agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            if (index == EquipmentIndex.None) return;
            for(int i = 0; i < _currentPresets.Count; i++)
            {
                var item = _currentPresets[i];
                _currentPresets[i] = new Tuple<WeaponParticlePreset, List<ParticleSystem>, bool>(item.Item1, item.Item2, false);
            }
            var weapon = Agent.WieldedWeapon;
            var data = weapon.GetWeaponStatsData();
            
            //data[0].WeaponLength
            if (weapon.Item != null && weapon.Item.HasTrait(Agent))
            {
                var info = weapon.Item.GetTorSpecificData(Agent);
                if (info != null)
                {
                    var traitsWithParticles = info.Traits.FindAll(x => x.WeaponParticlePreset != null && x.WeaponParticlePreset.ParticlePrefab != "invalid" && x.WeaponParticlePreset.ParticlePrefab != "none");
                    foreach (var trait in traitsWithParticles)
                    {
                        ApplyParticlePreset(trait.WeaponParticlePreset, weapon);
                    }
                }
            }
            RefreshVisuals();
        }

        private void RefreshVisuals()
        {
            foreach(var item in _currentPresets.Where(x => x.Item3 == true))
            {
                if(item.Item2.Count > 0)
                {
                    foreach(var ps in item.Item2)
                    {
                        ps.SetEnable(true);
                    }
                }
            }
            foreach (var item in _currentPresets.Where(x => x.Item3 == false))
            {
                if (item.Item2.Count > 0)
                {
                    foreach (var ps in item.Item2)
                    {
                        ps.SetEnable(false);
                    }
                }
            }
        }

        public List<ItemTrait> GetDynamicTraits(ItemObject itemObject)
        {
            List<ItemTrait> list = new List<ItemTrait>();
            foreach (var item in _dynamicTraits)
            {
                if(item.Item1.Item == itemObject) list.Add(item.Item2);
            }
            return list;
        }
    }
}
