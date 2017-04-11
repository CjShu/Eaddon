namespace Syndra.Lib.Core
{
    using EloBuddy;
    using LeagueSharp.SDK;

    internal class Helper : Program
    {
        public static float TotalDamage(Obj_AI_Base enemy)
        {
            var damage = 0f;
            if (Q.IsReady())
            {
                damage += (float)Player.Instance.CalculateDamage(enemy, DamageType.Magical,
                    Q.GetDamage(enemy));
            }
            if (W.IsReady())
            {
                damage += (float)Player.Instance.CalculateDamage(enemy, DamageType.Magical,
                    W.GetDamage(enemy));
            }
            if (E.IsReady())
            {
                damage += (float)Player.Instance.CalculateDamage(enemy, DamageType.Magical,
                    E.GetDamage(enemy));
            }
            if (R.IsReady())
            {
                damage += (float)Player.Instance.CalculateDamage(enemy, DamageType.Magical,
                    R.GetDamage(enemy)) * 3;
            }
            return damage;
        }

    }
}