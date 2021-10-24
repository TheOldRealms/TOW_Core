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
            var entity = GameEntity.Instantiate(Mission.Current.Scene, PrefabName, true);
            entity.SetMobility(GameEntity.Mobility.dynamic);
            entity.EntityFlags = (entity.EntityFlags | EntityFlags.DontSaveToScene);
            MatrixFrame identity = new MatrixFrame(Mat3.Identity, position);
            entity.SetFrame(ref identity);
        }

        internal void OnInit(string spawnPrefabName)
        {
            this.PrefabName = spawnPrefabName;
        }
    }
}
