using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.MountAndBlade.View.Missions.SiegeWeapon;

namespace TOW_Core.Battle.Artillery
{
    public class ArtilleryViewController : MissionView
    {
        public override void OnObjectUsed(Agent userAgent, UsableMissionObject usedObject)
        {
            base.OnObjectUsed(userAgent, usedObject);
			if (userAgent.IsMainAgent && usedObject is StandingPoint)
			{
				UsableMachine usableMachineFromPoint = this.GetUsableMachineFromPoint(usedObject as StandingPoint);
				if (usableMachineFromPoint is Artillery)
				{
					Artillery artillery = usableMachineFromPoint as Artillery;
					if (artillery.GetComponent<RangedSiegeWeaponView>() == null)
					{
						this.AddRangedSiegeWeaponView(artillery);
					}
				}
			}
		}

        private void AddRangedSiegeWeaponView(Artillery artillery)
        {
			ArtilleryView artilleryView = new ArtilleryView();
			artilleryView.Initialize(artillery, base.MissionScreen);
			artillery.AddComponent(artilleryView);
		}

        private UsableMachine GetUsableMachineFromPoint(StandingPoint standingPoint)
		{
			GameEntity gameEntity = standingPoint.GameEntity;
			while (gameEntity != null && !gameEntity.HasScriptOfType<UsableMachine>())
			{
				gameEntity = gameEntity.Parent;
			}
			if (gameEntity != null)
			{
				UsableMachine firstScriptOfType = gameEntity.GetFirstScriptOfType<UsableMachine>();
				if (firstScriptOfType != null)
				{
					return firstScriptOfType;
				}
			}
			return null;
		}
	}
}
