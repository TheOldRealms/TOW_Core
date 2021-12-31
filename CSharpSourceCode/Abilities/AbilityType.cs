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
        TargetedStatic,
        Summoning,
        AgentMoving,
        TargetedMovingProjectile
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