using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    public enum DamageType
    {
        BasicAttack,
        Skill,
        Ultimate
    }

    public struct DamageResult
    {
        public Entity Attacker { get; set; }
        public Entity Target { get; set; }
        public float RawDamage { get; set; }
        public float FinalDamage { get; set; }
        public bool IsCrit { get; set; }
        public bool IsDodged { get; set; }
        public DamageType DamageType { get; set; }

        public DamageResult(Entity attacker, Entity target, float rawDamage, float finalDamage, bool isCrit, bool isDodged, DamageType damageType)
        {
            Attacker = attacker;
            Target = target;
            RawDamage = rawDamage;
            FinalDamage = finalDamage;
            IsCrit = isCrit;
            IsDodged = isDodged;
            DamageType = damageType;
        }
    }
}
