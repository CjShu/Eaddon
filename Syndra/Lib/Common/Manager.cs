namespace Syndra.Lib.Common
{
    using EloBuddy;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.UI;
    using LeagueSharp.SDK.Utils;
    using LeagueSharp.SDK.Enumerations;
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class Manager
    {
        private static AIHeroClient player => Player.Instance;

        public static string[] AutoEnableList =
        {
             "Annie", "Ahri", "Akali", "Anivia", "Annie", "Brand", "Cassiopeia", "Diana", "Evelynn", "FiddleSticks", "Fizz", "Gragas", "Heimerdinger", "Karthus",
             "Kassadin", "Katarina", "Kayle", "Kennen", "Kled", "Leblanc", "Lissandra", "Lux", "Malzahar", "Mordekaiser", "Morgana", "Nidalee", "Orianna",
             "Ryze", "Sion", "Swain", "Syndra", "Teemo", "TwistedFate", "Veigar", "Viktor", "Vladimir", "Xerath", "Ziggs", "Zyra", "Velkoz", "Azir", "Ekko",
             "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves", "Jayce", "Jinx", "KogMaw", "Lucian", "MasterYi", "MissFortune", "Quinn", "Shaco", "Sivir",
             "Talon", "Tristana", "Twitch", "Urgot", "Varus", "Vayne", "Yasuo", "Zed", "Kindred", "AurelionSol"
        };


        public static List<Obj_AI_Minion> GetMinions(Vector3 From, float Range)
        {
            return GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Range, false, @From)).ToList();
        }

        public static List<Obj_AI_Minion> GetMobs(Vector3 From, float Range, bool OnlyBig = false)
        {
            if (OnlyBig)
            {
                return GameObjects.Jungle.Where(x => x.IsValidTarget(Range, false, @From) && (x.Name.Contains("Crab") || !GameObjects.JungleSmall.Contains(x))).ToList();
            }
            else
                return GameObjects.Jungle.Where(x => x.IsValidTarget(Range, false, @From)).ToList();
        }

        public static List<AIHeroClient> GetEnemies(float Range)
        {
            return GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Range) && x.IsEnemy && !x.IsZombie && !x.IsDead).ToList();
        }

        public static AIHeroClient GetTarget(float Range, DamageType DamageType = DamageType.Physical)
        {
            return Variables.TargetSelector.GetTarget(Range, DamageType);
        }

        public static AIHeroClient GetTarget(Spell Spell, bool Ignote = true)
        {
            return Variables.TargetSelector.GetTarget(Spell, Ignote);
        }
      
        public static bool InAutoAttackRange(AttackableUnit target)
        {
            var baseTarget = (Obj_AI_Base)target;
            var myRange = GetAttackRange(GameObjects.Player);

            if (baseTarget != null)
            {
                return baseTarget.IsHPBarRendered &&
                    Vector2.DistanceSquared(baseTarget.ServerPosition.ToVector2(),
                    Player.Instance.ServerPosition.ToVector2()) <= myRange * myRange;
            }

            return target.IsValidTarget() &&
                Vector2.DistanceSquared(target.Position.ToVector2(),
                Player.Instance.ServerPosition.ToVector2())
                <= myRange * myRange;
        }

        public static float GetAttackRange(Obj_AI_Base Target)
        {
            if (Target != null)
            {
                return Target.GetRealAutoAttackRange();
            }
            else
                return 0f;
        }

        /// <summary>
        /// Judge Target MoveMent Status (This Part From SebbyLib)
        /// </summary>
        /// <param name="Target">Target</param>
        /// <returns></returns>
        public static bool CanMove(AIHeroClient Target)
        {
            if (Target.MoveSpeed < 50 || Target.IsStunned || Target.HasBuffOfType(BuffType.Stun) ||
                Target.HasBuffOfType(BuffType.Fear) || Target.HasBuffOfType(BuffType.Snare) ||
                Target.HasBuffOfType(BuffType.Knockup) || Target.HasBuff("Recall") ||
                Target.HasBuffOfType(BuffType.Knockback) || Target.HasBuffOfType(BuffType.Charm) ||
                Target.HasBuffOfType(BuffType.Taunt) || Target.HasBuffOfType(BuffType.Suppression)
                || (Target.IsCastingInterruptableSpell() && !Target.IsMoving))
            {
                return false;
            }
            else
                return true;
        }

        public static void WriteConsole(string Message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(GameObjects.Player.ChampionName + " : " + Message);
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
            switch (Program.Menu["Pred"]["SelectPred"].GetValue<MenuList>().Index)
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
                            AoE = AOE,
                            Collision = Spells.Collision,
                            Delay = Spells.Delay,
                            From = Player.Instance.ServerPosition,
                            Radius = Spells.Width,
                            Range = Spells.Range,
                            Speed = Spells.Speed,
                            Type = Spells.Type,
                            Unit = target
                        };

                        var predput = Movement.GetPrediction(predInput);

                        if (Spells.Speed != float.MaxValue && YasuoWindWall.CollisionYasuo(Player.Instance.ServerPosition, predput.CastPosition))
                        {
                            return;
                        }

                        if (predput.Hitchance >= HitChance.VeryHigh)
                        {
                            Spells.Cast(predput.CastPosition);
                        }
                        else if (predInput.AoE && predput.AoeTargetsHitCount > 1 && predput.Hitchance >= HitChance.High)
                        {
                            Spells.Cast(predput.CastPosition);
                        }
                    }
                    break;
            }
        }

        public static bool CheckTarget(AIHeroClient Target)
        {
            if (Target != null && !Target.IsDead && !Target.IsZombie && Target.IsHPBarRendered)
            {
                return true;
            }
            else
                return false;
        }

        public static double GetDamage(AIHeroClient Target, bool CalCulateAttackDamage = true,
            bool CalCulateQDamage = true, bool CalCulateWDamage = true,
            bool CalCulateEDamage = true, bool CalCulateRDamage = true)
        {
            if (CheckTarget(Target))
            {
                double Damage = 0d;

                if (CalCulateAttackDamage)
                {
                    Damage += GameObjects.Player.GetAutoAttackDamage(Target);
                }

                if (CalCulateQDamage)
                {
                    Damage += GameObjects.Player.Spellbook.GetSpell(SpellSlot.Q).IsReady() ? GameObjects.Player.GetSpellDamage(Target, SpellSlot.Q) : 0d;
                }

                if (CalCulateWDamage)
                {
                    Damage += GameObjects.Player.Spellbook.GetSpell(SpellSlot.W).IsReady() ? GameObjects.Player.GetSpellDamage(Target, SpellSlot.W) : 0d;
                }

                if (CalCulateEDamage)
                {
                    Damage += GameObjects.Player.Spellbook.GetSpell(SpellSlot.E).IsReady() ? GameObjects.Player.GetSpellDamage(Target, SpellSlot.E) : 0d;
                }

                if (CalCulateRDamage)
                {
                    Damage += GameObjects.Player.Spellbook.GetSpell(SpellSlot.R).IsReady() ? GameObjects.Player.GetSpellDamage(Target, SpellSlot.R) : 0d;
                }

                // exhaust
                if (GameObjects.Player.HasBuff("SummonerExhaust"))
                    Damage = Damage * 0.6f;

                // blitzcrank passive
                if (Target.HasBuff("BlitzcrankManaBarrierCD") && Target.HasBuff("ManaBarrier"))
                    Damage -= Target.Mana / 2f;

                // kindred r
                if (Target.HasBuff("KindredRNoDeathBuff"))
                    Damage = 0;

                // tryndamere r
                if (Target.HasBuff("UndyingRage") && Target.GetBuff("UndyingRage").EndTime - Game.Time > 0.3)
                    Damage = 0;

                // kayle r
                if (Target.HasBuff("JudicatorIntervention"))
                    Damage = 0;

                // zilean r
                if (Target.HasBuff("ChronoShift") && Target.GetBuff("ChronoShift").EndTime - Game.Time > 0.3)
                    Damage = 0;

                // fiora w
                if (Target.HasBuff("FioraW"))
                    Damage = 0;

                return Damage;
            }
            else
            {
                return 0d;
            }
        }

        public static int GetCustomDamage(this AIHeroClient source, string auraname, AIHeroClient target)
        {
            if (auraname == "sheen")
            {
                return
                    (int)
                        source.CalculateDamage(target, DamageType.Physical,
                            1.0 * source.FlatPhysicalDamageMod + source.BaseAttackDamage);
            }

            if (auraname == "lichbane")
            {
                return
                    (int)
                        source.CalculateDamage(target, DamageType.Magical,
                            (0.75 * source.FlatPhysicalDamageMod + source.BaseAttackDamage) +
                            (0.50 * source.FlatMagicDamageMod));
            }

            return 0;
        }

        public static bool SpellCollision(AIHeroClient t, Spell spell, int extraWith = 50)
        {
            foreach (var hero in GameObjects.EnemyHeroes.Where(hero => hero.IsValidTarget(spell.Range + spell.Width, true, spell.RangeCheckFrom) && t.NetworkId != hero.NetworkId))
            {
                var prediction = spell.GetPrediction(hero);
                var powCalc = Math.Pow((spell.Width + extraWith + hero.BoundingRadius), 2);
                if (prediction.UnitPosition.ToVector2().DistanceSquared(spell.From.ToVector2(), spell.GetPrediction(t).CastPosition.ToVector2(), true) <= powCalc)
                {
                    return true;
                }
                else if (prediction.UnitPosition.ToVector2().Distance(spell.From.ToVector2(), t.ServerPosition.ToVector2(), true) <= powCalc)
                {
                    return true;
                }

            }
            return false;
        }

        /// <summary>
        /// (Sebby Lib)
        /// </summary>
        /// <returns></returns>
        public static bool CanHarras()
        {
            if (!player.IsAttackingPlayer && !player.IsUnderEnemyTurret() && Variables.Orbwalker.CanMove)
                return true;
            else
                return false;
        }

        #region 模式

        public static bool Combo
        {
            get
            {
                return Variables.Orbwalker.ActiveMode == OrbwalkingMode.Combo;
            }
        }

        public static bool Harass
        {
            get
            {
                return Variables.Orbwalker.ActiveMode == OrbwalkingMode.Hybrid;
            }
        }

        public static bool LaneClear
        {
            get
            {
                return Variables.Orbwalker.ActiveMode == OrbwalkingMode.LaneClear;
            }
        }

        public static bool Hit
        {
            get
            {
                return Variables.Orbwalker.ActiveMode == OrbwalkingMode.LastHit;
            }
        }

        public static bool None
        {
            get
            {
                return Variables.Orbwalker.ActiveMode == OrbwalkingMode.None;
            }
        }

        #endregion

        #region BUFF

        public static readonly Dictionary<int, List<OnDamageEvent>> DamagesOnTime = new Dictionary<int, List<OnDamageEvent>>();

        public static bool CanKillableWith(this Obj_AI_Base t, Spell spell)
        {
            return t.Health < spell.GetDamage(t) - 5;
        }

        public static bool HasBuffInst(this Obj_AI_Base obj, string buffName)
        {
            return obj.Buffs.Any(buff => buff.DisplayName == buffName);
        }

        public static bool HasPassive(this Obj_AI_Base obj)
        {
            return obj.PassiveCooldownEndTime - (Game.Time - 15.5) <= 0;
        }

        public static bool HasBlueBuff(this Obj_AI_Base obj)
        {
            return obj.Buffs.Any(buff => buff.DisplayName == "CrestoftheAncientGolem");
        }

        public static bool HasRedBuff(this Obj_AI_Base obj)
        {
            return obj.Buffs.Any(buff => buff.DisplayName == "BlessingoftheLizardElder");
        }

        #endregion
    }

    internal class BlueBuff
    {
        public static float StartTime { get; set; }
        public static float EndTime { get; set; }
    }

    internal class RedBuff
    {
        public static float StartTime { get; set; }
        public static float EndTime { get; set; }
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