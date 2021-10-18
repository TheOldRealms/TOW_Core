using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.TriggeredEffect;
using TOW_Core.Utilities;

namespace TOW_Core.Abilities.Scripts
{
    public class AgentMovingScript : AbilityScript
    {
        private MissionWeapon[] weapons = new MissionWeapon[4];

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            if (!_hasTriggered)
            {
                for (int i = 0; i < 3; i++)
                {
                    weapons[i] = _casterAgent.Equipment[i];
                    _casterAgent.RemoveEquippedWeapon((EquipmentIndex)i);
                }
                MakeInvisible(_casterAgent);
            }
            if (_isFading)
            {
                for (int i = 0; i < 3; i++)
                {
                    _casterAgent.EquipWeaponWithNewEntity((EquipmentIndex)i, ref weapons[i]);
                }
                MakeVisible(_casterAgent);
                return;
            }
            _timeSinceLastTick += dt;
            UpdateLifeTime(dt);

            var frame = GameEntity.GetGlobalFrame();
            frame.origin.z += 5; 
            UpdateSound(frame.origin);

            if (_ability.Template.TriggerType == TriggerType.EveryTick && _timeSinceLastTick > _ability.Template.TickInterval)
            {
                _timeSinceLastTick = 0;
                TriggerEffect(frame.origin, frame.origin.NormalizedCopy());
                _casterAgent.TeleportToPosition(frame.origin);
                _hasTriggered = true;
            }
            else if (_ability.Template.TriggerType == TriggerType.TickOnce && _abilityLife > _ability.Template.TickInterval && !_hasTriggered)
            {
                TriggerEffect(frame.origin, frame.origin.NormalizedCopy());
                _casterAgent.TeleportToPosition(frame.origin);
                _hasTriggered = true;
            }
            if (ShouldMove())
            {
                UpdatePosition(frame, dt);
            }
        }

        protected override MatrixFrame GetNextFrame(MatrixFrame oldFrame, float dt)
        {
            var frame = base.GetNextFrame(oldFrame, dt);
            var heightAtPosition = Mission.Current.Scene.GetGroundHeightAtPosition(frame.origin);
            frame.origin.z = heightAtPosition + base._ability.Template.Radius / 2;
            return frame;
        }

        private void MakeInvisible(Agent agent)
        {
            foreach (var mesh in agent.AgentVisuals.GetSkeleton().GetAllMeshes())
            {
                mesh.SetVisibilityMask(VisibilityMaskFlags.ShadowStatic);
            }
        }

        private void MakeVisible(Agent agent)
        {
            foreach (var mesh in agent.AgentVisuals.GetSkeleton().GetAllMeshes())
            {
                mesh.SetVisibilityMask(VisibilityMaskFlags.Default);
            }
        }
    }
}
