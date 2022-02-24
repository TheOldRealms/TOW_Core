namespace TOW_Core.Abilities
{
    public enum AbilityType
    {
        Innate,
        Spell,
        Prayer,
        ItemBound,
        SpecialMove
    }

    public enum AbilityEffectType
    {
        MovingProjectile,
        DynamicProjectile,
        DirectionalMovingAOE, //i.e. wind
        RandomMovingAOE, //i.e. vortex
        CenteredStaticAOE,
        TargetedStaticAOE,
        SingleTarget,
        Summoning,
        AgentMoving,
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

    //This is for triggeredeffects.
    public enum TargetType
    {
        Friendly,
        Enemy,
        All,
        FriendlyHero,
        EnemyHero
    }
}