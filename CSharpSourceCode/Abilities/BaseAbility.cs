using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using System.Xml.Serialization;
using System.Timers;
using System.Xml.Schema;
using System.Xml;

namespace TOW_Core.Abilities
{
    [Serializable]
    public abstract class BaseAbility
    {
        public string Name { get; protected set; } = "";
        public string SpriteName { get; protected set; } = "";
        public int CoolDown { get; protected set; } = 10;
        public float MaxDuration { get; protected set; } = 3f;
        private int _coolDownLeft = 0;
        private Timer _timer = null;

        public bool IsOnCooldown()
        {
            return this._timer.Enabled;
        }

        public int GetCoolDownLeft()
        {
            return this._coolDownLeft;
        }

        public BaseAbility()
        {
            this._timer = new Timer(1000);
            this._timer.Elapsed += TimerElapsed;
            this._timer.Enabled = false;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            this._coolDownLeft -= 1;
            if(this._coolDownLeft <= 0)
            {
                this._coolDownLeft = 0;
                this._timer.Stop();
            }
        }

        public void Use(Agent casterAgent)
        {
            if (!this.IsOnCooldown())
            {
                this._coolDownLeft = this.CoolDown;
                this._timer.Start();
                OnUse(casterAgent);
            }
        }

        protected virtual void OnUse(Agent casterAgent) { }
    }
}
