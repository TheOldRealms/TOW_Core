using SandBox.Missions.MissionLogics;
using System;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.TriggeredEffect.Scripts
{
    public class HandGrenadeScript : BlackPowderWeaponScript
    {
        private bool _hasExploded = false;
        private bool _hasLaunched = false;
        private Int32 _explosionTimer = 0;
        private SoundEvent _tickSound;

        protected override void OnInit()
        {
            base.OnInit();
            SetScriptComponentToTick(GetTickRequirement());
        }
        
        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            _explosionTimer++;
            if (_tickSound == null)
            {
                Int32 _tickSoundindex = SoundEvent.GetEventIdFromString("dwarf_hand_grenade_tick");
                _tickSound = SoundEvent.CreateEvent(_tickSoundindex, Mission.Current.Scene);
                _tickSound.SetPosition(GameEntity.GlobalPosition);
            }
            _tickSound.SetPosition(GameEntity.GlobalPosition);
            if (!_hasLaunched)
            {
                _hasLaunched = true;
                _tickSound.Play();
            }
            if (_hasLaunched && _explosionTimer % 40 == 0)
                _tickSound.Play();
            if (_explosionTimer >= 135 && !_hasExploded)
            {
                _hasExploded = true;
                _tickSound.Release();
                _explosion.Trigger(GameEntity.GlobalPosition, Vec3.Zero, _shooterAgent);
                GameEntity.FadeOut(0.5f, true);

                // alarm enemies
                var spawnLogic = Mission.Current.GetMissionBehavior<HideoutMissionController>();
                if (spawnLogic != null)
                {
                    foreach (var agent in Mission.Current.PlayerEnemyTeam.TeamAgents)
                    {
                        spawnLogic.OnAgentAlarmedStateChanged(agent, Agent.AIStateFlag.Alarmed);
                        agent.SetWatchState(Agent.WatchState.Alarmed);
                    }
                }
            }
        }
        
        protected override void OnRemoved(int removeReason)
        {
            base.OnRemoved(removeReason);
            _tickSound.Release();
        }
    }
}
