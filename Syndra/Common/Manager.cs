namespace Syndra.Common
{
    using EloBuddy;
    using LeagueSharp.Common;
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class Manager
    {

        public static Orbwalking.Orbwalker Orbwalker => Program.Orbwalker;
        public static bool InCombo => Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo;
        public static bool InLaneClear => Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear;
        public static bool InHarass => Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed;
        public static bool InNone => Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None;

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

        public static void WriteConsole(string Message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ObjectManager.Player.ChampionName + " : " + Message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static bool CanMovee(AIHeroClient target)
        {
            if (target.HasBuffOfType(BuffType.Stun)
                || target.HasBuffOfType(BuffType.Snare)
                || target.HasBuffOfType(BuffType.Knockup)
                || target.HasBuffOfType(BuffType.Charm)
                || target.HasBuffOfType(BuffType.Fear)
                || target.HasBuffOfType(BuffType.Knockback)
                || target.HasBuffOfType(BuffType.Taunt)
                || target.HasBuffOfType(BuffType.Suppression)
                || target.IsStunned || target.IsCastingInterruptableSpell()
                || target.MoveSpeed < 50f)
            {
                return false;
            }
            else
                return true;
        }

        public static bool CanKill(Obj_AI_Base Target)
        {
            if (Target.HasBuffOfType(BuffType.PhysicalImmunity)
                || Target.HasBuffOfType(BuffType.SpellImmunity)
                || Target.IsZombie
                || Target.IsInvulnerable
                || Target.HasBuffOfType(BuffType.Invulnerability)
                || Target.HasBuffOfType(BuffType.SpellShield)
                || Target.HasBuff("deathdefiedbuff")
                || Target.HasBuff("Undying Rage")
                || Target.HasBuff("Chrono Shift"))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void CastTo(this Spell Spells, Obj_AI_Base target, bool AOE = false, bool CastToPosition = false, Vector3 Position = new Vector3())
        {
            switch (Program.Menu.Item("SelectPred").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    {
                        if (CastToPosition)
                        {
                            Spells.Cast(Position);
                        }
                        else
                        {
                            Spells.Cast(target, false, AOE);
                        }
                    }
                    break;
                case 1:
                    {
                        var predInput = new PredictionInput
                        {
                            Aoe = AOE,
                            Collision = Spells.Collision,
                            Delay = Spells.Delay,
                            From = HeroManager.Player.ServerPosition,
                            Radius = Spells.Width,
                            Range = Spells.Range,
                            Speed = Spells.Speed,
                            Type = Spells.Type,
                            Unit = target
                        };

                        var predput = Prediction.GetPrediction(predInput);

                        if (Spells.Speed != float.MaxValue && YasuoWindWall.CollisionYasuo(HeroManager.Player.ServerPosition, predput.CastPosition))
                        {
                            return;
                        }

                        if (predput.Hitchance >= HitChance.VeryHigh)
                        {
                            Spells.Cast(predput.CastPosition);
                        }
                        else if (predInput.Aoe && predput.AoeTargetsHitCount > 1 && predput.Hitchance >= HitChance.High)
                        {
                            Spells.Cast(predput.CastPosition);
                        }
                    }
                    break;
                case 2:
                    {
                        LeagueSharp.SDK.Enumerations.SkillshotType CoreType2 = LeagueSharp.SDK.Enumerations.SkillshotType.SkillshotLine;

                        var predInput = new LeagueSharp.SDK.PredictionInput
                        {
                            AoE = AOE,
                            Collision = Spells.Collision,
                            Delay = Spells.Delay,
                            From = HeroManager.Player.ServerPosition,
                            Radius = Spells.Width,
                            Range = Spells.Range,
                            Speed = Spells.Speed,
                            Type = CoreType2,
                            Unit = target
                        };

                        var predput = LeagueSharp.SDK.Movement.GetPrediction(predInput);

                        if (Spells.Speed != float.MaxValue && YasuoWindWall.CollisionYasuo(HeroManager.Player.ServerPosition, predput.CastPosition))
                        {
                            return;
                        }

                        if (predput.Hitchance >= LeagueSharp.SDK.Enumerations.HitChance.VeryHigh)
                        {
                            Spells.Cast(predput.CastPosition);
                        }
                        else if (predInput.AoE && predput.AoeTargetsHitCount > 1 && predput.Hitchance >= LeagueSharp.SDK.Enumerations.HitChance.High)
                        {
                            Spells.Cast(predput.CastPosition);
                        }
                    }
                    break;
            }
        }

        public static bool Check(this Obj_AI_Base target, float range = float.MaxValue)
        {
            if (target == null ||target.IsDead || target.Health <= 0)
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

            if (target.HasBuff("UndyingRage") && target.GetBuff("UndyingRage").EndTime - Game.Time > 0.3 && target.Health <= target.MaxHealth * 0.10f)
            {
                return false;
            }

            if (target.HasBuff("JudicatorIntervention"))
            {
                return false;
            }

            if (target.HasBuff("ChronoShift") && target.GetBuff("ChronoShift").EndTime - Game.Time > 0.3 && target.Health <= target.MaxHealth * 0.10f)
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

            if (target.HasBuff("itemmagekillerveil"))
            {
                return false;
            }

            return !target.HasBuff("FioraW");
        }

        public static bool IsUnKillable(this Obj_AI_Base target)
        {
            if (target == null || target.IsDead || target.Health <= 0)
            {
                return true;
            }

            if (target.HasBuff("KindredRNoDeathBuff"))
            {
                return true;
            }

            if (target.HasBuff("UndyingRage") && target.GetBuff("UndyingRage").EndTime - Game.Time > 0.3 && target.Health <= target.MaxHealth * 0.10f)
            {
                return true;
            }

            if (target.HasBuff("JudicatorIntervention"))
            {
                return true;
            }

            if (target.HasBuff("ChronoShift") && target.GetBuff("ChronoShift").EndTime - Game.Time > 0.3 && target.Health <= target.MaxHealth * 0.10f)
            {
                return true;
            }

            if (target.HasBuff("ShroudofDarkness"))
            {
                return true;
            }

            if (target.HasBuff("SivirShield"))
            {
                return true;
            }

            if (target.HasBuff("itemmagekillerveil"))
            {
                return true;
            }

            return target.HasBuff("FioraW");
        }

    }

    internal class OnDamageEvent
    {
        public int Time;
        public float Damage;

        internal OnDamageEvent(int time, float damage)
        {
            Time = time;
            Damage = damage;
        }
    }
}