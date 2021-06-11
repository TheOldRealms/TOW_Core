using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle;
using TOW_Core.Battle.Extensions;
using TOW_Core.Utilities;

namespace TOW_Core.Abilities
{
    public class WindOfDeathAbilityScript : ScriptComponentBehaviour
    {
        private Agent _casterAgent;
        private WindOfDeathAbility _ability;
        private float _speed = 8f;
        private float _abilitylife = -1f;
        private bool _isFading;
        private float _damageInterval = 0.5f;
        private float _timeSinceLastDamage = 0f;
        private float _range = 3.5f;
        private int _damageMin = 50;
        private int _damageMax = 80;

        protected override TickRequirement GetTickRequirement()
        {
            return TickRequirement.Tick;
        }
        protected override bool MovesEntity() => true;
        public void SetAgent(Agent agent) => _casterAgent = agent;
        public void SetAbility(WindOfDeathAbility ability) => _ability = ability;

        protected override void OnInit()
        {
            base.OnInit();
            SetScriptComponentToTick(GetTickRequirement());
        }

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            if (_timeSinceLastDamage > _damageInterval) DamageAgents();
            _timeSinceLastDamage += dt;
            var frame = base.GameEntity.GetGlobalFrame();
            var newframe = frame.Advance(_speed * dt);
            var height = Mission.Current.Scene.GetTerrainHeight(frame.origin.AsVec2);
            newframe.origin.z = height;
            base.GameEntity.SetGlobalFrame(newframe);

            if (_abilitylife < 0)
            {
                _abilitylife = 0;
            }
            else
            {
                _abilitylife += dt;
            }
            if (_ability != null)
            {
                if (_abilitylife > _ability.MaxDuration && !_isFading)
                {
                    base.GameEntity.FadeOut(0.1f, true);
                    _isFading = true;
                }
            }
        }

        private void DamageAgents()
        {
            _timeSinceLastDamage = 0f;
            TOWBattleUtilities.DamageAgentsInArea(base.GameEntity.GetGlobalFrame().origin.AsVec2, _range, _damageMin, _damageMax,  _casterAgent);
        }
    }
}
