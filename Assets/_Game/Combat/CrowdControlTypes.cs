namespace WuxiaGame.Combat
{
    public enum CrowdControlType
    {
        None = 0,
        Stun = 1,
        Root = 2,
        Freeze = 3
    }

    public enum EffectPowerTier
    {
        None = 0,
        TierA = 1, // Minor
        TierB = 2, // Standard
        TierC = 3  // Major
    }

    public enum DebuffType
    {
        Attack = 1,
        Defense = 2,
        AttackSpeed = 3,
        MoveSpeed = 4,
        CritRate = 5,
        DodgeRate = 6,
        RageGain = 7,
        HealingReceived = 8,
        DamageDealt = 9
    }

    public enum DebuffModifierMode
    {
        Flat = 1,
        Percentage = 2
    }
}
