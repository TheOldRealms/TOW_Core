using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Scripts
{
    public class ShieldScript : AbilityScript
    {
        public override void Initialize(Ability ability)
        {
            _ability = ability;
            var frame = MatrixFrame.Identity;
            var shield = GameEntity.Instantiate(Scene, "magic_sphere", frame);
            var meta = shield.GetMetaMesh(0);
            var metaFrame = meta.Frame;
            metaFrame.Scale(new Vec3(1.5f, 1.5f, 1.5f));
            shield.SetFrame(ref metaFrame);
            GameEntity.AddChild(shield);
        }

        protected override void OnTick(float dt)
        {
            if (_ability == null) return;
            if (_isFading) return;
            _timeSinceLastTick += dt;
            UpdateLifeTime(dt);
            if (_casterAgent.Health > 0)
            {
                ChangePosition();
                CheckAndDestroy();
            }
            else
            {
                GameEntity.FadeOut(0.05f, true);
                _isFading = true;
            }
        }

        protected void ChangePosition()
        {
            Vec3 position = _casterAgent.GetEyeGlobalPosition();
            position.z -= 1;
            MatrixFrame frame = new MatrixFrame(Mat3.Identity, position);
            GameEntity.SetGlobalFrame(frame);
        }

        private void CheckAndDestroy()
        {
            var missiles = Mission.Current.Missiles.ToList();
            for (int i = 0; i < missiles.Count; i++)
            {
                var missile = missiles[i];
                if (missile != null)
                {
                    var distance = (GameEntity.GlobalPosition - missile.Entity.GlobalPosition).Length;
                    if (distance < 3f)
                    {
                        missile.Entity.FadeOut(0, true);
                        var frame = missile.Entity.GetFrame();
                        Mission.Current.AddParticleSystemBurstByName("psys_game_sparkle_a", frame, false);
                        Mission.Current.RemoveMissileAsClient(missile.Index);
                    }
                }
            }
        }
    }
}
