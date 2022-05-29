using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Library;

namespace TOW_Core.CampaignSupport.Models
{
    public class TORMapWeatherModel : DefaultMapWeatherModel
    {
        public override AtmosphereInfo GetAtmosphereModel(CampaignTime timeOfYear, Vec3 pos)
        {
            var atmo = base.GetAtmosphereModel(timeOfYear, pos);
            atmo.TimeInfo.Season = CampaignTime.Now.GetSeasonOfYear;
            return atmo;
        }
    }
}
