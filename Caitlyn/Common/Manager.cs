namespace Caitlyn.Common
{
    using EloBuddy;
    using LeagueSharp.Common;
    using SharpDX;
    using System.Collections.Generic;
    using System.Linq;
    using System;

    public static class Manager
    {
        public static Orbwalking.Orbwalker Orbwalker => Program.Orbwalker;
        public static bool InCombo => Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo;

        public static bool Check(this Obj_AI_Base target, float range = float.MaxValue)
        {
            if (target == null)
            {
                return false;
            }

            if (target.Distance(ObjectManager.Player) > range)
            {
                return false;
            }

            if (target.HasBuff("KindredRNoDeathBuff"))
            {
                return false;
            }

            if (target.HasBuff("UndyingRage") && target.GetBuff("UndyingRage").EndTime - Game.Time > 0.3)
            {
                return false;
            }

            if (target.HasBuff("JudicatorIntervention"))
            {
                return false;
            }

            if (target.HasBuff("ChronoShift") && target.GetBuff("ChronoShift").EndTime - Game.Time > 0.3)
            {
                return false;
            }

            if (target.HasBuff("ShroudofDarkness"))
            {
                return false;
            }

            if (target.HasBuff("SivirShield"))
            {
                return false;
            }

            return !target.HasBuff("FioraW");
        }

        public static bool InAutoAttackRange(AttackableUnit target)
        {
            var baseTarget = (Obj_AI_Base)target;
            var myRange = GetAttackRange(ObjectManager.Player);

            if (baseTarget != null)
            {
                return baseTarget.IsHPBarRendered &&
                    Vector2.DistanceSquared(baseTarget.ServerPosition.To2D(),
                    ObjectManager.Player.ServerPosition.To2D()) <= myRange * myRange;
            }

            return target.IsValidTarget() &&
                Vector2.DistanceSquared(target.Position.To2D(),
                ObjectManager.Player.ServerPosition.To2D())
                <= myRange * myRange;
        }

        public static float GetAttackRange(Obj_AI_Base target)
        {
            if (target != null)
            {
                return Orbwalking.GetRealAutoAttackRange();
            }
            else
                return 0f;
        }

        public static bool CanMove(this AIHeroClient target)
        {
            return !(target.MoveSpeed < 50) && !target.IsStunned && !target.HasBuffOfType(BuffType.Stun) &&
                   !target.HasBuffOfType(BuffType.Fear) && !target.HasBuffOfType(BuffType.Snare) &&
                   !target.HasBuffOfType(BuffType.Knockup) && !target.HasBuff("Recall") &&
                   !target.HasBuffOfType(BuffType.Knockback)
                   && !target.HasBuffOfType(BuffType.Charm) && !target.HasBuffOfType(BuffType.Taunt) &&
                   !target.HasBuffOfType(BuffType.Suppression) && (!target.IsCastingInterruptableSpell()
                                                                   || target.IsMoving) &&
                   !target.HasBuff("zhonyasringshield") && !target.HasBuff("bardrstasis");
        }

        /// <summary>
        /// Check Target
        /// </summary>
        /// <param name="Target">Target</param>
        /// <returns></returns>
        public static bool CheckTarget(AIHeroClient Target)
        {
            if (Target != null && !Target.IsDead && !Target.IsZombie && Target.IsHPBarRendered)
            {
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Send Message To Console
        /// </summary>
        /// <param name="Message"></param>
        public static void WriteConsole(string Message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ObjectManager.Player.ChampionName + " : " + Message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static int GetCustomDamage(this AIHeroClient source, string auraname, AIHeroClient target)
        {
            if (auraname == "sheen")
            {
                return
                    (int)
                        source.CalcDamage(target, Damage.DamageType.Physical,
                            1.0 * source.FlatPhysicalDamageMod + source.BaseAttackDamage);
            }

            if (auraname == "lichbane")
            {
                return
                    (int)
                        source.CalcDamage(target, Damage.DamageType.Magical,
                            (0.75 * source.FlatPhysicalDamageMod + source.BaseAttackDamage) +
                            (0.50 * source.FlatMagicDamageMod));
            }

            return 0;
        }
    }
}