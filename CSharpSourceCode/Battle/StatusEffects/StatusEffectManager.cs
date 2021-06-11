using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using TaleWorlds.ModuleManager;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.StatusEffects
{
    public class StatusEffectManager
    {
        private readonly string ModuleName = "TOW_Core";
        private readonly string EffectsFileName = "tow_statuseffects.xml";
        private static Dictionary<string, StatusEffect> _idToStatusEffect = new Dictionary<string, StatusEffect>();

        public StatusEffectManager()
        {

        }

        public void LoadStatusEffects()
        {
            var files = Directory.GetFiles(ModuleHelper.GetModuleFullPath(ModuleName), EffectsFileName, SearchOption.AllDirectories);
            XmlSerializer serializer = new XmlSerializer(typeof(List<StatusEffect>), new XmlRootAttribute("StatusEffects"));
            List<StatusEffect> effectList;
            using(FileStream fs = new FileStream(files[0], FileMode.Open))
            {
                effectList = (List<StatusEffect>)serializer.Deserialize(fs);
            }
            foreach(StatusEffect effect in effectList)
            {
                _idToStatusEffect.Add(effect.Id, effect);
            }
        }

        public static StatusEffect GetStatusEffect(string effectId)
        {
            return _idToStatusEffect[effectId];
        }
    }
}
