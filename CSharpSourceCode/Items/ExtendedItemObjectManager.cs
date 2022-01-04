using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Items
{
    public static class ExtendedItemObjectManager
    {
        private static Dictionary<string, ExtendedItemObjectProperties> _itemToInfoMap = new Dictionary<string, ExtendedItemObjectProperties>();
        private static string XMLPath = Path.Combine(BasePath.Name, "Modules/TOW_Core/ModuleData/tow_extendeditemproperties_test.xml");


        internal static ExtendedItemObjectProperties GetAdditionalProperties(string itemId)
        {
            ExtendedItemObjectProperties info = null;
            _itemToInfoMap.TryGetValue(itemId, out info);
            return info;
        }

        public static void LoadXML()
        {
            if (File.Exists(XMLPath))
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<ExtendedItemObjectProperties>));
                List<ExtendedItemObjectProperties> list = ser.Deserialize(File.OpenRead(XMLPath)) as List<ExtendedItemObjectProperties>;
                if(list != null && list.Count > 0)
                {
                    foreach(var item in list)
                    {
                        _itemToInfoMap.Add(item.ItemStringId, item);
                    }
                }
            }
        }

        internal static void WriteXML()
        {
            var trait1 = new ItemTrait();
            trait1.ItemTraitName = "Flaming Sword";
            trait1.ItemTraitDescription = "This sword is on fire. It deals fire damage and applies the burning damage over time effect. Bonus fire damage: 10%";
            trait1.ImbuedStatusEffectId = "fireball_dot";
            trait1.WeaponParticlePreset = new WeaponParticlePreset { ParticlePrefab = "magic_sparks", ParticlesStartOffset = 0.5f, ParticlesEndOffset = 1.2f, NumberOfParticleSystems = 1 };
            trait1.OnHitScriptName = "none";
            trait1.OffenseProperty = new ObjectDataExtensions.OffenseProperty { ArmorPenetration = 0, DefaultDamageTypeOverride = Battle.Damage.DamageType.Fire, BonusDamagePercent = 10 };

            var items = MBObjectManager.Instance.GetObjectTypeList<ItemObject>().Where(x=>x.IsTorItem());
            foreach(var item in items)
            {
                var info = new ExtendedItemObjectProperties(item.StringId);
                if(item.HasWeaponComponent)
                {
                    info.ItemDamageProperty = new Battle.Damage.ItemDamageProperty { DamageType = Battle.Damage.DamageType.Fire, MinDamage = 10, MaxDamage = 120 };
                    if(item.StringId == "tor_empire_weapon_sword_runefang_001")
                    {
                        info.Traits.Add(trait1);
                    }
                }
                _itemToInfoMap.Add(item.StringId, info);
            }
            var ser = new XmlSerializer(typeof(List<ExtendedItemObjectProperties>));
            var stream = File.OpenWrite(XMLPath);
            ser.Serialize(stream, _itemToInfoMap.Values.ToList());
            stream.Close();
        }
    }
}
