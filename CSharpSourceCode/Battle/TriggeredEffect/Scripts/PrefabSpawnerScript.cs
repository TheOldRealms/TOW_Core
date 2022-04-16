using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.TriggeredEffect.Scripts
{
    public class PrefabSpawnerScript : ITriggeredScript
    {
        public string PrefabName { get; private set; }

        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents)
        {
            SpawnPrefab(position, triggeredByAgent);
        }

        private void SpawnPrefab(Vec3 position, Agent triggeredByAgent)
        {
            var team = Mission.Current.Teams.FirstOrDefault(x => x != triggeredByAgent.Team);
            var target = team.Formations.First().GetFirstUnit().Position;
            var direction = (target - position).NormalizedCopy();
            var rotation = Mat3.CreateMat3WithForward(-direction);
            var entity = GameEntity.Instantiate(Mission.Current.Scene, PrefabName, true);
            entity.SetMobility(GameEntity.Mobility.dynamic);
            entity.EntityFlags = (entity.EntityFlags | EntityFlags.DontSaveToScene);
            var frame = new MatrixFrame(rotation, position);
            entity.SetGlobalFrame(frame);
            var artillery = entity.GetFirstScriptInFamilyDescending<Artillery.ArtilleryRangedSiegeWeapon>();
            if (artillery != null)
            {
                artillery.SetSide(triggeredByAgent.Team.Side);
                artillery.Team = triggeredByAgent.Team;
                artillery.ForcedUse = !triggeredByAgent.Team.IsPlayerTeam;
            }
        }

        internal void OnInit(string spawnPrefabName)
        {
            this.PrefabName = spawnPrefabName;
        }
    }
}