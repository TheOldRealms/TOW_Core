using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.TriggeredEffect.Scripts
{
    public class SmokingLimbScript : ScriptComponentBehavior
    {
        private float _liveTime = 0;
        private float _endTime = 0;
        private GameEntity _smoke;

        protected override void OnInit()
        {
            MatrixFrame frame = MatrixFrame.Identity;
            _smoke = GameEntity.CreateEmpty(Scene);
            _smoke.SetFrame(ref frame);
            ParticleSystem.CreateParticleSystemAttachedToEntity("psys_burnt_limb_smoke", _smoke, ref frame);
            GameEntity.AddChild(_smoke);
            _endTime = MBRandom.RandomFloatRanged(10, 15);
            SetScriptComponentToTick(TickRequirement.Tick);
        }

        protected override void OnTick(float dt)
        {
            if (_liveTime < _endTime)
            {
                _liveTime += dt;
            }
            else
            {
                GameEntity.RemoveAllChildren();
                _smoke.FadeOut(0.5f, true);
                SetScriptComponentToTick(TickRequirement.None);
            }
        }
    }
}
