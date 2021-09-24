using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TOW_Core.Abilities
{
    public enum AbilityType
    {
        Innate,
        Spell,
        Prayer,
        ItemBound
    }

    public enum AbilityEffectType
    {
        MovingProjectile,
        DynamicProjectile,
        DirectionalMovingAOE, //i.e. wind
        RandomMovingAOE, //i.e. vortex
        CenteredStaticAOE,
        TargetedStaticAOE,
        TargetedStatic,
        Summoning
    }

    public enum AbilityTargetType
    {
        Self,
        Enemies,
        Allies,
        All
    }

    public enum CastType
    {
        Instant,
        WindUp,
        Channel
    }

    public enum TriggerType
    {
        EveryTick,
        OnCollision,
        TickOnce
    }

    public enum TargetType
    {
        Friendly,
        Enemy,
        All,
        FriendlyHero,
        EnemyHero
    }
}